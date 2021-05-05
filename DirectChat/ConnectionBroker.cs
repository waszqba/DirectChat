using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DirectChat
{
    /// <summary>
    /// This class has potential to serve multiple connections and chats in the future.
    /// It also dispatches socket messages to appropriate handlers
    /// and enforces encryption of user messages when RSA is contracted
    /// </summary>
    internal class ConnectionBroker
    {
        private readonly Action<byte[], EndPoint> _usrMsg;
        private readonly Action<EndPoint> _disconnected;
        private readonly Dictionary<string, Connection> _connections = new();
        public bool Booted => _connections.Count > 0 && _connections["0"].Booted;

        public ConnectionBroker(ConnectionMeta config, Action<byte[], EndPoint> userMsgHook, Action<EndPoint> disconnectHook)
        {
            _connections.Add("0", new Connection(new P2PSocket(config, Received, OnAccept, disconnectHook)));
            _usrMsg = userMsgHook;
            _disconnected = disconnectHook;
        }

        private void Received(byte[] rawBytes)
        {
        }

        public void NewConvo(ConnectionMeta config)
        {
            _connections.Add(new IPEndPoint(IPAddress.Parse(config.ip), config.remotePort).ToString(),
                new Connection(new P2PSocket(config, Received, OnAccept, _disconnected), _usrMsg, _disconnected));
        }

        public void SendUsrMsg(byte[] msg, string connectionIndex) => _connections[connectionIndex].SendUsrMsg(msg);

        private void OnAccept(P2PSocket socket)
        {
            _connections.Add(socket.RemoteEndPoint!.ToString()!, new Connection(socket, _usrMsg, _disconnected));
        }
    }

    internal class Connection
    {
        private readonly P2PSocket _socket;
        private readonly CryptoBroker _rsa = new();
        private readonly Action<byte[], EndPoint>? _usrMsg;
        private EndPoint? RemoteEndPoint => _socket.RemoteEndPoint;
        public bool Booted => _socket.Booted;

        public Connection(P2PSocket socket, Action<byte[], EndPoint>? userMsgHook = null, Action<EndPoint>? disconnectHook = null)
        {
            _socket = socket;
            _usrMsg = userMsgHook;
            _socket.Reattach(DispatchMessage, disconnectHook);
            if (disconnectHook == null) return; // only host socket is allowed to do that
            _socket.SendData(new SocketMessage(true, _rsa.PublicKey).Raw);
        }

        private void RSAContract(byte[] guestKey)
        {
            if (_rsa.Contracted) return;
            var key = _rsa.Handshake(guestKey);
            _socket.SendData(new SocketMessage(true, key).Raw);
        }

        public void SendUsrMsg(byte[] msg)
        {
            var toSend = _rsa.Contracted ? _rsa.Encrypt(msg) : msg;
            _socket.SendData(new SocketMessage(false, toSend).Raw);
        }

        private void HandleUsrMsg(byte[] message)
        {
            _usrMsg?.Invoke(_rsa.Contracted ? _rsa.Decrypt(message) : message, RemoteEndPoint!);
        }

        private void DispatchMessage(byte[] rawBytes)
        {
            var msg = new SocketMessage(rawBytes);
            if (msg.IsCrypto)
                RSAContract(msg.Message);
            else
                HandleUsrMsg(msg.Message);
        }
    }
}
