﻿using Foundation;
using MultipeerConnectivity;
using UIKit;
using VathServer.Platforms.MacCatalyst;

namespace VathServer;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
        {
            // Set up Multipeer Connectivity
            var peerID = new MCPeerID("EisVathServer");
            var session = new MCSession(peerID, null, MCEncryptionPreference.Optional);
            session.Delegate = new SessionDelegate();

            var advertiser = new MCNearbyServiceAdvertiser(peerID, null, "eis-eyesight");
            advertiser.Delegate = new AdvertiserDelegate();
            advertiser.StartAdvertisingPeer();

            // Store the session and advertiser objects
            MacMultipeerManager.Session = session;
            MacMultipeerManager.Advertiser = advertiser;
        }

        return base.FinishedLaunching(app, options);
    }
}
