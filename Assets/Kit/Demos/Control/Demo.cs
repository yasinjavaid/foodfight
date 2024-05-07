using Kit;
using Kit.UI.Message;
using UniTaskPubSub;
using UnityEngine;

namespace Demos.Control
{
	public class Demo: MonoBehaviour
	{
		public class PlayerMoved
		{
			public int X;
			public int Y;
		}

		private void Awake()
		{
			AsyncMessageBus.Default.Subscribe<PlayerMoved>(playerMoved => MessageWindow.Show("Demo", $"Player moved to {playerMoved.X}, {playerMoved.Y}."));
		}

		public static void DelayTime()
		{
			ControlHelper.Delay(1.0f, () => MessageWindow.Show("Demo", "After a second."));
		}

		public static void DelayFrames()
		{
			ControlHelper.Delay(500, () => MessageWindow.Show("Demo", "After a thousand frames."));
		}

		public static void MessageBroker()
		{
			AsyncMessageBus.Default.Publish(new PlayerMoved() { X = 3, Y = 3 });
		}
	}
}