using Kit;
using Sirenix.OdinInspector;
using UnityEngine;
namespace Game.Items.Modifiers
{
	public class Modifier: Item
	{
		[FoldoutGroup("Positioning")]
		public HumanBodyBones AttachBone = HumanBodyBones.Head;

		[FoldoutGroup("Positioning")]
		public Vector3 PositionOffset = Vector3.zero;

		[FoldoutGroup("Positioning")]
		public Vector3 RotationOffset = Vector3.zero;


		[FoldoutGroup("EffectOnPlayer")]
		public GameObject ModifierTakenObjectOnPlayerPrefab;

		public virtual void Attach(Player player)
		{
			GameManager.Instance.canUseModifier = true;
			Transform bone = player.Animator.GetBoneTransform(AttachBone);
			if (bone == null) 
			{
				return;
			}
            if (ModifierTakenObjectOnPlayerPrefab == null)
            {

				Destroy(gameObject);
				return;
            }
			ModifierTakenObjectOnPlayerPrefab.transform.parent = bone;
			ModifierTakenObjectOnPlayerPrefab.transform.localPosition = PositionOffset;
			ModifierTakenObjectOnPlayerPrefab.transform.localRotation = RotationOffset.ToQuaternion();

			Owner = player;
		}

		public virtual void Detach()
		{
			GameManager.Instance.canUseModifier = false;
			transform.parent = null;
			Owner = null;
		}

		public override void Use()
		{
			base.Use();
			

		
			//MessageBroker.Instance.Publish(new WeaponUsed() { Weapon = this });
		}
	}
}