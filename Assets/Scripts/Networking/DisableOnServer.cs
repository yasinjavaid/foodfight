using UnityEngine;

namespace Game.Networking
{
	public class DisableOnServer: MonoBehaviour
	{
		protected void Awake()
		{
#if CLIENT_BUILD
			gameObject.SetActive(true);
#else
			gameObject.SetActive(false);
#endif
		}
	}
}