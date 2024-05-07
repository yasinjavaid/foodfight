using System;
using Cysharp.Threading.Tasks.Linq;
using Game.Items;

namespace Game
{
	public struct AchievementTriggered
	{
		public Achievement Achievement;
	}

	public struct AchievementAchieved
	{
		public Achievement Achievement;
	}

	public abstract class Achievement
	{
		public int Times;
		public IDisposable Observer { get; private set; }

		public abstract IDisposable GetObserver();

		public virtual void Bind()
		{
			Observer = GetObserver();
		}

		public virtual void Unbind()
		{
			Observer.Dispose();
		}

		public virtual void Trigger()
		{
			if (Times > 0)
			{
				Times--;
				MessageBroker.Instance.Publish(new AchievementTriggered { Achievement = this });
			}
			else
				Achieve();
		}

		public virtual void Achieve()
		{
			MessageBroker.Instance.Publish(new AchievementAchieved { Achievement = this });
		}
	}

	public class ConsumeableAchievement: Achievement
	{
		public string Consumeable;

		public override IDisposable GetObserver()
		{
			return MessageBroker.Instance
							  .Receive<ItemUsed>()
							  .Where(msg => msg.Item.ID == Consumeable)
							  .Subscribe(_ => Trigger());
		}
	}

	public class WeaponAchievement: Achievement
	{
		public string Weapon;

		public override IDisposable GetObserver()
		{
			return MessageBroker.Instance
							  .Receive<PlayerKilled>()
							  .Where(msg => msg.With.ID == Weapon)
							  .Subscribe(_ => Trigger());
		}
	}
}