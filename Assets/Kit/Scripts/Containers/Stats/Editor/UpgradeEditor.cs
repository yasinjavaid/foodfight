#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Kit.Containers.Editor
{
	public class UpgradeDrawer<T>: OdinValueDrawer<T> where T: IUpgrade
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			SirenixEditorGUI.BeginToolbarBox(label);
			CallNextDrawer(null);
			SirenixEditorGUI.EndToolbarBox();
		}
	}
}
#endif