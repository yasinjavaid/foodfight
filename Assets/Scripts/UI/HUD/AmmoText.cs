using System;
using Cysharp.Threading.Tasks.Linq;
using Game.Items;
using Game.Items.Weapons;
using Kit;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.HUD
{
	public class AmmoText: MonoBehaviour
	{
		private Text text;
		private CompositeDisposable disposables = new CompositeDisposable();

		private void Awake()
		{
			text = GetComponent<Text>();
			Attach();
		}

		private void Attach()
		{
			disposables.Add(MessageBroker.Instance.Receive<WeaponEquipped>().Subscribe(msg => OnAmmoChanged(msg.Player)));
			disposables.Add(MessageBroker.Instance.Receive<AmmoChanged>().Subscribe(msg => OnAmmoChanged(msg.Player)));
		}

		private void OnAmmoChanged(Player player)
		{
			if (!GameManager.Instance.IsControlling)
				return;

			if (player != GameManager.Instance.Player)
				return;

			if (!player.HasWeapon)
				return;

			Refresh();
		}

		private void Refresh()
		{
			Weapon weapon = GameManager.Instance.Player.CurrentWeapon;
			text.text = $"{weapon.Ammo} / {weapon.Magazine}";
		}

		private void OnDestroy()
		{
			disposables.Dispose();
		}
	}
}