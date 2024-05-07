using Game.Networking;
using Kit;
using Photon.Bolt;
using UnityEngine;
using NetworkPlayer = Game.Networking.Bolt.NetworkPlayer;

namespace Game.Items.Weapons
{
	public class RayProjectile: Projectile
	{
		//public GenericHit[] hits = new GenericHit[1];
		//public float OverlapRadius = 0.1f;

		protected Vector3 increment;

		protected override void Start()
		{
			base.Start();
			increment = transform.forward * (Force * GameManager.Instance.FixedDeltaTime);
		}

		private void FixedUpdate()
		{
			Move();
			CheckForImpact();
		}

		private void Move()
		{
			transform.position += increment;
		}

		private void CheckForImpact()
		{
			Vector3 start = transform.position;
			Vector3 end = start + increment;

#if UNITY_EDITOR
			Debug.DrawLine(start, end, Color.red);
#endif

			if (Physics.Linecast(start, end, out RaycastHit hit))
			{
				Impact(hit.transform, hit.point, hit.normal, hit.collider.tag);
			
				Debugger.Log("hit",LogType.Log);
			
			}
			

			//if (GameManager.Instance.Overlap(start, OverlapRadius, ref hits) > 0)
			//{
			//	GenericHit hit = hits[0];
			//	Impact(hit.Transform, start, transform.forward);
			//}
		}
	}
}