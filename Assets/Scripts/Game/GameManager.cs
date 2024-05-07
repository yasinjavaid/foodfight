using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Linq;
using Game.Networking;
using Kit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
	public enum SpawnStrategy
	{
		Random,
		RoundRobin
	}

	public enum NetworkMode
	{
		Offline,
		Editor,
		Executable
	}

	public struct GenericHit
	{
		public Transform Transform;
		public float Distance;
	}

	public class GameManager: Singleton<GameManager>
	{
		public const string LogCategory = "Game";

		public Player PlayerPrefab;
		public PlayerInput InputPrefab;

		public SpawnStrategy SpawnStrategy = SpawnStrategy.RoundRobin;
		public List<SpawnPoint> SpawnPoints { get; } = new List<SpawnPoint>();
		private int spawnIndex = 0;

		public Player Player { get; private set; }
		public PlayerInput PlayerInput { get; private set; }
		public NetworkMode NetworkMode { get; private set; }

        public bool canUseModifier { get; set; }

        private CompositeDisposable disposables = new CompositeDisposable();

		public bool isFriendlyFire { get; set; }
		private void Awake()
		{
			Application.targetFrameRate = 60;
			ToggleCursor(false);
		}

		private void Start()
		{
			isFriendlyFire = false;
			SortSpawnPoints();
			CalculateNetworkMode();
			if (NetworkMode != NetworkMode.Offline)
				Connect();
			else
				Spawn();
		}

		private void CalculateNetworkMode()
		{
			if (NetworkManager.Instance == null)
				NetworkMode = NetworkMode.Offline;
			else if (NetworkManager.Instance.IsConnected)
				NetworkMode = NetworkMode.Editor;
			else
			{
#if UNITY_EDITOR
				NetworkMode = NetworkMode.Offline;
#else
				NetworkMode = NetworkMode.Executable;
#endif
			}

			Debugger.Log(LogCategory, $"Running in {NetworkMode} network mode.");
			if (NetworkMode != NetworkMode.Offline)
				Debugger.Log(LogCategory, $"Using {NetworkManager.Instance} backend.");
		}

		private void Connect()
		{
			if (NetworkMode == NetworkMode.Executable)
			{
				NetworkManager.Instance.Connect();
				disposables.Add(MessageBroker.Instance.Receive<ServerStarted>().Subscribe(_ => PositionWindow()));
				disposables.Add(MessageBroker.Instance.Receive<ClientConnected>().Subscribe(_ => PositionWindow()));
				disposables.Add(MessageBroker.Instance.Receive<VersionReceived>().Subscribe(msg => OnVersionReceived(msg.Version)));
			}
			else // Already Connected via BoltDebugStart?
			{
				if (NetworkManager.Instance.IsClient)
					Spawn();
			}
		}

		private void PositionWindow()
		{
			if (NetworkMode == NetworkMode.Executable)
				CustomDebugStart.PositionWindow();
		}

		public void OnVersionReceived(Version version)
		{
			Debugger.Log(LogCategory, $"Game version: {BuildManager.Version}.");
			if (BuildManager.MatchVersion(version))
			{
				Debugger.Log(LogCategory, "Versions matched.");
				Spawn();
			}
			else
				Debugger.Log(LogCategory, "Couldn't match server and client versions.");
		}

		private void Spawn()
		{
			if (NetworkMode != NetworkMode.Offline)
				NetworkManager.Instance.Spawn();
			else
				SpawnOfflinePlayer();
		}

		private void SpawnOfflinePlayer()
		{
			SpawnPoint spawnPoint = GetSpawnLocation();
			Debugger.Log(LogCategory, $"Spawning offline player at {spawnPoint.Position}...");
			Player player = Instantiate(PlayerPrefab, spawnPoint.Position, spawnPoint.Rotation);
			player.name = "Player (Offline)";
			GainControl(player);
		}

		private void Update()
		{
#if UNITY_EDITOR
			DrawAim();
#endif

#if DEVELOPMENT_BUILD
			if (Keyboard.current.escapeKey.wasPressedThisFrame)
				ToggleCursor(true);

			if (Mouse.current.leftButton.wasReleasedThisFrame)
				ToggleCursor(false);
#endif
		}

		private void OnApplicationFocus(bool hasFocus)
		{
#if DEVELOPMENT_BUILD
			ToggleCursor(!hasFocus);
#endif
		}

		private static void ToggleCursor(bool value)
		{
			Cursor.visible = value;
			Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Confined;
		}

		private void OnApplicationQuit()
		{
			disposables.Dispose();
			MessageBroker.Instance.Dispose();
			if (NetworkMode != NetworkMode.Offline)
				Disconnect();
		}

		private void Disconnect()
		{
			NetworkManager.Instance.Disconnect();
		}

#if SERVER_BUILD
		#region Spawn Points
		public void RegisterSpawnPoint(SpawnPoint spawnPoint)
		{
			SpawnPoints.Add(spawnPoint);
		}

		public void UnregisterSpawnPoint(SpawnPoint spawnPoint)
		{
			SpawnPoints.Remove(spawnPoint);
		}

		public void SortSpawnPoints()
		{
			SpawnPoints.Sort((s1, s2) => s1.Transform.GetSiblingIndex() - s2.Transform.GetSiblingIndex());
		}

		public SpawnPoint GetSpawnLocation()
		{
			SpawnPoint spawnPoint = null;
			switch (SpawnStrategy)
			{
				case SpawnStrategy.Random:
				{
					spawnPoint = SpawnPoints.GetRandom();
					break;
				}

				case SpawnStrategy.RoundRobin:
				{
					spawnPoint = SpawnPoints[spawnIndex];
					spawnIndex++;
					if (spawnIndex >= SpawnPoints.Count)
						spawnIndex = 0;
					break;
				}
			}
			return spawnPoint;
		}
		#endregion
#endif

		#region Player
#if CLIENT_BUILD
		public void GainControl(Player player)
		{
			Player = player;
			AttachInput(player);
			CameraManager.Instance.Attach(player);
			MessageBroker.Instance.Publish(new GainedControl { Player = Player });
			Debugger.Log(LogCategory, $"Gained control of player \"{player.name}\".");
		}

		private void AttachInput(Player player)
		{
			PlayerInput = InputPrefab.CopyTo(Player.gameObject);
			PlayerInput.ActivateInput();
			if (NetworkMode == NetworkMode.Offline)
				player.gameObject.AddComponent<OfflineInput>();
		}

		public void LoseControl()
		{
			Player player = Player;
			CameraManager.Instance.Detach();
			InputPrefab = null;
			Player = null;
			MessageBroker.Instance.Publish(new LostControl { Player = player });
		}
#endif

		public void ActivateInput(Player player)
		{
#if CLIENT_BUILD
			if (Player == player)
				PlayerInput.ActivateInput();
#endif
		}

		public void DeactivateInput(Player player)
		{
#if CLIENT_BUILD
			if (Player == player)
				PlayerInput.DeactivateInput();
#endif
		}

		public static Player GetPlayerFromGO(GameObject gameObject)
		{
			return gameObject.GetComponent<Player>();
		}

#if UNITY_EDITOR
		private void DrawAim()
		{
			if (IsControlling && Player.HasWeapon)
			{
				Transform muzzle = Player.CurrentWeapon.Muzzle;
				if (muzzle == null)
					return;
				Debug.DrawLine(Player.LocalAimLocation, muzzle.position, Color.green);
			}
		}
#endif

		public bool IsControlling => Player != null;

		#endregion

		#region Gameplay

		public int Raycast(Ray ray, ref GenericHit[] hits, int frame = -1)
		{
			if (NetworkMode != NetworkMode.Offline)
				return NetworkManager.Instance.Raycast(ray, ref hits, frame);
			else
				return ConvertUnityToGenericHits(Physics.RaycastAll(ray), ref hits);
		}

		public int Overlap(Vector3 origin, float radius, ref GenericHit[] hits, int frame = -1)
		{
			if (NetworkMode != NetworkMode.Offline)
				return NetworkManager.Instance.Overlap(origin, radius, ref hits, frame);

			else
				return ConvertCollidersToGenericHits(Physics.OverlapSphere(origin, radius), ref hits);
		}

		private static int ConvertCollidersToGenericHits(Collider[] colliders, ref GenericHit[] hits)
		{
			int count = Mathf.Min(colliders.Length, hits.Length);
			for (int i = 0; i < count; i++)
			{
				Collider collider = colliders[i];
				hits[i] = new GenericHit() { Transform = collider.transform };
			}
			return colliders.Length;
		}

		private static int ConvertUnityToGenericHits(RaycastHit[] unityHits, ref GenericHit[] hits)
		{
			int count = Mathf.Min(unityHits.Length, hits.Length);
			for (int i = 0; i < count; i++)
			{
				RaycastHit unityHit = unityHits[i];
				hits[i] = new GenericHit() { Distance = unityHit.distance, Transform = unityHit.transform };
			}
			return unityHits.Length;
		}

		public float DeltaTime => UnityEngine.Time.deltaTime;
		public float FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;
		public float Time => NetworkMode == NetworkMode.Offline ? UnityEngine.Time.time : NetworkManager.Instance.Time;

		#endregion
	}
}