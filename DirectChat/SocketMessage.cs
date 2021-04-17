using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectChat
{
    class SocketMessage
    {
        public readonly byte[] Message;
        public readonly bool IsCrypto;

        public byte[] Raw => ToRaw();

        public SocketMessage(byte[] raw)
        {
            IsCrypto = BitConverter.ToBoolean(raw, 0);
            var msgLen = BitConverter.ToInt32(raw, 1);
            Message = raw.Skip(5).Take(msgLen).ToArray();
        }

        public SocketMessage(bool crypto, byte[] data)
        {
            IsCrypto = crypto;
            Message = data;
        }

        private byte[] ToRaw()
        {
            var list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(IsCrypto));
            list.AddRange(BitConverter.GetBytes(Message.Length));
            list.AddRange(Message);
            return list.ToArray();
        }
    }
}
