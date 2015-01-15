using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Lenovo.WiFi.ICS;
using Lenovo.WiFi.Wlan;
using NETCONLib;

namespace Lenovo.WiFi
{
    public class HostedNetworkManager : IDisposable
    {
        readonly object _l = new object();
        private bool _disposed;

        private readonly WlanHandle _wlanHandle;
        private WlanHostedNetworkConnectionSettings _connectionSettings;
        private WlanHostedNetworkSecuritySettings _securitySettings;
        private readonly Guid _hostedNetworkInterfaceGuid;
        private WlanHostedNetworkState _hostedNetworkState;

        private readonly ICSManager _icsManager;

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust", Unrestricted = false)]
        public HostedNetworkManager()
        {
            uint returnValue = 0;
            
            var enabled = IntPtr.Zero;
            var connectionSettings = IntPtr.Zero;
            var securitySettings = IntPtr.Zero;
            var status = IntPtr.Zero;

            try
            {
                Lock();

                uint negotiatedVersion;

                WlanHandle clientHandle;
                returnValue = NativeMethods.WlanOpenHandle(
                    WlanApiVersion.Version,
                    IntPtr.Zero,
                    out negotiatedVersion,
                    out clientHandle);

                Utilities.ThrowOnError(returnValue);

                if (negotiatedVersion != (uint) WlanApiVersion.Version)
                {
                    throw new WlanException("Wlan API version negotiation failed.");
                }

                this._wlanHandle = clientHandle;

                WlanNotificationSource previousNotificationSource;
                returnValue = NativeMethods.WlanRegisterNotification(
                    clientHandle,
                    WlanNotificationSource.HostedNetwork,
                    true,
                    OnNotification,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out previousNotificationSource);

                Utilities.ThrowOnError(returnValue);

                WlanHostedNetworkReason faileReason;
                returnValue = NativeMethods.WlanHostedNetworkInitSettings(
                    clientHandle,
                    out faileReason,
                    IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                uint dataSize;
                WlanOpcodeValueType opcodeValueType;
                returnValue = NativeMethods.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.Enable,
                    out dataSize,
                    out enabled,
                    out opcodeValueType,
                    IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                this.IsHostedNetworkAllowed = Convert.ToBoolean(Marshal.ReadInt32(enabled));

                returnValue = NativeMethods.WlanHostedNetworkQueryProperty(
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

                this._connectionSettings =
                    (WlanHostedNetworkConnectionSettings)
                        Marshal.PtrToStructure(connectionSettings, typeof (WlanHostedNetworkConnectionSettings));

                returnValue = NativeMethods.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.SecuritySettings,
                    out dataSize,
                    out securitySettings,
                    out opcodeValueType,
                    IntPtr.Zero);

                if (securitySettings == IntPtr.Zero
                    || Marshal.SizeOf(typeof(WlanHostedNetworkSecuritySettings)) < dataSize)
                {
                    Utilities.ThrowOnError(13);
                }

                this._securitySettings =
                    (WlanHostedNetworkSecuritySettings)
                        Marshal.PtrToStructure(securitySettings, typeof (WlanHostedNetworkSecuritySettings));

                returnValue = NativeMethods.WlanHostedNetworkQueryStatus(
                        clientHandle,
                        out status,
                        IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);

                var wlanHostedNetworkStatus =
                    (WlanHostedNetworkStatus)
                        Marshal.PtrToStructure(status, typeof (WlanHostedNetworkStatus));

                _hostedNetworkInterfaceGuid = wlanHostedNetworkStatus.IPDeviceID;
                _hostedNetworkState = wlanHostedNetworkStatus.HostedNetworkState;

                _icsManager = new ICSManager();
            }
            finally
            {
                if (returnValue != 0 && !this._wlanHandle.IsInvalid)
                {
                    this._wlanHandle.Dispose();
                }

                Unlock();

                if (enabled != IntPtr.Zero)
                {
                    NativeMethods.WlanFreeMemory(enabled);
                }

                if (connectionSettings != IntPtr.Zero)
                {
                    NativeMethods.WlanFreeMemory(connectionSettings);
                }

                if (securitySettings != IntPtr.Zero)
                {
                    NativeMethods.WlanFreeMemory(securitySettings);
                }

                if (status != IntPtr.Zero)
                {
                    NativeMethods.WlanFreeMemory(status);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _icsManager.Dispose();

                if (_wlanHandle != null && !_wlanHandle.IsInvalid)
                {
                    _wlanHandle.Dispose();
                }
            }

            _disposed = true;
        }

        public bool IsHostedNetworkAllowed { get; private set; }

        public event EventHandler HostedNetworkEnabled;
        public event EventHandler HostedNetworkStarted;
        public event EventHandler HostedNetworkStopped;
        public event EventHandler HostedNetworkDisabled;

        public event EventHandler<DeviceConnectedEventArgs> DeviceConnected;
        public event EventHandler<DeviceDisconnectedEventArgs> DeviceDisconnected;

        public string GetHostedNetworkName()
        {
            return this._connectionSettings.HostedNetworkSSID.SSID;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust", Unrestricted = false)]
        public void SetHostedNetworkName(string name)
        {
            const int dot11SSIDMaxLength = 32;

            if (Encoding.ASCII.GetByteCount(name) > dot11SSIDMaxLength)
            {
                throw new ArgumentOutOfRangeException("name", "The size of SSID should be less than 32 bytes.");
            }

            var newSettings = new WlanHostedNetworkConnectionSettings
            {
                HostedNetworkSSID = new Dot11SSID {SSID = name, SSIDLength = (uint) Encoding.Default.GetByteCount(name)},
                MaxNumberOfPeers = this._connectionSettings.MaxNumberOfPeers
            };

            WlanHostedNetworkReason faileReason;
            IntPtr newSettingPtr = Marshal.AllocHGlobal(Marshal.SizeOf(newSettings));
            Marshal.StructureToPtr(newSettings, newSettingPtr, false);

            Utilities.ThrowOnError(
                NativeMethods.WlanHostedNetworkSetProperty(
                    this._wlanHandle,
                    WlanHostedNetworkOpcode.ConnectionSettings,
                    (uint)Marshal.SizeOf(newSettings),
                    newSettingPtr,
                    out faileReason,
                    IntPtr.Zero));

            this._connectionSettings = newSettings;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust", Unrestricted = false)]
        public string GetHostedNetworkKey()
        {
            string result = string.Empty;

            uint keyLength;
            IntPtr keyData;
            bool isPassPhrase;
            bool isPersistent;
            WlanHostedNetworkReason failReason;

            uint error = NativeMethods.WlanHostedNetworkQuerySecondaryKey(
                this._wlanHandle,
                out keyLength,
                out keyData,
                out isPassPhrase,
                out isPersistent,
                out failReason,
                IntPtr.Zero);

            Utilities.ThrowOnError(error);

            if (keyLength != 0 && keyData != IntPtr.Zero)
            {
                result = Marshal.PtrToStringAnsi(keyData, (int)keyLength);

                if (isPassPhrase)
                {
                    result = result.Substring(0, (int)keyLength - 1);
                }

                NativeMethods.WlanFreeMemory(keyData);
            }

            return result;
        }

        public void SetHostedNetworkKey(string key)
        {
            var keyLength = Encoding.ASCII.GetByteCount(key);

            if (keyLength < 8 || keyLength > 63)
            {
                throw new ArgumentOutOfRangeException("key", "The size of key should be between 8 and 64.");
            }

            WlanHostedNetworkReason failReason;

            uint error = NativeMethods.WlanHostedNetworkSetSecondaryKey(
                this._wlanHandle,
                (uint)key.Length + 1,
                Encoding.ASCII.GetBytes(key),
                true,
                true,
                out failReason,
                IntPtr.Zero);

            Utilities.ThrowOnError(error);
        }

        public void StartHostedNetwork()
        {
            if (!_icsManager.IsServiceStatusValid)
            {
                throw new ICSException("The service of ICS is in pending state.");
            }

            Lock();

            try
            {
                WlanHostedNetworkReason failReason;

                if (_hostedNetworkState == WlanHostedNetworkState.Active)
                {
                    Utilities.ThrowOnError(
                    NativeMethods.WlanHostedNetworkForceStop(
                        this._wlanHandle,
                        out failReason,
                        IntPtr.Zero
                    ));
                }

                Utilities.ThrowOnError(
                    NativeMethods.WlanHostedNetworkStartUsing(
                        this._wlanHandle,
                        out failReason,
                        IntPtr.Zero));

                var privateGuid = _hostedNetworkInterfaceGuid;
                var publicGuid = GetPreferredPublicGuid(privateGuid);

                _icsManager.EnableSharing(publicGuid, privateGuid);
            }
            finally
            {
                Unlock();
            }
        }

        public void StopHostedNetwork()
        {
            if (_hostedNetworkState != WlanHostedNetworkState.Active)
            {
                return;
            }

            Lock();

            try
            {
                WlanHostedNetworkReason failReason;
                Utilities.ThrowOnError(
                    NativeMethods.WlanHostedNetworkStopUsing(
                        this._wlanHandle,
                        out failReason,
                        IntPtr.Zero));
            }
            finally
            {
                Unlock();
            }
        }

        private Guid GetPreferredPublicGuid(Guid privateGuid)
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            var nic = nics.FirstOrDefault(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                                               && n.OperationalStatus == OperationalStatus.Up) ??
                      nics.FirstOrDefault(n => n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                                               && n.OperationalStatus == OperationalStatus.Up
                                               && new Guid(n.Id) != privateGuid);

            if (nic == null)
            {
                throw new ApplicationException("No preferred public network is available.");
            }

            return new Guid(nic.Id);
        }

        private void Lock()
        {
            Monitor.Enter(_l);
        }

        private void Unlock()
        {
            Monitor.Exit(_l);
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
            _hostedNetworkState = WlanHostedNetworkState.Idle;

            if (HostedNetworkEnabled != null)
            {
                HostedNetworkEnabled(this, EventArgs.Empty);
            }
        }

        void OnHostedNetworkStarted()
        {
            _hostedNetworkState = WlanHostedNetworkState.Active;

            if (HostedNetworkStarted != null)
            {
                HostedNetworkStarted(this, EventArgs.Empty);
            }
        }

        void OnHostedNetworkStopped()
        {
            _hostedNetworkState = WlanHostedNetworkState.Idle;

            if (HostedNetworkStopped != null)
            {
                HostedNetworkStopped(this, EventArgs.Empty);
            }
        }

        void OnHostedNetworkDisabled()
        {
            _hostedNetworkState = WlanHostedNetworkState.Unavailable;

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
