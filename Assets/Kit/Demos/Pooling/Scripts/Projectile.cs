using Kit.Behaviours;
using Kit.Pooling;
using UnityEngine;

namespace Demos.Pooling
{
	public class Projectile: PoolWhenInvisible, IPooled
	{
		public float Damage = 25.0f;

		protected float spawnTime;

		public void AwakeFromPool()
		{
			spawnTime = Time.time;

			// We have to reset rotation since Fire2 alters it
			transform.rotation = Quaternion.identity;
		}

		public void OnDestroyIntoPool()
		{
			// Nothing to do when being pooled
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			// Avoid self-collision checks
			if (Time.time - spawnTime < 0.05f)
				return;

			Ship ship = other.GetComponent<Ship>();
			if (ship != null)
			{
				ship.Hit(Damage);
				Pooler.Destroy(this);
			}
		}
	}
}