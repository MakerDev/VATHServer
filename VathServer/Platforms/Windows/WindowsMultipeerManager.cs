using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using VathServer.Interfaces;
using Zeroconf;
using System.IO;
using Windows.Media.Protection.PlayReady;

namespace VathServer.Platforms.Windows
{
    public class WindowsMultipeerManager : IMultipeerManager
    {
        private const int PORT = 9099;

        public event Action<string> OnDataReceived;
        public event Action OnDeviceConnected;

        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        public WindowsMultipeerManager()
        {
            _listener = new TcpListener(IPAddress.Any, PORT);
            _listener.Start();
            StartListeningAsync();
        }

        public async Task StartListeningAsync()
        {
            _client = await _listener.AcceptTcpClientAsync();

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8, true, 512);
            _writer = new StreamWriter(_stream, Encoding.UTF8, leaveOpen: true);

            await _writer.WriteLineAsync("Hello from Windows");
            await _writer.FlushAsync();

            await Task.Run(async () =>
            {
                while (true)
                {
                    var data = await _reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(data) || data.StartsWith("close"))
                    {
                        break;
                    }

                    OnDataReceived?.Invoke(data);
                }

                CloseConnection();
                await StartListeningAsync();
            });
        }

        public async Task<bool> SendDataAsync(string data)
        {
            if (_client == null || !_client.Connected || _writer == null)
            {
                return false;
            }

            await _writer.WriteLineAsync(data);
            await _writer.FlushAsync();

            return true;
        }

        public bool SendData(string data)
        {
            return SendDataAsync(data).Result;
        }

        // Make sure to call this method when you are done with the connection.
        public void CloseConnection()
        {
            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _client?.Close();
        }

        public void DidConnectDevice()
        {
            OnDeviceConnected?.Invoke();
        }
    }
}