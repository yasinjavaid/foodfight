using System;
using Kit.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace Kit
{
	/// <summary>UI hookup for the <see cref="Console" />.</summary>
	public class ConsoleUI: MonoBehaviour
	{
		/// <summary><see cref="UnityEngine.Animator" /> to use for show/hide animations.</summary>
		public Animator Animator;

		/// <summary><see cref="ScrollRect" /> for the console log.</summary>
		public ScrollRect LogScroll;

		/// <summary>The text-box for the console log.</summary>
		public Text LogText;

		/// <summary>The command input-field.</summary>
		public InputFieldEx CommandInput;

		#region Version Information
		public enum VersionMode
		{
			None,
			Build,
			Revision,
			MajorMinor,
			Full
		}

		public VersionMode DisplayVersion = VersionMode.Full;

#if DEVELOPMENT_BUILD
		private GUIStyle versionStyle;
		private GUIStyle shadowStyle;
		private string versionText;

		private void Awake()
		{
			if (DisplayVersion == VersionMode.None)
				return;

			Version version = BuildManager.Version;

			switch (DisplayVersion)
			{
				case VersionMode.Build:
					versionText = version.Build > 0 ? version.Build.ToString() : version.ToString();
					break;

				case VersionMode.Revision:
					versionText = version.Build > 0 ? version.Revision.ToString() : version.ToString();
					break;

				case VersionMode.MajorMinor:
					versionText = version.Minor > 0 ? $"{version.Major}.{version.Minor}" : version.Major.ToString();
					break;

				case VersionMode.Full:
					versionText = version.ToString();
					break;
			}

			versionText = $"Build: {versionText} ";

		}

		private void OnGUI()
		{
			if (DisplayVersion == VersionMode.None)
				return;

			Rect versionRect = new Rect(Screen.width - 100, Screen.height - 40, 100, 20);
			Rect shadowRect = versionRect;
			shadowRect.center += new Vector2(2, 2);

			if (versionStyle == null)
			{
				versionStyle = new GUIStyle(GUI.skin.label)
							   {
								   fontSize = 16,
								   fontStyle = FontStyle.Bold,
								   alignment = TextAnchor.MiddleRight,
								   normal = { textColor = new Color(1, 1, 1, 1.0f) }
							   };

				shadowStyle = new GUIStyle(versionStyle) { normal = { textColor = new Color(0, 0, 0, 0.5f) } };
			}

			GUI.Label(shadowRect,  versionText, shadowStyle);
			GUI.Label(versionRect, versionText, versionStyle);
		}
#endif
		#endregion

#if CONSOLE && (UNITY_EDITOR || DEVELOPMENT_BUILD)
		private void OnDestroy()
		{
			Console.Destroy();
		}
#endif
	}
}