// using System;
// using System.Linq;
// using System.Net.NetworkInformation;
// using System.Threading;
// using Kit;
// using Mirror;
// using Mirror.Discovery;
//
// namespace Game.Networking.Mirror
// {
// 	public class NetworkManager: global::Mirror.NetworkManager, INetworkManager
// 	{
// 		private const string LogCategory = "Mirror";
// 		private const float DiscoveryTimeout = 3.0f;
//
// 		private CancellationTokenSource cancelSource = new CancellationTokenSource();
// 		private NetworkDiscovery discovery;
//
// 		public override void Awake()
// 		{
// 			base.Awake();
//
// 			discovery = GetComponent<NetworkDiscovery>();
// 		}
//
// 		public void Connect()
// 		{
// 			if (IsConnectedToLAN)
// 			{
// 				Debugger.Log(LogCategory, "Starting server discovery on LAN...");
//
// 				discovery.StartDiscovery();
// 				discovery.OnServerFound.AddListener(OnDiscoveredServer);
//
// 				ControlHelper.Delay(DiscoveryTimeout, StartServer, cancelSource.Token);
// 			}
// 			else
// 			{
// 				Debugger.Log(LogCategory, $"Trying to connect to the server at \"{networkAddress}\"...");
// 				StartClient();
// 			}
// 		}
//
// 		public override void OnStopClient()
// 		{
// 			StartServerInternal();
// 		}
//
// 		private void OnDiscoveredServer(ServerResponse response)
// 		{
// 			discovery.StopDiscovery();
// 			cancelSource.Cancel();
//
// 			StartClientInternal(response.uri);
// 		}
//
// 		private void StartServerInternal()
// 		{
// 			Debugger.Log(LogCategory, "No servers found, creating a new one...");
// 			StartServer();
// 			discovery.AdvertiseServer();
// 		}
//
// 		private void StartClientInternal(Uri uri)
// 		{
// 			Debugger.Log(LogCategory, $"Server found at {uri}, connecting...");
// 			StartClient(uri);
// 		}
//
// 		public bool IsSingleplayer => mode == NetworkManagerMode.Offline;
// 		public bool IsClient => mode       == NetworkManagerMode.ClientOnly;
// 		public bool IsServer => mode       == NetworkManagerMode.ServerOnly;
//
// 		private static bool IsConnectedToLAN => NetworkInterface.GetAllNetworkInterfaces()
// 														 .Any(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
// 																   x.OperationalStatus    == OperationalStatus.Up);
// 	}
// }