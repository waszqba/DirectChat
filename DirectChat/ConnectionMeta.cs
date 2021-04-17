using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectChat
{
    public class ConnectionMeta
    {
        public int port { get; set; }
        public int remotePort { get; set; }
        public string ip { get; set; }
        public bool host { get; set; }
    }
}
