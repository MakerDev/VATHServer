using System;
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
        public event Action OnDeviceConnected;

        public bool SendData(string data)
        {
            throw new NotImplementedException();
        }

        public void DidConnectDevice()
        {
            OnDeviceConnected?.Invoke();
        }
    }
}
