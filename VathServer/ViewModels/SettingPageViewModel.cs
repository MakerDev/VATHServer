using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using VathServer.Interfaces;

namespace VathServer.ViewModels
{
    public partial class SettingPageViewModel : ObservableObject
    {
        private readonly IMultipeerManager _multipeerManager;
        [ObservableProperty]
        private static double _screenSizeInInch = 14;
        [ObservableProperty]
        private static double _imageSizeInCm = 5;
        [ObservableProperty]
        private static double _contrastValue = 1.0;
        [ObservableProperty]
        private static string _ipAddress = "";


        public SettingPageViewModel(IMultipeerManager multipeerManager)
        {
            GetIpAddress();
            _multipeerManager = multipeerManager;
            _multipeerManager.OnDataReceived += OnDataReceived;
        }

        private void OnDataReceived(string message)
        {
            if (message.StartsWith("conn"))
            {
                MainThread.BeginInvokeOnMainThread(async () => await StartSessionAsync());
                
            }
        }

        public static Dictionary<string, object> GetSesseionNavigationParam()
        {
            return new Dictionary<string, object>()
            {
                { nameof(ScreenSizeInInch), _screenSizeInInch },
                { nameof(ContrastValue), _contrastValue }
            };
        }

        private void GetIpAddress()
        {
            // Get all network interfaces on the system
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var ipAddressCandidates = new List<UnicastIPAddressInformation>();

            // Loop through each network interface
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Check if the interface is operational and not a loopback or tunnel interface
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                {
                    // Get the IP properties of the interface
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                    // Get the collection of unicast IP addresses assigned to the interface
                    UnicastIPAddressInformationCollection ipAddresses = ipProperties.UnicastAddresses;

                    // Loop through each IP address and display it
                    foreach (UnicastIPAddressInformation ipAddress in ipAddresses)
                    {
                        // Only consider IPv4 addresses
                        if (ipAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Console.WriteLine($"IPv4 Address: {ipAddress.Address}");
                            ipAddressCandidates.Add(ipAddress);
                        }
                    }
                }
            }

            if (ipAddressCandidates.Count > 0)
            {
                foreach (var ipAddress in ipAddressCandidates)
                {
                    if (ipAddress.IPv4Mask.ToString().Split('.')[2] == "255")
                    {
                        IpAddress += ipAddress.Address.ToString() + "\n";

                        //Take the first address.
                        //return;
                    }
                }

                // If there is no subnet mask, take the first address.
                IpAddress += ipAddressCandidates[0].Address.ToString();
                return;
            }

            // If there is no address, set it to localhost.
            IpAddress = "192.0.0.1";
        }

        [RelayCommand]
        private async Task StartSessionAsync()
        {
            _multipeerManager.OnDataReceived -= OnDataReceived;

            var navigationParameter = GetSesseionNavigationParam();

            await Shell.Current.GoToAsync(nameof(SessionView), navigationParameter);
        }
    }
}
