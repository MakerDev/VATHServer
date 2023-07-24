using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VathServer.Interfaces
{
    public interface IMultipeerManager
    {
        public event Action<string> OnDataReceived;
        public event Action OnDeviceConnected;

        public bool SendData(string data);
        public Task<bool> SendDataAsync(string data);
    }
}
