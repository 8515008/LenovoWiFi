using System;
using System.Runtime.InteropServices;

namespace HostedNetworkManager.Wlan
{
    internal struct Dot11SSID
    {
        internal uint SSIDLength;
        [MarshalAs(UnmanagedType.LPTStr, SizeConst = 32)]
        internal string SSID;
    }

    internal struct WlanHostedNetworkConnectionSettings
    {
        internal Dot11SSID HostedNetworkSSID;
        internal uint MaxNumberOfPeers;
    }

    internal struct WlanHostedNetworkDataPeerStateChange
    {
        internal WlanHostedNetworkPeerState OldState;
        internal WlanHostedNetworkPeerState NewState;
        internal WlanHostedNetworkReason StateChangeReason;
    }

    internal struct WlanHostedNetworkPeerState
    {
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
        internal byte[] PeerMacAddress;
        internal WlanHostedNetworkPeerAuthState PeerAuthState;
    }

    internal struct WlanHostedNetworkSecuritySettings
    {
        internal Dot11AuthAlgorithm Dot11AuthAlgo;
        internal Dot11CipherAlgorithm Dot11CipherAlgo;
    }

    internal struct WlanHostedNetworkStateChange
    {
        internal WlanHostedNetworkState OldState;
        internal WlanHostedNetworkState NewState;
        internal WlanHostedNetworkReason StateChangeReason;
    }

    internal struct WlanHostedNetworkStatus
    {
        internal WlanHostedNetworkState HostedNetworkState;
        internal Guid IPDeviceID;
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
        internal byte[] WlanHostedNetworkBSSID;
        internal Dot11PhyType Dot11PhyType;
        internal uint ChannelFrequency;
        internal uint NumberOfPeers;
        internal WlanHostedNetworkPeerState[] PeerList;
    }

    internal struct WlanNotificationData
    {
        internal WlanNotificationSource NotificationSource;
        internal WlanHostedNetworkNotificationCode NotificationCode;
        internal Guid InterfaceGuid;
        internal int DataSize;
        internal IntPtr Data;
    }
}
