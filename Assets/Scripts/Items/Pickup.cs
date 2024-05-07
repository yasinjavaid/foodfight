using Kit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Items
{
	public class Pickup: MonoBehaviour, IInteractable
	{
		[Required]
		public Item Prefab;

		public Vector3 PositionOffset = Vector3.zero;
		public Vector3 RotationOffset = Vector3.zero;

		public float RotateSpeed = 100.0f;
		public ParticleSystem PickupEffect;
		public AudioClip PickupSound;

		protected new Transform transform;
		protected Item item;

		protected virtual void Awake()
		{
			transform = base.transform;
		}

		protected virtual void Start()
		{
			Spawn();
		}

#if CLIENT_BUILD
		protected void Update()
		{
			transform.Rotate(0, RotateSpeed * Time.deltaTime, 0);
		}
#endif

		protected virtual void Spawn()
		{
			if (Prefab == null)
				return;

			item = Prefab.Spawn();
			Transform itemTransform = item.transform;
			itemTransform.parent = transform;
			itemTransform.localPosition = PositionOffset;
			itemTransform.localRotation = RotationOffset.ToQuaternion();
		}

		public virtual void Interact(Player player)
		{
			PickedUp(player);
			player.Interactables.Remove(this);
		}

		protected virtual void PickedUp(Player player)
		{
			Vector3 position = transform.position;

			EffectsManager.Spawn(PickupEffect, position);
			AudioManager.Play(PickupSound, position);

			item.transform.parent = null;
			Destroy(gameObject);
			item.PickedUp(player);
		}

		public Vector3 Position => transform.position;
	}
}