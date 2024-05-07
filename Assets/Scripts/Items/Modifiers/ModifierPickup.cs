using Game.Items.Modifiers;
namespace Game.Items
{
	public class ModifierPickup : Pickup
	{
		protected override void PickedUp(Player player)
		{
			base.PickedUp(player);
			Modifier modifier = (Modifier)item;
			player.Pickup(modifier);
		}
	}
}

