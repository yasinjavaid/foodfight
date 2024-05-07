using Game.Items;
using Game.Items.Weapons;

namespace Game
{
	public struct PlayerHit
	{
		public Player Player;
		public Item With;
	}

	public struct PlayerKilled
	{
		public Player Player;
		public Item With;
	}

	public struct WeaponEquipped
	{
		public Player Player;
		public Weapon Weapon;
	}

	public struct AmmoChanged
	{
		public Player Player;
	}

	public struct AmmoEmptied
	{
		public Player Player;
	}
}