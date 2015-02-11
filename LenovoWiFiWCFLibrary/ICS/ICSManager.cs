using System;
using System.Collections.Generic;
using System.ServiceProcess;
using NETCONLib;

using NLog;

namespace Lenovo.WiFi.ICS
{
    internal class ICSManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        public IDictionary<Guid, ICSConnection> Connections
        {
            get
            {
                var dictionary = new Dictionary<Guid, ICSConnection>();

                foreach (INetConnection conn in this._netSharingManager.EnumEveryConnection)
                {
                    var icsConnection = new ICSConnection(this._netSharingManager, conn);
                    dictionary.Add(new Guid(icsConnection.InterfaceId), icsConnection);
                    Logger.Trace("Connections: Interface {0} added", icsConnection.InterfaceId);
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
            Logger.Trace("EnableSharing: Public GUID: {0}, Private GUID: {1}", publicGuid, privateGuid);

            if (!this.Connections.ContainsKey(publicGuid))
            {
                Logger.Error("EnableSharing: Public GUID: {0} was not found in connection list", publicGuid);
                throw new ArgumentException("The connection with publicGuid was not found.");
            }

            if (!this.Connections.ContainsKey(privateGuid))
            {
                Logger.Error("EnableSharing: Private GUID: {0} was not found in connection list", privateGuid);
                throw new ArgumentException("The connection with privateGuid was not found.");
            }

            var publicConnection = this.Connections[publicGuid];
            var privateConnection = this.Connections[privateGuid];
            if (publicConnection.IsPublicEnabled
                && privateConnection.IsPrivateEnabled)
            {
                Logger.Trace("EnableSharing: Both connections were enabled");
                return;
            }

            this.DisableAllSharing();
            publicConnection.EnableAsPublic();
            privateConnection.EnableAsPrivate();

            Logger.Trace("EnableSharing: Enabled");
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

            Logger.Trace("DisableAllSharing: Previous sharing disabled");
        }
    }
}
