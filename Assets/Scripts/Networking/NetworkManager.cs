using System;
using Kit;
using UnityEngine;

namespace Game.Networking
{
	public struct ServerStarted
	{
	}

	public struct ServerConnected
	{
		public int ClientID;
	}

	public struct ClientConnected
	{
	}

	public struct VersionReceived
	{
		public Version Version;
	}

	public abstract class NetworkManager
	{
		public static NetworkManager Instance = new Bolt.NetworkManager();
		public virtual string LogCategory => "Network";
		public const string ServerCheckFile = "Server.tmp";

		public virtual int ClientID { get; protected set; } = -1;

		private bool serverFileCreated = false;

		public virtual void Connect()
		{
			Configure();
			if (!IsServerRunning)
				StartServer();
			else
				StartClient();
		}

		public virtual bool IsServerRunning => ResourceManager.Exists(ResourceFolder.Data, ServerCheckFile);

		public virtual void OnServerStarted()
		{
			Debugger.Log(LogCategory, "Server started.");
			ResourceManager.SaveText(ResourceFolder.Data, ServerCheckFile, "");
			serverFileCreated = true;
			MessageBroker.Instance.Publish(new ServerStarted());
		}

		public virtual void OnServerFailed(string reason)
		{
			Debugger.Log(LogCategory, $"Server couldn't start: {reason}");
		}

		public virtual void OnClientStarted()
		{
			Debugger.Log(LogCategory, "Client started.");
			ConnectToServer();
		}

		public virtual void OnClientFailed(string reason)
		{
			Debugger.Log(LogCategory, $"Client couldn't start: {reason}", LogType.Error);
		}

		public virtual void OnServerConnected(int clientID)
		{
			Debugger.Log(LogCategory, $"Server connected to client {clientID}.");
				SendVersion(clientID);
			MessageBroker.Instance.Publish(new ServerConnected() {ClientID = clientID});
		}

		public virtual void OnClientConnected(int clientID)
		{
			Debugger.Log(LogCategory, $"Client connected to server as client {clientID}.");
			ClientID = clientID;
			MessageBroker.Instance.Publish(new ClientConnected());
		}

		public virtual void OnClientConnectFailed()
		{
			Debugger.Log(LogCategory, "Client couldn't connect to server.", LogType.Error);
		}

		public virtual void OnVersionReceived(int major, int minor, int build)
		{
			Version version = build > 0 ? new Version(major, minor, build) : new Version(major, minor);
			Debugger.Log(LogCategory, $"Server version received: {version}.");
			MessageBroker.Instance.Publish(new VersionReceived() { Version = version});
		}

		public virtual void Disconnect()
		{
			if (serverFileCreated)
				ResourceManager.Delete(ResourceFolder.Data, ServerCheckFile);
		}

		protected abstract void Configure();
		public abstract void SendVersion(int clientID);
		public abstract void StartServer();
		public abstract void StartClient();
		public abstract void ConnectToServer();
		public abstract void Spawn();
		public abstract bool IsConnected { get; }
		public abstract bool IsRunning { get; }
		public abstract bool IsSingleplayer { get; }
		public abstract bool IsServer { get; }
		public abstract bool IsClient { get; }
		public abstract float Time { get; }
		public abstract int Frame { get; }
		public abstract int Raycast(Ray ray, ref GenericHit[] hits, int frame = -1);
		public abstract int Overlap(Vector3 origin, float radius, ref GenericHit[] hits, int frame = -1);
	}
}