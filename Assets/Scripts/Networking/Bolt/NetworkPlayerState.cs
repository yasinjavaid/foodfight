using UnityEngine;

namespace Game.Networking.Bolt
{
	public class LocalPlayerState: PlayerState
	{
		private NetworkPlayer player;
		private Photon.Bolt.IPlayerState state;

		public LocalPlayerState(NetworkPlayer player)
		{
			this.player = player;
			state = player.state;
		}

		public override float Health
		{
			get => state.Health;
			set { }
		}

		public override int Ammo
		{
			get => state.Ammo;
			set { }
		}
	}

	public class ServerPlayerState: PlayerState
	{
		private NetworkPlayer player;
		private Photon.Bolt.IPlayerState state;

		public ServerPlayerState(NetworkPlayer player)
		{
			this.player = player;
			state = player.state;
		}

		public override Vector3 Movement
		{
			get => state.Movement;
			set => state.Movement = value;
		}

		public override bool IsGrounded
		{
			get => state.IsGrounded;
			set => state.IsGrounded = value;
		}

		public override bool IsSprinting
		{
			get => state.IsSprinting;
			set => state.IsSprinting = value;
		}

		public override bool IsAiming
		{
			get => state.IsAiming;
			set => state.IsAiming = value;
		}

		public override Vector3 AimLocation
		{
			get => state.AimLocation;
			set => state.AimLocation = value;
		}

		public override float Health
		{
			get => state.Health;
			set => state.Health = value;
		}

		public override int Ammo
		{
			get => state.Ammo;
			set => state.Ammo = value;
		}
	}

	public class ObserverPlayerState: PlayerState
	{
		private NetworkPlayer player;
		private Photon.Bolt.IPlayerState state;

		public ObserverPlayerState(NetworkPlayer player)
		{
			this.player = player;
			state = player.state;
		}

		public override Vector3 Movement
		{
			get => state.Movement;
			set { }
		}

		public override bool IsGrounded
		{
			get => state.IsGrounded;
			set { }
		}

		public override bool IsSprinting
		{
			get => state.IsSprinting;
			set { }
		}

		public override bool IsAiming
		{
			get => state.IsAiming;
			set { }
		}

		public override Vector3 AimLocation
		{
			get => state.AimLocation;
			set { }
		}

		public override float Health
		{
			get => state.Health;
			set { }
		}

		public override int Ammo
		{
			get => state.Ammo;
			set { }
		}
	}
}