using Foundation;
using MultipeerConnectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VathServer.Interfaces;

namespace VathServer.Platforms.MacCatalyst
{

    public class MacMultipeerManager : IMultipeerManager
    {
        public static MacMultipeerManager Instance { get; private set; } = null;
        public static MCSession Session { get; set; }
        public static MCNearbyServiceAdvertiser Advertiser { get; set; }

        //Define an event that has one parameter of type string
        //public delegate void OnDataReceviedHandler(string message);
        public event Action<string> OnDataReceived;

        public MacMultipeerManager()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public bool SendData(string message)
        {
            var session = Session;
            var peers = session.ConnectedPeers;

            if (peers.Length > 0)
            {
                var data = NSData.FromString(message, NSStringEncoding.UTF8);

                session.SendData(data, peers, MCSessionSendDataMode.Reliable, out NSError error);

                if (error != null)
                {
                    // Handle error while sending data
                    Console.WriteLine("Error sending data: " + error.LocalizedDescription);
                    return false;
                }
                else
                {
                    // Data sent successfully
                    Console.WriteLine("Data sent successfully");
                    return true;
                }
            }
            else
            {
                // No connected peers
                Console.WriteLine("No connected peers");
                return false;
            }
        }

        public void DidReceiveData(string message)
        {
            OnDataReceived?.Invoke(message);
        }
    }


    public class SessionDelegate : MCSessionDelegate
    {
        public override void DidReceiveData(MCSession session, NSData data, MCPeerID peerID)
        {
            // Handle received data
            var receivedData = data.ToArray();
            string message = System.Text.Encoding.UTF8.GetString(receivedData);

            // Do something with the received message
            Console.WriteLine("Received message: " + message);
            MacMultipeerManager.Instance?.DidReceiveData(message);
        }

        public override void DidChangeState(MCSession session, MCPeerID peerID, MCSessionState state)
        {
            Console.WriteLine($"New state:{state}");
        }
    }

    public class AdvertiserDelegate : MCNearbyServiceAdvertiserDelegate
    {
        public override void DidReceiveInvitationFromPeer(MCNearbyServiceAdvertiser advertiser, MCPeerID peerID, NSData context, MCNearbyServiceAdvertiserInvitationHandler invitationHandler)
        {
            // Accept the invitation and establish a session
            var session = MacMultipeerManager.Session;
            invitationHandler(true, session);
        }
    }
}

