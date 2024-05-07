using Kit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Items.Weapons
{
	public struct WeaponReloaded
	{
		public Weapon Weapon;
	}

	public struct WeaponUsed
	{
		public Weapon Weapon;
	}

	public class Weapon: Item
	{
		[FoldoutGroup("Positioning")]
		public HumanBodyBones AttachBone = HumanBodyBones.RightHand;

		[FoldoutGroup("Positioning")]
		public Vector3 PositionOffset = Vector3.zero;

		[FoldoutGroup("Positioning")]
		public Vector3 RotationOffset = Vector3.zero;

		[FoldoutGroup("Positioning")]
		public Transform Muzzle;

		[FoldoutGroup("Functionality")]
		[Required]
		public Projectile Projectile;

		[FoldoutGroup("Functionality")]
		public bool Autofire;

		[FoldoutGroup("Functionality")]
		public int Magazine = 10;

		[FoldoutGroup("Functionality")]
		public float Force = 10.0f;

		public virtual void Attach(Player player)
		{
			Transform bone = player.Animator.GetBoneTransform(AttachBone);
			if (bone == null)
				return;

			transform.parent = bone;
			transform.localPosition = PositionOffset;
			transform.localRotation = RotationOffset.ToQuaternion();

			Owner = player;
			Refill();
		}

		public virtual void Detach()
		{
			transform.parent = null;
			Owner = null;
		}

		public override void Use()
		{
			base.Use();
			if (Projectile == null)
				return;

			FireProjectile();

			Ammo--;
			MessageBroker.Instance.Publish(new WeaponUsed() { Weapon = this });
		}

		protected virtual void FireProjectile()
		{
			Vector3 position = Muzzle == null ? transform.position : Muzzle.position;
			Quaternion rotation = Quaternion.LookRotation(Owner.State.AimLocation - position);
			FireProjectile(position, rotation);
		}

		protected virtual void FireProjectile(Vector3 position, Quaternion rotation)
		{
			Projectile projectile = Instantiate(Projectile, position, rotation);
			projectile.Weapon = this;
			projectile.Force = Force;
		}

		protected virtual void Refill()
		{
			Ammo = Magazine;
		}

		public virtual void Reload()
		{
			Refill();
			MessageBroker.Instance.Publish(new WeaponReloaded() { Weapon = this });
		}

		public int Ammo { get => Owner.State.Ammo; set => Owner.State.Ammo = value; }
		public bool HasAmmo => Owner.HasAmmo;
		public virtual bool CanReload => Ammo < Magazine;
		public override bool CanUse => HasAmmo;
	}
}