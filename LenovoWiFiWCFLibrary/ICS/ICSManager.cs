using System;
using System.Collections.Generic;
using System.ServiceProcess;
using NETCONLib;

namespace Lenovo.WiFi.ICS
{
    internal class ICSManager : IDisposable
    {
        private readonly INetSharingManager _netSharingManager;
        private readonly ServiceController _icsService;

        internal ICSManager()
        {
            _netSharingManager = new NetSharingManager();

            if (!_netSharingManager.SharingInstalled)
            {
                throw new ICSException("The operating system doesn't support connection sharing.");
            }

            _icsService = new ServiceController("SharedAccess");
        }

        public void Dispose()
        {
            _icsService.Dispose();
        }

        public Dictionary<Guid, ICSConnection> Connections
        {
            get
            {
                var dictionary = new Dictionary<Guid, ICSConnection>();

                foreach (INetConnection conn in this._netSharingManager.EnumEveryConnection)
                {
                    var icsConnection = new ICSConnection(this._netSharingManager, conn);
                    dictionary.Add(new Guid(icsConnection.InterfaceId), icsConnection);
                }

                return dictionary;
            }
        }

        internal bool IsServiceStatusValid
        {
            get
            {
                return _icsService.Status != ServiceControllerStatus.StartPending
                       && _icsService.Status != ServiceControllerStatus.StopPending;
            }
        }

        internal void EnableSharing(Guid publicGuid, Guid privateGuid)
        {
            if (!this.Connections.ContainsKey(publicGuid))
            {
                throw new ArgumentException("The connection with publicGuid was not found.");
            }

            if (!this.Connections.ContainsKey(privateGuid))
            {
                throw new ArgumentException("The connection with privateGuid was not found.");
            }

            var publicConnection = this.Connections[publicGuid];
            var privateConnection = this.Connections[privateGuid];
            if (publicConnection.IsPublicEnabled
                && privateConnection.IsPrivateEnabled)
            {
                return;
            }

            this.DisableAllSharing();
            publicConnection.EnableAsPublic();
            privateConnection.EnableAsPrivate();
        }

        private void DisableAllSharing()
        {
            foreach (var connection in this.Connections.Values)
            {
                if (connection.IsSupported)
                {
                    connection.DisableSharing();
                }
            }
        }
    }
}
