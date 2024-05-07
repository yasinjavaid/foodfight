using UnityEngine;
using NetworkPlayer = Game.Networking.Bolt.NetworkPlayer;
namespace Game.Items.Weapons
{
	public class RigidProjectile: Projectile
	{
		protected Rigidbody rigidBody;

		protected override void Awake()
		{
			base.Awake();
			rigidBody = GetComponent<Rigidbody>();
		}

		protected override void Start()
		{
			base.Start();
			rigidBody.AddForceAtPosition(transform.forward * Force, transform.position, ForceMode.VelocityChange);
		}

		private void OnCollisionEnter(Collision collision)
		{
			ContactPoint contact = collision.GetContact(0);
			Impact(collision.transform, contact.point, contact.normal, collision.gameObject.tag);
		}
	}
}