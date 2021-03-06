#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DirectChat
{
    internal class P2PSocket
    {
        public readonly bool Booted;
        public EndPoint? RemoteEndPoint;
        private Socket? _internalSocket;
        private Socket? _socket
        {
            set
            {
                _internalSocket = value;
                if (value != null) RemoteEndPoint = value.RemoteEndPoint;
            }
            get => _internalSocket;
        }
        private Socket? _hostSocket;
        private byte[]? _buffer;
        private readonly ConnectionMeta _config;
        private readonly TrialWrapper? _sendCallback;
        private readonly TrialWrapper _acceptCb;
        private readonly TrialWrapper _receiveCb;
        private readonly TrialWrapper _connectCb;
        private Action<byte[]>? _onReceived;
        private Action<EndPoint>? _onDisconnect;
        private readonly Action<P2PSocket>? _onAccept;

        public P2PSocket(ConnectionMeta config, Action<byte[]> onReceivedHook, Action<P2PSocket> onAccept, Action<EndPoint> onDisconnectHook, Socket? socket = null)
        {
            _config = config;
            _connectCb = new TrialWrapper(Connect);
            _acceptCb = new TrialWrapper(Accept);
            _receiveCb = new TrialWrapper(Receive);
            Booted = ImplantSocket(socket) || new TrialWrapper(SSetup).Proof();
            if (!Booted) return;
            _onReceived = onReceivedHook;
            _onAccept = onAccept;
            _onDisconnect = onDisconnectHook;
            _sendCallback = new TrialWrapper(Send);
        }

        public P2PSocket Reattach(Action<byte[]> onReceivedHook, Action<EndPoint>? onDisconnectHook)
        {
            _onReceived = onReceivedHook;
            _onDisconnect = onDisconnectHook ?? _onDisconnect;
            return this;
        }

        private bool ImplantSocket(Socket? socket)
        {
            if (socket == null) return false;
            _socket = socket;
            _buffer = new byte[_socket.ReceiveBufferSize];
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, _receiveCb.Exec, null);
            return true;
        }

        public void Shutdown()
        {
            if (_socket == null) return;
            try
            {
                if (_socket.Connected)
                    _socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                _socket.Close();
                _socket = null;
            }
        }

        private void SSetup(IAsyncResult? ar = null)
        {
            var host = _config.host;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(
                host ? IPAddress.Any : IPAddress.Parse(_config.ip),
                host ? _config.port : _config.remotePort);
            if (!host)
            {
                _socket = socket;
                _socket.BeginConnect(endPoint, _connectCb.Exec, null);
                return;
            }
            _hostSocket = socket;
            _hostSocket.Bind(endPoint);
            _hostSocket.Listen(10);
            _hostSocket.BeginAccept(_acceptCb.Exec, null);
        }

        private void Accept(IAsyncResult AR)
        {
            // ImplantSocket(_hostSocket!.EndAccept(AR));
            // it os host's responsibility to enforce protocols
            _onAccept!(new P2PSocket(_config, _onReceived!, _onAccept, _onDisconnect!, _hostSocket!.EndAccept(AR)));
            _hostSocket.BeginAccept(_acceptCb.Exec, null);
        }

        private void Connect(IAsyncResult AR)
        {
            _socket!.EndConnect(AR);
            ImplantSocket(_socket);
        }

        private void Receive(IAsyncResult AR)
        {
            try
            {
                if (_socket!.EndReceive(AR) == 0)
                {
                    HandleDisconnect();
                    return;
                }
            }
            catch
            {
                HandleDisconnect();
                return;
            }
            
            _onReceived!(_buffer!);
            _socket.BeginReceive(_buffer!, 0, _buffer!.Length, SocketFlags.None, _receiveCb.Exec, null);
        }

        private void Send(IAsyncResult ar)
        {
            _socket!.EndSend(ar);
        }

        public bool SendData(byte[] rawData)
        {
            try
            {
                _socket!.BeginSend(rawData, 0, rawData.Length, SocketFlags.None, _sendCallback!.Exec, null);
            }
            catch (Exception e) when (e is SocketException || e is ObjectDisposedException)
            {
                MessageBox.Show(e.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void HandleDisconnect()
        {
            Shutdown();
            _onDisconnect!(RemoteEndPoint!);
        }

    }


    internal class TrialWrapper
    {
        private readonly dynamic _fn;

        public TrialWrapper(Action<IAsyncResult> fn)
        {
            _fn = fn;
        }
        public TrialWrapper(Func<IAsyncResult, int> fn)
        {
            _fn = fn;
        }


        public void Exec(IAsyncResult ar)
        {
            try
            {
                _fn(ar);
            }
            catch (Exception e) when (e is SocketException || e is ObjectDisposedException)
            {
                MessageBox.Show(e.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool Proof(IAsyncResult ar = null)
        {
            try
            {
                _fn(ar);
            }
            catch (Exception e) when (e is SocketException || e is ObjectDisposedException)
            {
                MessageBox.Show(e.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}
