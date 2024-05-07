using Kit;
using Photon.Bolt;
using Photon.Bolt.LagCompensation;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Game.Networking.Bolt
{
	public class NetworkManager: Networking.NetworkManager
	{
		public override string LogCategory => "Bolt";
		public const ushort ServerPort = 54321;
		public const ushort ClientPort = 0;

		public BoltConnection ClientConnection { get; private set; }

		private BoltRuntimeSettings settings;
		private BoltConfig configuration;
		private Logger logger;

		public override void OnClientConnected(int clientID)
		{
			base.OnClientConnected(clientID);
			BoltNetwork.FindConnection((uint) clientID, out BoltConnection clientConnection);
			ClientConnection = clientConnection;
		}

		protected override void Configure()
		{
			settings = BoltRuntimeSettings.instance;

			configuration = settings.GetConfigCopy();
			configuration.connectionTimeout = 60000000;
			configuration.connectionRequestTimeout = 500;
			configuration.connectionRequestAttempts = 1000;

			logger = new Logger();
			BoltLog.Add(logger);
		}

		public override void StartServer()
		{
			StartServer(configuration, ServerPort);
		}

		public override void StartClient()
		{
			StartClient(configuration, ClientPort);
		}

		public void StartServer(BoltConfig config, ushort port)
		{
			Debugger.Log(LogCategory, $"Starting server at {port}...");
			UdpEndPoint serverEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, port);
			BoltLauncher.StartServer(serverEndPoint, config);
		}

		public void StartClient(BoltConfig config, ushort port)
		{
			Debugger.Log(LogCategory, $"Starting client at {port}...");
			UdpEndPoint clientEndPoint = new UdpEndPoint(UdpIPv4Address.Localhost, port);
			BoltLauncher.StartClient(clientEndPoint, config);
		}

		public override void ConnectToServer()
		{
			ConnectToServer(ServerPort);
		}

		public void ConnectToServer(ushort port)
		{
			Debugger.Log(LogCategory, $"Connecting to server at {port}...");
			BoltNetwork.Connect(port);
		}

		public override void SendVersion(int clientID)
		{
			Debugger.Log(LogCategory, $"Sending version info to client {clientID}...");
			BoltNetwork.FindConnection((uint) clientID, out BoltConnection connection);
			VersionResponse.Post(connection, BuildManager.Version.Major, BuildManager.Version.Minor, BuildManager.Version.Build);
		}

		public override void Spawn()
		{
			PlayerSpawn.Post(GlobalTargets.OnlyServer, ReliabilityModes.ReliableOrdered);
		}

		public INetworkPlayer Spawn(BoltConnection connection)
		{
			SpawnPoint spawnPoint = GameManager.Instance.GetSpawnLocation();
			Debugger.Log(LogCategory, $"Spawning player for {connection.ConnectionId} at {spawnPoint.Position}...");
			BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player_Bolt, spawnPoint.Position, spawnPoint.Rotation);
			NetworkPlayer player = entity.GetComponent<NetworkPlayer>();
			entity.AssignControl(connection);
			player.name = $"Player ({entity.Controller.ConnectionId})";
			return player;
		}

		public override int Raycast(Ray ray, ref GenericHit[] hits, int frame = -1)
		{
			return ConvertBoltToGenericHits(BoltNetwork.RaycastAll(ray, frame), ref hits);
		}

		public override int Overlap(Vector3 origin, float radius, ref GenericHit[] hits, int frame = -1)
		{
			return ConvertBoltToGenericHits(BoltNetwork.OverlapSphereAll(origin, radius, frame), ref hits);
		}

		private static int ConvertBoltToGenericHits(BoltPhysicsHits boltHits, ref GenericHit[] hits)
		{
			int count = Mathf.Min(boltHits.count, hits.Length);
			for (int i = 0; i < count; i++)
			{
				BoltPhysicsHit boltHit = boltHits.GetHit(i);
				hits[i] = new GenericHit() { Distance = boltHit.distance, Transform = boltHit.body.transform };
			}
			return boltHits.count;
		}

		public override bool IsRunning => BoltNetwork.IsRunning;
		public override bool IsConnected => BoltNetwork.IsConnected;
		public override bool IsServer => BoltNetwork.IsServer;
		public override bool IsClient => BoltNetwork.IsClient;
		public override bool IsSingleplayer => BoltNetwork.IsSinglePlayer;
		public override float Time => BoltNetwork.ServerTime;
		public override int Frame => BoltNetwork.ServerFrame;
		public BoltConnection ServerConnection => BoltNetwork.Server;
	}
}