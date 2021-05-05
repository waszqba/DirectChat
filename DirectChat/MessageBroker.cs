using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DirectChat
{
    internal class MessageBroker
    {
        private readonly ConnectionBroker _connector;
        private readonly Action<string, EndPoint> _onMessage;
        public bool Booted => _connector.Booted;

        public MessageBroker(ConnectionMeta config, Action<string, EndPoint> onMessageHook, Action<EndPoint> onDisconnectHook)
        {
            _connector = new ConnectionBroker(config, OnMessage, onDisconnectHook);
            _onMessage = onMessageHook;
        }

        private void OnMessage(byte[] raw, EndPoint endPoint)
        {
            _onMessage(Encoding.Default.GetString(raw), endPoint);
        }

        public void Send(string msg, string index)
        {
            _connector.SendUsrMsg(Encoding.Default.GetBytes(msg), index);
        }

        public void NewConvo(ConnectionMeta config)
        {
            _connector.NewConvo(config);
        }

    }
}
