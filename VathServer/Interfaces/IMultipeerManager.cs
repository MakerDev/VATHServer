﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VathServer.Interfaces
{
    public interface IMultipeerManager
    {
        public event Action<string> OnDataReceived;
        public bool SendData(string data);
    }
}
