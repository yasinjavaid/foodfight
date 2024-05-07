using UniTaskPubSub.AsyncEnumerable;
using UnityEngine;

namespace Game
{
	public static class MessageBroker
	{
		public static AsyncEnumerableMessageBus Instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			Instance = new AsyncEnumerableMessageBus();
		}
	}
}