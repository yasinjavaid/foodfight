﻿using Kit;
using Kit.UI;
using Kit.UI.Message;
using UnityEngine;
using UnityEngine.UI;

namespace Demos.UI
{
	public class Demo : MonoBehaviour
	{
		public Image BGImage;
		public MyWindow PrefabWindow;
		public MyWindow SceneWindow;
		public string PrefabPath;
		public WindowReference SoftReference;

		public void ShowWindowWithPrefab()
		{
			UIManager.Show(PrefabWindow);
		}

		public void ShowWindowFromScene()
		{
			SceneWindow.Show();
		}

		public void ShowWindowWithPrefabPath()
		{
			UIManager.Show(PrefabPath);
		}

		public void ShowWindowWithSoftReference()
		{
			UIManager.Show(SoftReference);
		}

		public void PassDataToWindow()
		{
			UIManager.Show(PrefabWindow, "Data passed to the window.");
		}

		public void ConflictWindow()
		{
			UIManager.Show(PrefabWindow, conflictMode: WindowConflictMode.HidePrevious);
		}

		public void ShowMessage()
		{
			MessageWindow.Show("Clear background",
							   "Do you want to clear the background?",
							   MessageType.Question, MessageButtons.YesNo,
							   yesAction: () => BGImage.sprite = null);
		}
	}
}
