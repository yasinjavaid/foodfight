/*
using DG.Tweening;
using Kit;
using Sirenix.OdinInspector;
using UnityEngine;
*/

namespace Game.Items.Weapons
{
	public class KernelPopper: Weapon
	{
		/*
				[FoldoutGroup("Corn")]
				public Transform Burner;

				[FoldoutGroup("Corn")]
				public Transform Corn;

				[FoldoutGroup("Corn")]
				public float CornSpawnTimeMin = 0.1f;

				[FoldoutGroup("Corn")]
				public float CornSpawnTimeMax = 0.25f;

				[FoldoutGroup("Corn")]
				public float CornMoveXMin = -0.3f;

				[FoldoutGroup("Corn")]
				public float CornMoveXMax = 0.3f;

				[FoldoutGroup("Corn")]
				public float CornMoveYMin = 0.1f;

				[FoldoutGroup("Corn")]
				public float CornMoveYMax = 0.2f;

				[FoldoutGroup("Corn")]
				public float CornMoveYBottom = 0.5f;

				[FoldoutGroup("Corn")]
				public float CornDurationMin = 0.25f;

				[FoldoutGroup("Corn")]
				public float CornDurationMax = 1.0f;

				protected override void Start()
				{
					base.Start();
					QueueCorn();
				}

				private void QueueCorn()
				{
					float spawnDelay = Random.Range(CornSpawnTimeMin, CornSpawnTimeMax);
					ControlHelper.Delay(spawnDelay, SpawnCorn);
				}

				private void SpawnCorn()
				{
					GameObject cornObject = Instantiate(Corn.gameObject, Burner.position, Burner.rotation);
					Transform cornTransform = cornObject.transform;
					cornTransform.parent = Burner;

					float moveX = Random.Range(CornMoveXMin,       CornMoveXMax);
					float moveY = Random.Range(CornMoveYMin,       CornMoveYMax);
					float duration = Random.Range(CornDurationMin, CornDurationMax);

					Vector3 top = new Vector3();
					top.x += moveX / 2.0f;
					top.z += moveX / 2.0f;
					top.y += moveY;

					Vector3 bottom = new Vector3();
					bottom.x += moveX;
					bottom.z += moveX;
					bottom.y -= CornMoveYBottom;

					cornTransform.DOLocalPath(new [] { top, bottom }, duration, PathType.CatmullRom)
								 .OnComplete(() => Destroy(cornObject));
					QueueCorn();
				}
		*/
	}
}