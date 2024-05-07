using System;
using Cysharp.Threading.Tasks;

namespace Game.UI.Splash
{
	public struct SplashTask
	{
		public string Name;
		public Func<UniTask> Task;
		public float Weight;

		public SplashTask(string name, Func<UniTask> task, int weight)
		{
			Name = name;
			Task = task;
			Weight = weight;
		}
	}
}