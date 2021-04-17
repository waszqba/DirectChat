using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DirectChat
{
    class CryptoBroker
    {
        public readonly byte[] PublicKey;
        public bool Contracted { get; private set; }
        private readonly RSAParameters _privateKey;
        private byte[] _guestKey;

        public CryptoBroker()
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            PublicKey = rsa.ExportRSAPublicKey();
            _privateKey = rsa.ExportParameters(true);
            Contracted = false;
        }

        public byte[] Handshake(byte[] guestKey)
        {
            if (_guestKey != null) throw new Exception("Już zainicjowano kontrakt z gościem w tej sesji");
            _guestKey = guestKey;
            Contracted = true;
            return PublicKey;
        }

        public byte[] Encrypt(byte[] data)
        {
            //Create a new instance of RSACryptoServiceProvider.
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportRSAPublicKey(_guestKey, out var res);
            return rsa.Encrypt(data, true);
        }

        public byte[] Decrypt(byte[] data)
        {
            using var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(_privateKey);
            return rsa.Decrypt(data, true);
        }
    }
}
