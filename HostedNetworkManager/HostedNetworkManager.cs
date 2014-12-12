using System;
using System.Runtime.InteropServices;
using System.Threading;
using HostedNetworkManager.Wlan;

namespace HostedNetworkManager
{
    public class HostedNetworkManager
    {
        readonly object l = new object();

        private readonly IntPtr wlanHandle;
        private readonly WlanHostedNetworkConnectionSettings connectionSettings;
        private readonly WlanHostedNetworkSecuritySettings securitySettings;
        private WlanHostedNetworkState hostedNetworkState;

        public HostedNetworkManager()
        {
            var clientHandle = IntPtr.Zero;
            uint dataSize;
            var enabled = IntPtr.Zero;
            var connectionSettings = IntPtr.Zero;
            var securitySettings = IntPtr.Zero;
            var status = IntPtr.Zero;

            try
            {
                Lock();

                uint negotiatedVersion;

                var returnValue = WlanApi.WlanOpenHandle(
                    WlanApiVersion.Version,
                    IntPtr.Zero,
                    out negotiatedVersion,
                    out clientHandle);

                Utilities.ThrowOnError(returnValue);

                if (negotiatedVersion != (uint) WlanApiVersion.Version)
                {
                    throw new WlanException("Wlan API version negotiation failed.");
                }

                this.wlanHandle = clientHandle;

                WlanNotificationSource previousNotificationSource;
                returnValue = WlanApi.WlanRegisterNotification(
                    clientHandle,
                    WlanNotificationSource.HostedNetwork,
                    true,
                    OnNotification,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out previousNotificationSource);

                Utilities.ThrowOnError(returnValue);

                WlanHostedNetworkReason faileReason;
                returnValue = WlanApi.WlanHostedNetworkInitSettings(
                    clientHandle,
                    out faileReason,
                    IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                WlanOpcodeValueType opcodeValueType;
                returnValue = WlanApi.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.Enable,
                    out dataSize,
                    out enabled,
                    out opcodeValueType,
                    IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                this.IsHostedNetworkAllowed = Convert.ToBoolean(Marshal.ReadInt32(enabled));

                returnValue = WlanApi.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.ConnectionSettings,
                    out dataSize,
                    out connectionSettings,
                    out opcodeValueType,
                    IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                if (connectionSettings == IntPtr.Zero
                    || Marshal.SizeOf(typeof(WlanHostedNetworkConnectionSettings)) < dataSize)
                {
                    Utilities.ThrowOnError(13);
                }

                this.connectionSettings =
                    (WlanHostedNetworkConnectionSettings)
                        Marshal.PtrToStructure(connectionSettings, typeof (WlanHostedNetworkConnectionSettings));

                returnValue = WlanApi.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.SecuritySettings,
                    out dataSize,
                    out securitySettings,
                    out opcodeValueType,
                    IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                this.securitySettings =
                    (WlanHostedNetworkSecuritySettings)
                        Marshal.PtrToStructure(securitySettings, typeof (WlanHostedNetworkSecuritySettings));

                returnValue = WlanApi.WlanHostedNetworkQueryStatus(
                        clientHandle,
                        out status,
                        IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                WlanHostedNetworkStatus wlanHostedNetworkStatus =
                    (WlanHostedNetworkStatus)
                        Marshal.PtrToStructure(status, typeof (WlanHostedNetworkStatus));

                hostedNetworkState = wlanHostedNetworkStatus.HostedNetworkState;
            }
            finally
            {
                Unlock();
            }
        }

        public bool IsHostedNetworkAllowed { get; private set; }

        public event EventHandler HostedNetworkEnabled;
        public event EventHandler HostedNetworkStarted;
        public event EventHandler HostedNetworkStopped;
        public event EventHandler HostedNetworkDisabled;

        public event EventHandler<DeviceConnectedEventArgs> DeviceConnected;
        public event EventHandler<DeviceDisconnectedEventArgs> DeviceDisconnected;

        private void Lock()
        {
            Monitor.Enter(l);
        }

        private void Unlock()
        {
            Monitor.Exit(l);
        }

        private void OnNotification(WlanNotificationData notificationData, IntPtr context)
        {
            if (notificationData.NotificationSource == WlanNotificationSource.HostedNetwork)
            {
                switch (notificationData.NotificationCode)
                {
                    case WlanHostedNetworkNotificationCode.StateChange:
                        if (Marshal.SizeOf(typeof(WlanHostedNetworkStateChange)) == notificationData.DataSize
                            && notificationData.Data != IntPtr.Zero)
                        {
                            var stateChange =
                                (WlanHostedNetworkStateChange) Marshal.PtrToStructure(notificationData.Data,
                                    typeof (WlanHostedNetworkStateChange));

                            switch (stateChange.NewState)
                            {
                                case WlanHostedNetworkState.Active:
                                    OnHostedNetworkStarted();
                                    break;
                                case WlanHostedNetworkState.Idle:
                                    if (stateChange.OldState == WlanHostedNetworkState.Active)
                                    {
                                        OnHostedNetworkStopped();
                                    }
                                    else
                                    {
                                        OnHostedNetworkEnabled();
                                    }
                                    break;
                                case WlanHostedNetworkState.Unavailable:
                                    if (stateChange.OldState == WlanHostedNetworkState.Active)
                                    {
                                        OnHostedNetworkStopped();
                                    }
                                    OnHostedNetworkDisabled();
                                    break;
                            }
                        }
                        break;
                    case WlanHostedNetworkNotificationCode.PeerStateChange:
                        if (Marshal.SizeOf(typeof(WlanHostedNetworkDataPeerStateChange)) == notificationData.DataSize
                            && notificationData.Data != IntPtr.Zero)
                        {
                            var peerStateChange =
                                (WlanHostedNetworkDataPeerStateChange)
                                    Marshal.PtrToStructure(notificationData.Data,
                                        typeof (WlanHostedNetworkDataPeerStateChange));

                            if (peerStateChange.NewState.PeerAuthState == WlanHostedNetworkPeerAuthState.Authenticated)
                            {
                                OnDeviceConnected(peerStateChange.NewState);
                            }
                            if (peerStateChange.NewState.PeerAuthState == WlanHostedNetworkPeerAuthState.Invalid)
                            {
                                OnDeviceDisconnected(peerStateChange.NewState.PeerMacAddress);
                            }
                        }
                        break;
                }
            }
        }

        void OnHostedNetworkEnabled()
        {
            if (HostedNetworkEnabled != null)
            {
                HostedNetworkEnabled(this, EventArgs.Empty);
            }
        }

        void OnHostedNetworkStarted()
        {
            if (HostedNetworkStarted != null)
            {
                HostedNetworkStarted(this, EventArgs.Empty);
            }
        }

        void OnHostedNetworkStopped()
        {
            if (HostedNetworkStopped != null)
            {
                HostedNetworkStopped(this, EventArgs.Empty);
            }
        }

        void OnHostedNetworkDisabled()
        {
            if (HostedNetworkDisabled != null)
            {
                HostedNetworkDisabled(this, EventArgs.Empty);
            }
        }

        void OnDeviceConnected(WlanHostedNetworkPeerState peerState)
        {
            if (DeviceConnected != null)
            {
                var args = new DeviceConnectedEventArgs(peerState.PeerMacAddress,
                    Convert.ToBoolean(peerState.PeerAuthState));
                DeviceConnected(this, args);
            }
        }

        void OnDeviceDisconnected(byte[] deviceMacAddress)
        {
            if (DeviceDisconnected != null)
            {
                var args = new DeviceDisconnectedEventArgs(deviceMacAddress);
                DeviceDisconnected(this, args);
            }
        }
    }
}
