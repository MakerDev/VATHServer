﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VathServer.Interfaces;

namespace VathServer.Platforms.Windows
{
    public class WindowsMultipeerManager : IMultipeerManager
    {
        public event Action<string> OnDataReceived;

        public bool SendData(string data)
        {
            throw new NotImplementedException();
        }
    }
}