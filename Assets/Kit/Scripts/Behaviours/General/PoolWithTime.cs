using System.Threading;
using Kit.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Pools the <see cref="GameObject" /> after a specified time.</summary>
	public class PoolWithTime: MonoBehaviour, IPooled
	{
		/// <summary>Time to hold out for in seconds before pooling.</summary>
		[Tooltip("Time to hold out for before pooling.")]
		[SuffixLabel("seconds", true)]
		public float Time = 5.0f;

		protected CancellationTokenSource cancelSource;

		public virtual void AwakeFromPool()
		{
			StartTimer();
		}

		public virtual void OnDestroyIntoPool()
		{
			Cancel();
		}

		protected virtual void OnDestroy()
		{
			Cancel();
		}

		public virtual void ChangeTime(float time)
		{
			Time = time;
			RestartTimer();
		}

		public virtual void RestartTimer()
		{
			Cancel();
			StartTimer();
		}

		protected virtual void StartTimer()
		{
			cancelSource = new CancellationTokenSource();
			ControlHelper.Delay(Time, Pool, cancelSource.Token);
		}

		protected virtual void Cancel()
		{
			if (cancelSource != null)
			{
				cancelSource.Cancel();
				cancelSource.Dispose();
				cancelSource = null;
			}
		}

		public virtual void Pool()
		{
			Pooler.Destroy(this);
		}
	}
}