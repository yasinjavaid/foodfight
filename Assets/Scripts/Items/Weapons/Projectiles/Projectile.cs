using Game.Networking;
using Kit;
using Sirenix.OdinInspector;
using UnityEngine;
using NetworkPlayer = Game.Networking.Bolt.NetworkPlayer;

namespace Game.Items.Weapons
{
	public abstract class Projectile: MonoBehaviour
	{
		public FoodType FoodType;

		[FoldoutGroup("Behaviour")]
		[Min(0)]
		public float Damage = 10.0f;

		[FoldoutGroup("Behaviour")]
		[Min(0)]
		[Tooltip("Reduction of damage per second spent in flight")]
		public float Falloff = 0.0f;

		[FoldoutGroup("Behaviour")]
		[Min(0)]
		[SuffixLabel("seconds")]
		[Tooltip("Time before the projectile goes out of scope")]
		public float Life = 5.0f;

		[FoldoutGroup("Effects")]
		public ParticleSystem ImpactEffect;

		[FoldoutGroup("Effects")]
		public AudioClip ImpactSound;

		public Weapon Weapon { get; set; }
		public float Force { get; set; }

		protected new Transform transform;
		protected float startTime;

		protected virtual void Awake()
		{
			transform = base.transform;
		}

		protected virtual void Start()
		{
			startTime = GameManager.Instance.Time;
			Destroy(gameObject, Life);
		}

		protected virtual void Impact(Transform other, Vector3 position, Vector3 normal, string tag)
		{
#if CLIENT_BUILD
			EffectsManager.Spawn(ImpactEffect, position);
			AudioManager.PlaySound(ImpactSound);
#endif
			Destroy(gameObject);

			Player player = GameManager.GetPlayerFromGO(other.gameObject);
			if (player != null)
				player.Hit(Weapon, CurrentDamage);

			HitInfoForProjectile structForCollissionInfo;
			structForCollissionInfo.hitpoint = position;
			structForCollissionInfo.hitnormal = normal;
			structForCollissionInfo.networkId = NetworkManager.Instance.IsServer ?
				Weapon.Owner.GetComponent<NetworkPlayer>().entity.NetworkId :
				new Photon.Bolt.NetworkId();
			structForCollissionInfo.colliderTag =tag;
			if (!GameManager.Instance.canUseModifier) return;
			ShowModifier(ref structForCollissionInfo);


		}

		public void ShowModifier(ref HitInfoForProjectile collInfo) 
		{
			if (NetworkManager.Instance.IsServer)
			{
				Debug.Log("FireEventSent :hit normal" + collInfo.hitnormal);

				MessageBroker.Instance.Publish(collInfo);
			}
			else if (!NetworkManager.Instance.IsClient &&
					 !NetworkManager.Instance.IsServer &&
					 !NetworkManager.Instance.IsSingleplayer)
			{
				MessageBroker.Instance.Publish(collInfo);
			}
		}
		public float CurrentDamage => Damage - DamageReduction;
		public float DamageReduction => (GameManager.Instance.Time - startTime) * Falloff;
	}
}