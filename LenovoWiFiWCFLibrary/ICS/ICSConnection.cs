using System;
using NETCONLib;

namespace Lenovo.WiFi.ICS
{
    internal class ICSConnection
    {
        readonly INetConnectionProps _netConnectionProperties;
        readonly INetSharingConfiguration _netSharingConfiguration;

        internal ICSConnection(INetSharingManager nsManager, INetConnection netConnection)
        {
            if (nsManager == null)
            {
                throw new ArgumentNullException("nsManager");
            }

            if (netConnection == null)
            {
                throw new ArgumentNullException("netConnection");
            }

            this._netConnectionProperties = nsManager.NetConnectionProps[netConnection];
            this._netSharingConfiguration = nsManager.INetSharingConfigurationForINetConnection[netConnection];
        }

        internal string InterfaceId
        {
            get { return this._netConnectionProperties.Guid; }
        }

        internal bool IsSupported
        {
            get { return _netConnectionProperties.MediaType == tagNETCON_MEDIATYPE.NCM_LAN; }
        }

        internal bool IsAvailable
        {
            get { return _netConnectionProperties.Status != tagNETCON_STATUS.NCS_DISCONNECTED; }
        }

        internal bool IsEnabled
        {
            get
            {
                return this._netSharingConfiguration.SharingEnabled;
            }
        }

        internal bool IsPublicEnabled
        {
            get
            {
                return this._netSharingConfiguration.SharingEnabled
                    && this._netSharingConfiguration.SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC;
            }
        }

        internal bool IsPrivateEnabled
        {
            get
            {
                return this._netSharingConfiguration.SharingEnabled
                    && this._netSharingConfiguration.SharingConnectionType == tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE;
            }
        }

        internal void EnableAsPublic()
        {
            this.DisableSharing();
            this._netSharingConfiguration.EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
        }

        internal void EnableAsPrivate()
        {
            this.DisableSharing();
            this._netSharingConfiguration.EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
        }

        internal void DisableSharing()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            this._netSharingConfiguration.DisableSharing();
        }
    }
}
