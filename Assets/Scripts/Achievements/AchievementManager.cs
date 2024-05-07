using System;
using Cysharp.Threading.Tasks.Linq;
using Kit;

namespace Game
{
	public static class AchievementManager
	{
		private static CompositeDisposable disposables = new CompositeDisposable();

		public static void Bind()
		{
			foreach ((_, Achievement achievement) in DataManager.GameState.Achievements)
				achievement.Bind();

			disposables.Add(MessageBroker.Instance
								 .Receive<AchievementAchieved>()
								 .Subscribe(msg => OnAchievementAchieved(msg.Achievement)));

			disposables.Add(MessageBroker.Instance
									   .Receive<AchievementTriggered>()
									   .Subscribe(msg => OnAchievementTriggered(msg.Achievement)));
		}

		public static void Unbind()
		{
			foreach ((_, Achievement achievement) in DataManager.GameState.Achievements)
				achievement.Unbind();
			disposables.Dispose();
		}

		private static void OnAchievementTriggered(Achievement achievement)
		{
			throw new NotImplementedException();
		}

		private static void OnAchievementAchieved(Achievement achievement)
		{
			throw new NotImplementedException();
		}
	}
}