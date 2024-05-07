using Game.Items.Weapons;

namespace Game.Items
{
	public class WeaponPickup: Pickup
	{
		protected override void PickedUp(Player player)
		{
			base.PickedUp(player);
			Weapon weapon = (Weapon) item;
			player.Pickup(weapon);
		}
	}
}