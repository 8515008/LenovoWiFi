using System;
using System.Runtime.InteropServices;

namespace HostedNetworkManager.Wlan
{
    class WlanApi
    {
        const string WlanApiDll = "Wlanapi.dll";

        #region WlanHostedNetworkInitSettings

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkInitSettings(
            [In] IntPtr clientHandle,
            [Out] out WlanHostedNetworkReason failReason,
            IntPtr reserved);

        #endregion WlanHostedNetworkInitSettings

        #region WlanHostedNetworkQueryStatus

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkQueryStatus(
            [In] IntPtr clientHandle,
            [Out] out IntPtr wlanHostedNetworkStatus,
            IntPtr reserved);

        #endregion

        #region WlanHostedNetworkQueryProperty

        [DllImport(WlanApiDll)]
        internal static extern uint WlanHostedNetworkQueryProperty(
            [In] IntPtr clientHandle,
            [In] WlanHostedNetworkOpcode opCode,
            [Out] out uint dataSize,
            [Out] out IntPtr data,
            [Out] out WlanOpcodeValueType wlanOpcodeValueType,
            IntPtr reserved);

        #endregion WlanHostedNetworkQueryProperty

        #region WlanOpenHandle

        [DllImport(WlanApiDll)]
        internal static extern uint WlanOpenHandle(
            [In] WlanApiVersion clientVersion,
            IntPtr reserved,
            [Out] out uint negotiatedVersion,
            out IntPtr clientHandle);

        #endregion WlanOpenHandle

        #region WlanRegisterNotification


        public delegate void WlanNotificationCallback(WlanNotificationData notificationData, IntPtr context);

        [DllImport(WlanApiDll)]
        internal static extern uint WlanRegisterNotification(
            [In] IntPtr clientHandle,
            [In] WlanNotificationSource notificationSource,
            [In] bool ignoreDuplicate,
            [In] WlanNotificationCallback notificationCallback,
            [In] IntPtr callbackContext,
            IntPtr reserved,
            [Out] out WlanNotificationSource previousNotificationSource);

        #endregion
    }
}
