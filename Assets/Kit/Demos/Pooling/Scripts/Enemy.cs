using System;
using System.Threading;
using Cysharp.Threading.Tasks.Linq;
using Kit;
using Kit.Pooling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Demos.Pooling
{
	public class Enemy: Ship
	{
		public float Delay = 3.0f;
		public float Interval = 2.0f;

		protected new Transform transform;

		protected CancellationTokenSource cancelSource = new CancellationTokenSource();

		private void Awake()
		{
			transform = base.transform;

			UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(Random.Range(0.0f, Delay)), TimeSpan.FromSeconds(Interval))
								  .ForEachAsync(_ => Fire(), cancelSource.Token);

			Demo.Instance.AddEnemy(this);
		}

		protected void Fire()
		{
			Pooler.GetGroup("EnemyProjectiles").Pools.GetRandom().Instantiate(transform.position);
		}

		public override void Die()
		{
			base.Die();
			Demo.Instance.RemoveEnemy(this);
		}

		private void OnDestroy()
		{
			cancelSource.Cancel();
			cancelSource.Dispose();
		}
	}
}