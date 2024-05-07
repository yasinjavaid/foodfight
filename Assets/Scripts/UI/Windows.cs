using UnityEngine;

namespace Game.UI
{
	public static class Windows
	{
		public const string MessageWindow = "Windows/MessageWindow";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		public static void Initialize()
		{
			Kit.UI.Message.MessageWindow.DefaultWindow = MessageWindow;
		}
	}
}