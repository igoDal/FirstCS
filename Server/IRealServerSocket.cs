using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface IRealServerSocket
    {
        int Send(byte[] buffer);
        int Receive(byte[] buffer);
    }
}
