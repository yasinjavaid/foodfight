using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Kit.Behaviours
{
	/// <summary>Fades the screen in on Awake.</summary>
	/// <remarks>Drop it on a <see cref="GameObject" /> in a scene to fade it in whenever it loads.</remarks>
	public class FadeIn: MonoBehaviour
	{
		/// <summary>The fading color to use.</summary>
		[Tooltip("The fading color to use.")]
		public Color FadeColor = SceneDirector.DefaultFadeColor;

		/// <summary>The fade duration.</summary>
		[Tooltip("The fade duration.")]
		[SuffixLabel("seconds", true)]
		public float FadeTime = SceneDirector.DefaultFadeTime;

		/// <summary>Stuff to do when the fading is done.</summary>
		[Tooltip("Stuff to do when the fading is done.")]
		public UnityEvent Completed;

		private void Awake()
		{
			SceneDirector.FadeIn(FadeColor, FadeTime, Completed.Invoke);
		}
	}
}