using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectChat
{
    class MessageBroker
    {
        private readonly ConnectionBroker _connector;
        private readonly Action<string> _onMessage;
        public bool Booted => _connector != null && _connector.Booted;

        public MessageBroker(ConnectionMeta config, Action<string> onMessageHook, Action onDisconnectHook)
        {
            _connector = new ConnectionBroker(config, OnMessage, onDisconnectHook);
            _onMessage = onMessageHook;
        }

        private void OnMessage(byte[] raw)
        {
            var text = Encoding.Default.GetString(raw);
            _onMessage(text);
        }

        public void Send(string msg)
        {
            _connector.SendUsrMsg(Encoding.Default.GetBytes(msg));
        }
    }
}
