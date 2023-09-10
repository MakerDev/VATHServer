﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace VathServer.ViewModels
{
    public partial class SettingPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _screenSizeInInch = 14;
        [ObservableProperty]
        private double _imageSizeInCm = 5;
        [ObservableProperty]
        private double _contrastValue = 1.0;
        [ObservableProperty]
        private string _ipAddress = "";
        public SettingPageViewModel()
        {
            GetIpAddress();
        }

        private void GetIpAddress()
        {
            // Get all network interfaces on the system
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

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
                        if (ipAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork 
                                && ipAddress.IPv4Mask.ToString().Split('.')[2] == "255")
                        {
                            Console.WriteLine($"IPv4 Address: {ipAddress.Address}");
                            IpAddress = ipAddress.Address.ToString();

                            //Take the first address.
                            return;
                        }
                    }
                }
            }
        }

        [RelayCommand]
        private async Task StartSessionAsync()
        {
            var navigationParameter = new Dictionary<string, object>()
            {
                { nameof(ScreenSizeInInch), ScreenSizeInInch },
                { nameof(ContrastValue), ContrastValue }
            };
            await Shell.Current.GoToAsync(nameof(SessionView), navigationParameter);
        }
    }
}
