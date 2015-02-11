using System;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

using Lenovo.WiFi.ICS;
using Lenovo.WiFi.Wlan;
using NLog;

namespace Lenovo.WiFi
{
    public class HostedNetworkManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _l = new object();
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
            var enabled = IntPtr.Zero;
            var connectionSettings = IntPtr.Zero;
            var securitySettings = IntPtr.Zero;
            var status = IntPtr.Zero;

            try
            {
                Logger.Trace(".ctor: Start invoking native codes...");
                Lock();

                uint negotiatedVersion;
                WlanHandle clientHandle;

                Logger.Trace(".ctor: Invoking WlanOpenHandle...");
                var returnValue = NativeMethods.WlanOpenHandle(
                    WlanApiVersion.Version,
                    IntPtr.Zero,
                    out negotiatedVersion,
                    out clientHandle);
                Logger.Trace(".ctor: WlanOpenHandle returned {0}", returnValue);

                Utilities.ThrowOnError(returnValue);

                if (negotiatedVersion != (uint) WlanApiVersion.Version)
                {
                    Logger.Error(".ctor: Wlan API version negotiation failed");
                    throw new WlanException("Wlan API version negotiation failed");
                }

                Logger.Trace(".ctor: _wlanHandle: {0:x16}", clientHandle.DangerousGetHandle().ToInt64());
                this._wlanHandle = clientHandle;

                WlanNotificationSource previousNotificationSource;

                Logger.Trace(".ctor: Invoking WlanRegisterNotification...");
                returnValue = NativeMethods.WlanRegisterNotification(
                    clientHandle,
                    WlanNotificationSource.HostedNetwork,
                    true,
                    OnNotification,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out previousNotificationSource);
                Logger.Trace(".ctor: WlanRegisterNotification returned {0}", returnValue);

                Utilities.ThrowOnError(returnValue);

                WlanHostedNetworkReason faileReason;

                Logger.Trace(".ctor: Invoking WlanHostedNetworkInitSettings...");
                returnValue = NativeMethods.WlanHostedNetworkInitSettings(
                    clientHandle,
                    out faileReason,
                    IntPtr.Zero);
                Logger.Trace(".ctor: WlanHostedNetworkInitSettings returned {0}", returnValue);

                Utilities.ThrowOnError(returnValue);

                uint dataSize;
                WlanOpcodeValueType opcodeValueType;

                Logger.Trace(".ctor: Invoking WlanHostedNetworkQueryProperty with WlanHostedNetworkOpcode.Enable...");
                returnValue = NativeMethods.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.Enable,
                    out dataSize,
                    out enabled,
                    out opcodeValueType,
                    IntPtr.Zero);
                Logger.Trace(".ctor: WlanHostedNetworkQueryProperty with WlanHostedNetworkOpcode.Enable returned {0}",
                    returnValue);

                Utilities.ThrowOnError(returnValue);

                this.IsHostedNetworkAllowed = Convert.ToBoolean(Marshal.ReadInt32(enabled));
                Logger.Info(".ctor: IsHostedNetworkAllowed: {0}", this.IsHostedNetworkAllowed);

