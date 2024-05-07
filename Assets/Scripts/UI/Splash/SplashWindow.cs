using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kit;
using Kit.UI;
using UnityEngine;
using UnityEngine.UI;
#if MODDING
using Kit.Modding;

#endif

namespace Game.UI.Splash
{
	public class SplashWindow: MonoBehaviour
	{
		public Text MessageText;
		public Image ProgressImage;
		public SceneReference NextScene;

		private readonly Queue<SplashTask> tasks = new Queue<SplashTask>();

		#region Initalization

		private void Awake()
		{
			SceneDirector.FadeIn();
		}

		#endregion

		#region Task execution

		private void Start()
		{
			QueueTasks();
			RunTasks().Forget();
		}

		public void QueueTask(string taskName, Func<UniTask> task, int taskWeight)
		{
			QueueTask(new SplashTask(taskName, task, taskWeight));
		}

		public void QueueTask(SplashTask task)
		{
			tasks.Enqueue(task);
		}

		private async UniTask RunTasks()
		{
			float totalWeight = 0;
			if (ProgressImage != null)
			{
				totalWeight = tasks.Sum(t => t.Weight);
				ProgressImage.fillAmount = 0;
			}

			while (tasks.Count > 0)
			{
				SplashTask task = tasks.Dequeue();
				if (MessageText != null)
					MessageText.text = task.Name;

				await task.Task();

				if (ProgressImage != null)
					ProgressImage.fillAmount += task.Weight / totalWeight;
			}

			await LoadNextScene();
		}

		private UniTask LoadNextScene()
		{
			return SceneDirector.LoadScene(NextScene);
		}

		#endregion

		#region Tasks

		private void QueueTasks()
		{
			QueueDataTasks();
#if MODDING
			QueueModTasks();
#endif
		}

		private void QueueDataTasks()
		{
			QueueTask("Loading data...", DataManager.LoadDataAsync, 10);
		}

#if MODDING
		private void QueueModTasks()
		{
			var modPaths = ModManager.GetModPathsByGroup();
			int totalMods = modPaths.Sum(kvp => kvp.Value.Length);
			if (totalMods <= 0)
				return;

			QueueTask("Loading mods...",   () => ModManager.LoadModsAsync(modPaths), totalMods);
			QueueTask("Executing mods...", ModManager.ExecuteScriptsAsync,           totalMods);
		}
#endif

		#endregion
	}
}