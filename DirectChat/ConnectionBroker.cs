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
    class ConnectionBroker
    {
        private readonly P2PSocket _socket;
        private readonly CryptoBroker _rsa = new();
        private readonly Action<byte[]> _usrMsg;
        private readonly List<Connection> _connections = new();
        public bool Booted => _socket != null && _socket.Booted;

        public ConnectionBroker(ConnectionMeta config, Action<byte[]> userMsgHook, Action disconnectHook)
        {
            _socket = new P2PSocket(config, Received, OnAccept, disconnectHook);
            _usrMsg = userMsgHook;
        }

        private void Received(byte[] rawBytes)
        {
            var msg = new SocketMessage(rawBytes);
            DispatchMessage(msg);
        }

        private void DispatchMessage(SocketMessage msg)
        {
            if (msg.IsCrypto)
                RSAContract(msg.Message);
            else
                HandleUsrMsg(msg.Message);
        }

        private void HandleUsrMsg(byte[] message)
        {
            _usrMsg(_rsa.Contracted ? _rsa.Decrypt(message) : message);
        }

        public void SendUsrMsg(byte[] msg)
        {
            var toSend = _rsa.Contracted ? _rsa.Encrypt(msg) : msg;
            _socket.SendData(new SocketMessage(false, toSend).Raw);
        }

        private void RSAContract(byte[] guestKey)
        {
            if (_rsa.Contracted) return;
            var key = _rsa.Handshake(guestKey);
            _socket.SendData(new SocketMessage(true, key).Raw);
        }

        private void OnAccept()
        {
            _socket.SendData(new SocketMessage(true, _rsa.PublicKey).Raw);
        }

        private void OnAccept(P2PSocket socket)
        {
            _connections.Add(new Connection(socket, _usrMsg, () => { })); // TODO: pass a real action here
        }
    }

    internal class Connection
    {
        private readonly P2PSocket _socket;
        private readonly CryptoBroker _rsa = new();
        private readonly Action<byte[]> _usrMsg;
        public EndPoint? RemoteEndPoint => _socket.RemoteEndPoint;
        public bool Booted => _socket.Booted;

        public Connection(P2PSocket socket, Action<byte[]> userMsgHook, Action disconnectHook)
        {
            _socket = socket;
            _usrMsg = userMsgHook;
        }

        private void OnAccept()
        {
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
            _usrMsg(_rsa.Contracted ? _rsa.Decrypt(message) : message);
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
