﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public interface IRealClientSocket
    {
        int Send(byte[] buffer);
        int Receive(byte[] buffer);
    }
}
