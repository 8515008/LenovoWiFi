using System;
using System.Runtime.InteropServices;
using System.Security;

namespace HostedNetworkManager.Wlan
{
    [SuppressUnmanagedCodeSecurity()]
    class WlanApi
    {
        const string WlanApiDll = "Wlanapi.dll";

        #region WlanCloseHandle

        [DllImport(WlanApiDll)]
        internal static extern uint WlanCloseHandle(
            [In] IntPtr clientHandle,
            IntPtr reserved);

        #endregion WlanCloseHandle

        #region WlanFreeMemory

        [DllImport(WlanApiDll)]
        internal static extern void WlanFreeMemory(
            [In] IntPtr memory);

        #endregion WlanFreeMemory

        #region WlanHostedNetworkForceStop

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkForceStop(
            [In] WlanHandle clientHandle,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion

        #region WlanHostedNetworkInitSettings

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkInitSettings(
            [In] WlanHandle clientHandle,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion WlanHostedNetworkInitSettings

        #region WlanHostedNetworkQuerySecondaryKey

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkQuerySecondaryKey(
            [In] WlanHandle clientHandle,
            [Out] out uint keyLength,
            [Out] out IntPtr keyData,
            [Out] out bool isPassPhrase,
            [Out] out bool isPersistent,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion WlanHostedNetworkQuerySecondaryKey

        #region WlanHostedNetworkQueryStatus

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkQueryStatus(
            [In] WlanHandle clientHandle,
            [Out] out IntPtr wlanHostedNetworkStatus,
            IntPtr reserved);

        #endregion

        #region WlanHostedNetworkQueryProperty

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkQueryProperty(
            [In] WlanHandle clientHandle,
            [In] WlanHostedNetworkOpcode opCode,
            [Out] out uint dataSize,
            [Out] out IntPtr data,
            [Out] out WlanOpcodeValueType wlanOpcodeValueType,
            IntPtr reserved);

        #endregion WlanHostedNetworkQueryProperty

        #region WlanHostedNetworkQueryProperty

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkSetProperty(
            [In] WlanHandle clientHandle,
            [In] WlanHostedNetworkOpcode opCode,
            uint dataSize,
            IntPtr data,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion WlanHostedNetworkQueryProperty

        #region WlanHostedNetworkSetSecondaryKey

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkSetSecondaryKey(
            [In] WlanHandle clientHandle,
            [In] uint keyLength,
            [In] byte[] keyData,
            [In] bool isPassPhrase,
            [In] bool isPersistent,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion WlanHostedNetworkQueryProperty

        #region WlanHostedNetworkStartUsing

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkStartUsing(
            [In] WlanHandle clientHandle,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion WlanHostedNetworkStartUsing

        #region WlanHostedNetworkStopUsing

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkStopUsing(
            [In] WlanHandle clientHandle,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion WlanHostedNetworkStopUsing

        #region WlanOpenHandle

        [DllImport(WlanApiDll)]
        internal static extern uint WlanOpenHandle(
            [In] WlanApiVersion clientVersion,
            IntPtr reserved,
            [Out] out uint negotiatedVersion,
            out WlanHandle clientHandle);

        #endregion WlanOpenHandle

        #region WlanRegisterNotification


        public delegate void WlanNotificationCallback(WlanNotificationData notificationData, IntPtr context);

        [DllImport(WlanApiDll)]
        internal static extern uint WlanRegisterNotification(
            [In] WlanHandle clientHandle,
            [In] WlanNotificationSource notificationSource,
            [In] bool ignoreDuplicate,
            [In] WlanNotificationCallback notificationCallback,
            [In] IntPtr callbackContext,
            IntPtr reserved,
            [Out] out WlanNotificationSource previousNotificationSource);

        #endregion
    }
}