                Logger.Trace(
                    ".ctor: Invoking WlanHostedNetworkQueryProperty with WlanHostedNetworkOpcode.ConnectionSettings...");
                returnValue = NativeMethods.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.ConnectionSettings,
                    out dataSize,
                    out connectionSettings,
                    out opcodeValueType,
                    IntPtr.Zero);

                Utilities.ThrowOnError(returnValue);
                Logger.Trace(
                    ".ctor: WlanHostedNetworkQueryProperty with WlanHostedNetworkOpcode.ConnectionSettings returned {0}",
                    returnValue);

                if (connectionSettings == IntPtr.Zero
                    || Marshal.SizeOf(typeof (WlanHostedNetworkConnectionSettings)) < dataSize)
                {
                    Logger.Error(".ctor: Got invalid connection setting data");
                    Utilities.ThrowOnError(13);
                }

                this._connectionSettings =
                    (WlanHostedNetworkConnectionSettings)
                        Marshal.PtrToStructure(connectionSettings, typeof (WlanHostedNetworkConnectionSettings));
                Logger.Info(".ctor: Connection setting: SSID: {0}, max peer count: {1}",
                    this._connectionSettings.HostedNetworkSSID.SSID, this._connectionSettings.MaxNumberOfPeers);

                Logger.Trace(
                    ".ctor: Invoking WlanHostedNetworkQueryProperty with WlanHostedNetworkOpcode.SecuritySettings...");
                returnValue = NativeMethods.WlanHostedNetworkQueryProperty(
                    clientHandle,
                    WlanHostedNetworkOpcode.SecuritySettings,
                    out dataSize,
                    out securitySettings,
                    out opcodeValueType,
                    IntPtr.Zero);
                Logger.Trace(
                    ".ctor: WlanHostedNetworkQueryProperty with WlanHostedNetworkOpcode.ConnectionSettings returned {0}",
                    returnValue);

                if (securitySettings == IntPtr.Zero
                    || Marshal.SizeOf(typeof (WlanHostedNetworkSecuritySettings)) < dataSize)
                {
                    Logger.Error(".ctor: Got invalid security setting data");
                    Utilities.ThrowOnError(13);
                }

                this._securitySettings =
                    (WlanHostedNetworkSecuritySettings)
                        Marshal.PtrToStructure(securitySettings, typeof (WlanHostedNetworkSecuritySettings));
                Logger.Info(".ctor: Security setting: Authentication algorithm: {0}, cipher algorithm: {1}",
                    this._securitySettings.Dot11AuthAlgo, this._securitySettings.Dot11CipherAlgo);

                Logger.Trace(".ctor: Invoking WlanHostedNetworkQueryStatus...");
                returnValue = NativeMethods.WlanHostedNetworkQueryStatus(
                    clientHandle,
                    out status,
                    IntPtr.Zero);
                Logger.Trace(".ctor: WlanHostedNetworkQueryStatus returned {0}", returnValue);

                Utilities.ThrowOnError(returnValue);

                var wlanHostedNetworkStatus =
                    (WlanHostedNetworkStatus)
                        Marshal.PtrToStructure(status, typeof (WlanHostedNetworkStatus));

                _hostedNetworkInterfaceGuid = wlanHostedNetworkStatus.IPDeviceID;
                _hostedNetworkState = wlanHostedNetworkStatus.HostedNetworkState;
                Logger.Info(
                    ".ctor: Hosted network status: State: {0}, BSSID: {1}, physical type: {2}, channel frequency: {3}, current number of peers: {4}",
                    wlanHostedNetworkStatus.HostedNetworkState,
                    BitConverter.ToString(wlanHostedNetworkStatus.WlanHostedNetworkBSSID),
                    wlanHostedNetworkStatus.Dot11PhyType,
                    wlanHostedNetworkStatus.ChannelFrequency,
                    wlanHostedNetworkStatus.NumberOfPeers);

                _icsManager = new ICSManager();
                Logger.Trace(".ctor: ICSManager initialized  ");
            }
            catch (ICSException)
            {
                Logger.Trace(".ctor: ICSManager is invalid");
            }
            catch (Win32Exception)
            {
                if (!this._wlanHandle.IsInvalid)
                {
                    this._wlanHandle.Dispose();
                }
                throw;
            }
            finally
            {
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

        ~HostedNetworkManager()
        {
            Dispose(false);
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
                if (_icsManager != null)
                {
                    _icsManager.Dispose();
                }
            }

            if (_wlanHandle != null && !_wlanHandle.IsInvalid)
            {
                _wlanHandle.Dispose();
            }

            _disposed = true;
        }

        public bool IsHostedNetworkAllowed { get; private set; }
        public int ConnectedDeviceCount { get; private set; }

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
            Logger.Trace("SetHostedNetworkName: New value: {0}", name);
            const int dot11SSIDMaxLength = 32;

            var nameLength = Encoding.ASCII.GetByteCount(name);
            Logger.Info("SetHostedNetworkName: The default encoding character count of {0} is {1}", name, nameLength);

            if (nameLength > dot11SSIDMaxLength)
            {
                Logger.Error("SetHostedNetworkName: The length of name should be less than 32. Ignore it...");
                throw new ArgumentOutOfRangeException("name", "Character count should be less than 32");
            }

            var newSettings = new WlanHostedNetworkConnectionSettings
            {
                HostedNetworkSSID = new Dot11SSID { SSID = name, SSIDLength = (uint)Encoding.Default.GetByteCount(name) },
                MaxNumberOfPeers = this._connectionSettings.MaxNumberOfPeers
            };

            WlanHostedNetworkReason faileReason;
            var newSettingPtr = Marshal.AllocHGlobal(Marshal.SizeOf(newSettings));
            Marshal.StructureToPtr(newSettings, newSettingPtr, false);
            Logger.Trace("SetHostedNetworkName: New Setting: {0x16}", newSettingPtr.ToInt64());

            Utilities.ThrowOnError(
                NativeMethods.WlanHostedNetworkSetProperty(
                    this._wlanHandle,
                    WlanHostedNetworkOpcode.ConnectionSettings,
                    (uint)Marshal.SizeOf(newSettings),
                    newSettingPtr,
                    out faileReason,
                    IntPtr.Zero));
            Logger.Trace("SetHostedNetworkName: WlanHostedNetworkSetProperty invoked without error");

            this._connectionSettings = newSettings;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust", Unrestricted = false)]
        public string GetHostedNetworkKey()
        {
            Logger.Trace("GetHostedNetworkKey: Invoked");
            var result = string.Empty;

            uint keyLength;
            IntPtr keyData;
            bool isPassPhrase;
            bool isPersistent;
            WlanHostedNetworkReason failReason;


            var error = NativeMethods.WlanHostedNetworkQuerySecondaryKey(
                this._wlanHandle,
                out keyLength,
                out keyData,
                out isPassPhrase,
                out isPersistent,
                out failReason,
                IntPtr.Zero);
            Logger.Trace("GetHostedNetworkKey: WlanHostedNetworkQuerySecondaryKey returned {0}", error);

            Utilities.ThrowOnError(error);

            if (keyLength != 0 && keyData != IntPtr.Zero)
            {
                if (isPassPhrase)
                {
                    result = Marshal.PtrToStringAnsi(keyData, (int)keyLength).Substring(0, (int)keyLength - 1);
                }
                else
                {
                    var bytes = new byte[keyLength];
                    for (var i = 0; i < keyLength; i++)
                    {
                        bytes[i] = Marshal.ReadByte(keyData, i);
                    }
                    Logger.Warn("GetHostedNetworkKey: Key is in binary format: {0}", BitConverter.ToString(bytes));
                }

                NativeMethods.WlanFreeMemory(keyData);
            }
            else
            {
                Logger.Warn("GetHostedNetworkKey: Key length is 0 or key data was invalid.");
            }

            return result;
        }

        public void SetHostedNetworkKey(string key)
        {
            Logger.Trace("SetHostedNetworkKey: New value: {0}", key);

            var keyLength = Encoding.ASCII.GetByteCount(key);
            Logger.Info("SetHostedNetworkKey: The default encoding character count of {0} is {1}", key, keyLength);

            if (keyLength < 8 || keyLength > 63)
            {
                Logger.Error("SetHostedNetworkKey: The length of key should be within 8 and 63. Ignore it...");
                throw new ArgumentOutOfRangeException("key", "The length of key should be within 8 and 63");
            }

            WlanHostedNetworkReason failReason;

            Logger.Trace("SetHostedNetworkKey: Invoking WlanHostedNetworkSetSecondaryKey...");
            Utilities.ThrowOnError(
                NativeMethods.WlanHostedNetworkSetSecondaryKey(
                    this._wlanHandle,
                    (uint) key.Length + 1,
                    Encoding.ASCII.GetBytes(key),
                    true,
                    true,
                    out failReason,
                    IntPtr.Zero));
            Logger.Trace("SetHostedNetworkKey: WlanHostedNetworkSetSecondaryKey invoked without error");
        }

        public string GetHostedNetworkAuthAlgorithm()
        {
            Logger.Trace("GetHostedNetworkAuthAlgorithm: Invoked");
            string result;

            switch (_securitySettings.Dot11AuthAlgo)
            {
                case Dot11AuthAlgorithm.WEPOpen:
                case Dot11AuthAlgorithm.WPANone:
                    result = "NONE";
                    break;
                case Dot11AuthAlgorithm.WEPSharedKey:
                    result = "WEP";
                    break;
                case Dot11AuthAlgorithm.WPA:
                case Dot11AuthAlgorithm.WPAPSK:
                case Dot11AuthAlgorithm.RSNA:
                case Dot11AuthAlgorithm.RSNAPSK:
                    result = "WPA";
                    break;
                default:
                    result = "UNKNOWN";
                    break;
            }
            Logger.Trace("GetHostedNetworkAuthAlgorithm: Returned: {0}", result);

            return result;
        }

        public void StartHostedNetwork()
        {
            Logger.Trace("StartHostedNetwork: Invoked and ready for locking...");

            Lock();

            Logger.Trace("StartHostedNetwork: Locked");

            try
            {
                WlanHostedNetworkReason failReason;

                if (_hostedNetworkState == WlanHostedNetworkState.Active)
                {
                    Logger.Trace("StartHostedNetwork: A hosted network is already on, forcing it to stop...");

                    Utilities.ThrowOnError(
                        NativeMethods.WlanHostedNetworkForceStop(
                            this._wlanHandle,
                            out failReason,
                            IntPtr.Zero
                            ));

                    Logger.Trace("StartHostedNetwork: Stopped");
                }

                Logger.Trace("StartHostedNetwork: Invoking WlanHostedNetworkStartUsing...");
                Utilities.ThrowOnError(
                    NativeMethods.WlanHostedNetworkStartUsing(
                        this._wlanHandle,
                        out failReason,
                        IntPtr.Zero));
                Logger.Trace("StartHostedNetwork: Hosted network started, ready for enabling ICS");

                if (_icsManager == null || !_icsManager.IsServiceStatusValid)
                {
                    Logger.Error("StartHostedNetwork: ICSManager is invalid or ICS service is in pending state");
                    throw new ICSException("ICSManager is invalid or ICS service is in pending state");
                }
                else
                {
                    var privateGuid = _hostedNetworkInterfaceGuid;
                    var publicGuid = GetPreferredPublicGuid(privateGuid);

                    _icsManager.EnableSharing(publicGuid, privateGuid);
                }
            }
            finally
            {
                Unlock();
                Logger.Trace("StartHostedNetwork: Unlocked");
            }
        }

        public void StopHostedNetwork()
        {
            Logger.Trace("StopHostedNetwork: Invoked and ready for locking...");

            if (_hostedNetworkState != WlanHostedNetworkState.Active)
            {
                Logger.Info("StopHostedNetwork: Hosted network was already off");
                return;
            }

            Lock();

            try
            {
                WlanHostedNetworkReason failReason;

                Logger.Trace("StopHostedNetwork: Invoking WlanHostedNetworkStopUsing...");
                Utilities.ThrowOnError(
                    NativeMethods.WlanHostedNetworkStopUsing(
                        this._wlanHandle,
                        out failReason,
                        IntPtr.Zero));
                Logger.Trace("StopHostedNetwork: Hosted network stopped");
            }
            finally
            {
                Unlock();
                Logger.Trace("StopHostedNetwork: Unlocked");
            }
        }

        private static Guid GetPreferredPublicGuid(Guid privateGuid)
        {
            Logger.Trace("GetPreferredPublicGuid: Invoked");
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            var nic = nics.FirstOrDefault(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                                               && n.OperationalStatus == OperationalStatus.Up) ??
                      nics.FirstOrDefault(n => n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                                               && n.OperationalStatus == OperationalStatus.Up
                                               && new Guid(n.Id) != privateGuid);

            if (nic == null)
            {
                Logger.Error("GetPreferredPublicGuid: No preferred public network is available");
                throw new ApplicationException("No preferred public network is available.");
            }

            Logger.Error("GetPreferredPublicGuid: Preferred GUID: {0}", nic.Id);
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
                                (WlanHostedNetworkStateChange)Marshal.PtrToStructure(notificationData.Data,
                                    typeof(WlanHostedNetworkStateChange));

                            switch (stateChange.NewState)
                            {
                                case WlanHostedNetworkState.Active:
                                    Logger.Info("OnNotification: Hosted network started");
                                    OnHostedNetworkStarted();
                                    break;
                                case WlanHostedNetworkState.Idle:
                                    if (stateChange.OldState == WlanHostedNetworkState.Active)
                                    {
                                        Logger.Info("OnNotification: Hosted network stopped");
                                        OnHostedNetworkStopped();
                                    }
                                    else
                                    {
                                        Logger.Info("OnNotification: Hosted network enabled");
                                        OnHostedNetworkEnabled();
                                    }
                                    break;
                                case WlanHostedNetworkState.Unavailable:
                                    if (stateChange.OldState == WlanHostedNetworkState.Active)
                                    {
                                        Logger.Info("OnNotification: Hosted network stopped");
                                        OnHostedNetworkStopped();
                                    }
                                    Logger.Info("OnNotification: Hosted network disabled");
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
                                        typeof(WlanHostedNetworkDataPeerStateChange));

                            if (peerStateChange.NewState.PeerAuthState == WlanHostedNetworkPeerAuthState.Authenticated)
                            {
                                Logger.Info("OnNotification: New device {0} connected", BitConverter.ToString(peerStateChange.NewState.PeerMacAddress));
                                OnDeviceConnected(peerStateChange.NewState);
                            }
                            if (peerStateChange.NewState.PeerAuthState == WlanHostedNetworkPeerAuthState.Invalid)
                            {
                                Logger.Info("OnNotification: New device {0} disconnected", BitConverter.ToString(peerStateChange.NewState.PeerMacAddress));
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
            this.ConnectedDeviceCount++;

            if (DeviceConnected != null)
            {
                var args = new DeviceConnectedEventArgs(peerState.PeerMacAddress,
                    Convert.ToBoolean(peerState.PeerAuthState));
                DeviceConnected(this, args);
            }
        }

        void OnDeviceDisconnected(byte[] deviceMacAddress)
        {
            this.ConnectedDeviceCount--;

            if (DeviceDisconnected != null)
            {
                var args = new DeviceDisconnectedEventArgs(deviceMacAddress);
                DeviceDisconnected(this, args);
            }
        }
    }
}
