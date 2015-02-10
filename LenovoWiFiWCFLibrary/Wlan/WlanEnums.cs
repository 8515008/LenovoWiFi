using System;

namespace Lenovo.WiFi.Wlan
{
    internal enum Dot11AuthAlgorithm : uint
    {
        WEPOpen = 1,
        WEPSharedKey = 2,
        WPA = 3,
        WPAPSK = 4,
        WPANone = 5,
        RSNA = 6,
        RSNAPSK = 7,
        IHVStart = 0x80000000,
        IHVEnd = 0xFFFFFFFF
    }

    internal enum Dot11CipherAlgorithm : uint
    {
        None = 0x00,
        WEP40 = 0x01,
        TKIP = 0x02,
        CCMP = 0x04,
        WEP104 = 0x05,
        BIP = 0x06,
        WPAUseGroup = 0x100,
        RSNUseGroup = 0x100,
        WEP = 0x101,
        IHVStart = 0x80000000,
        IHVEnd = 0xFFFFFFFF
    }

    internal enum Dot11PhyType : uint
    {
        Unknown = 0,
        Any = Unknown,
        FHSS = 1,
        DSSS = 2,
        IRBaseband = 3,
        OFDM = 4,
        HRDSSS = 5,
        ERP = 6,
        HT = 7,
        VHT = 8,
        IHVStart = 0x80000000,
        IHVEnd = 0xFFFFFFFF
    }

    internal enum WlanApiVersion : uint
    {
        Version = VersionTwo,
        VersionOne = 1,
        VersionTwo = 2
    }

    internal enum WlanHostedNetworkNotificationCode
    {
        StateChange = 4096,
        PeerStateChange,
        RadioStateChange
    }

    internal enum WlanHostedNetworkPeerAuthState
    {
        Invalid,
        Authenticated
    }

    internal enum WlanHostedNetworkOpcode
    {
        ConnectionSettings,
        SecuritySettings,
        StationProfile,
        Enable
    }

    internal enum WlanHostedNetworkReason
    {
        Success,
        Unspecified,
        BadParameter,
        ServiceShuttingDown,
        InsufficientResources,
        ElevationRequired,
        ReadOnly,
        PersistenceFailed,
        CryptError,
        Impersonation,
        StopBeforeStart,

        InterfaceAvailable,
        InterfaceUnavailable,
        MiniportStopped,
        MiniportStarted,
        IncompatibleConnectionStarted,
        IncompatibleConnectionStopped,
        UserAction,
        ClientAbort,
        APStartFailed,

        PeerArrived,
        PeerDeparted,
        PeerTimeout,
        GPDenied,
        ServiceUnavailable,
        DeviceChange,
        PropertiesChange,
        VirtualStationBlockingUse,
        ServiceAvailableOnVirtualStation
    }

    internal enum WlanHostedNetworkState
    {
        Unavailable,
        Idle,
        Active
    }

    [Flags]
    internal enum WlanNotificationSource : uint
    {
        None = 0,
        All = 0xFFFFFFFF,
        OneX = 0x00000004,
        AutoConfigModule = 0x00000008,
        MediaSpecificModule = 0x00000010,
        Security = 0x00000020,
        IndependentHardwareVendor = 0x00000040,
        HostedNetwork = 0x00000080,
    }

    internal enum WlanOpcodeValueType
    {
        QueryOnly,
        SetByGroupPolicy,
        SetByUser,
        Invalid
    }
}
