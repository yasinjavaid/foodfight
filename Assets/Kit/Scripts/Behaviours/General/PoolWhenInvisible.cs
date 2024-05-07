using Kit.Pooling;
using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Pool the object when it stops rendering on-screen.</summary>
	public class PoolWhenInvisible: MonoBehaviour
	{
		protected virtual void OnBecameInvisible()
		{
			Pooler.Destroy(this);
		}
	}
}