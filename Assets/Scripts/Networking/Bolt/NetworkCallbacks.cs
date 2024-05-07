using Photon.Bolt;
using UdpKit;

namespace Game.Networking.Bolt
{
#if SERVER_BUILD
	[BoltGlobalBehaviour(BoltNetworkModes.Server)]
	public class ServerCallbacks: GlobalEventListener
	{
		public override void BoltStartDone()
		{
			base.BoltStartDone();
			Manager.OnServerStarted();
		}

		public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
		{
			base.BoltStartFailed(disconnectReason);
			Manager.OnServerFailed(disconnectReason.ToString());
		}

		public override void Connected(BoltConnection connection)
		{
			base.Connected(connection);
			Manager.OnServerConnected((int) connection.ConnectionId);
		}

		public override void OnEvent(PlayerSpawn ev)
		{
			Manager.Spawn(ev.RaisedBy);
		}

		public NetworkManager Manager => (NetworkManager) Networking.NetworkManager.Instance;
	}
#endif

#if CLIENT_BUILD
	[BoltGlobalBehaviour(BoltNetworkModes.Client)]
	public class ClientCallbacks: GlobalEventListener
	{
		public override void BoltStartDone()
		{
			base.BoltStartDone();
			Manager.OnClientStarted();
		}

		public override void BoltStartFailed(UdpConnectionDisconnectReason disconnectReason)
		{
			base.BoltStartFailed(disconnectReason);
			Manager.OnClientFailed(disconnectReason.ToString());
		}

		public override void Connected(BoltConnection connection)
		{
			base.Connected(connection);
			Manager.OnClientConnected((int) connection.ConnectionId);
		}

		public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
		{
			base.ConnectFailed(endpoint, token);
			Manager.OnClientConnectFailed();
		}

		public override void OnEvent(VersionResponse ev)
		{
			Manager.OnVersionReceived(ev.Major, ev.Minor, ev.Build);
		}

		public NetworkManager Manager => (NetworkManager) Networking.NetworkManager.Instance;
	}
#endif
}