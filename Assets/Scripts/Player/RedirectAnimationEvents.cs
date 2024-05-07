using UnityEngine;

namespace Game
{
	public class RedirectAnimationEvents : MonoBehaviour
	{
		public Player Player;

		private void TriggerFire()
		{
			Player.TriggerFire();
		}

		private void TriggerReload()
		{
			Player.TriggerReload();
		}

		private void TriggerDropWeapon()
		{
			Player.TriggerDropWeapon();
		}

		private void TriggerPickupWeapon()
		{
			Player.TriggerWeaponPickup();
		}
		private void TriggerPickupModifier()
		{
			Player.TriggerModifierPickup();
		}
	}
}
