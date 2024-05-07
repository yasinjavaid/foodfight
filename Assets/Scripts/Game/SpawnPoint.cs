using UnityEngine;

namespace Game
{
	public class SpawnPoint: MonoBehaviour
	{
		public void Awake()
		{
#if SERVER_BUILD
			GameManager.Instance.RegisterSpawnPoint(this);
#endif
		}

		public Transform Transform => transform;
		public Vector3 Position => Transform.position;
		public Quaternion Rotation => Transform.rotation;
	}
}