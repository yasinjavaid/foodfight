using UnityEngine;
using UnityEngine.InputSystem;

namespace Kit.Behaviours
{
	/// <summary>Quits the application when the Back button is pressed on Android.</summary>
	public class QuitOnBack: MonoBehaviour
	{
#if UNITY_ANDROID
	private void Update()
	{
		if (Keyboard.current.escapeKey.wasPressedThisFrame)
			Application.Quit();
	}
#endif
	}
}