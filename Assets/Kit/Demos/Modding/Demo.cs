using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kit;
using Kit.UI.Message;
using UnityEngine;
using UnityEngine.UI;

#if MODDING
using Kit.Modding;
#endif

namespace Demos.Modding
{
#if MODDING
#if HOTFIX_ENABLE
    [Hotfix]
#endif
#endif
    public class Demo: MonoBehaviour
	{
		public RawImage DisplayImage;
		public List<Button> DisabledButtons;

#if MODDING
		public string Title { get; protected set; } = "Demo";

		private void Awake()
		{
			DisabledButtons.ForEach(b => b.interactable = false);
		}

		public void LoadMods()
		{
			ModManager.LoadModsAsync(true).Forget();
			DisabledButtons.ForEach(b => b.interactable = true);
		}

		public void LoadResource()
		{
			Texture texture = ResourceManager.Load<Texture>(ResourceFolder.Resources, "Test.jpg");
			if (texture != null)
			{
				DisplayImage.texture = texture;
				DisplayImage.color = Color.white;
			}
		}

		public void ClickMe()
        {
            MessageWindow.Show(Title, "Dang it!\nSee the lua scripts for how this is being done.");
        }

        public void InjectedReplace()
		{
			string message = @"Steps to follow before testing injection:
1. Add HOTFIX_ENABLE in Scripting Define Symbols.
2. Move /Plugins/XLua/ to the base folder and delete all asmdefs from it.
3. Press XLua -> Generate Code.
4. Press XLua -> Hotfix Inject in Editor.";
			Debugger.Log(message);
		}

		public void InjectedExtend()
		{
			Debugger.Log("Game code");
		}
#endif
	}
}