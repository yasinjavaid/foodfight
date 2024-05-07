using UnityEngine;

namespace Game
{
	public class OfflinePlayerState: PlayerState
	{
		private Player player;

		public OfflinePlayerState(Player player)
		{
			this.player = player;
		}

		public override bool IsGrounded
		{
			get => player.LocalIsGrounded;
			set {}
		}

		public override Vector3 AimLocation
		{
			get => player.LocalAimLocation;
			set { }
		}

		public override int Ammo
		{
			get => base.Ammo;
			set
			{
				base.Ammo = value;
				MessageBroker.Instance.Publish(new AmmoChanged() { Player = player });
				if (!player.HasAmmo)
					MessageBroker.Instance.Publish(new AmmoEmptied() { Player = player });
			}
		}
	}
}