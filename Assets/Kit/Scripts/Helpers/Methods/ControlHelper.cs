using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace Kit
{
	/// <summary>Helper functions for events and control flow.</summary>
	public class ControlHelper: MonoBehaviour
	{
		/// <summary>Event fired when the app loses/gains focus.</summary>
		public static event Action<bool> ApplicationFocus;

		/// <summary>Event fired when the app pauses/unpauses.</summary>
		public static event Action<bool> ApplicationPause;

		/// <summary>Event fired when the app quits.</summary>
		public static event Action ApplicationQuit;

		static ControlHelper()
		{
			GameObject go = new GameObject(nameof(ControlHelper));
			go.AddComponent<ControlHelper>();
			DontDestroyOnLoad(go);
		}

		/// <summary>Perform an action next frame.</summary>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		public static void Delay(Action action, CancellationToken cancelToken = default)
		{
			Delay(1, action, cancelToken);
		}

		/// <summary>Call a method after specified number of frames.</summary>
		/// <param name="frames">Number of frames to hold out for.</param>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		public static void Delay(int frames, Action action, CancellationToken cancelToken = default)
		{
			UniTaskAsyncEnumerable.TimerFrame(frames).ForEachAsync(_ => action(), cancelToken);
		}

		/// <summary>Keep calling a method after a specified number of frames.</summary>
		/// <param name="delayFrames">Number of frames to hold out for before calling for the first time.</param>
		/// <param name="intervalFrames">Number of frames to hold out for before calling subsequent times.</param>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		public static void Delay(int delayFrames, int intervalFrames, Action action, CancellationToken cancelToken = default)
		{
			UniTaskAsyncEnumerable.TimerFrame(delayFrames, intervalFrames).ForEachAsync(_ => action(), cancelToken);
		}

		/// <summary>Call a method after specified number of seconds.</summary>
		/// <param name="seconds">Number of seconds to hold out for.</param>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		public static void Delay(float seconds, Action action, CancellationToken cancelToken = default)
		{
			UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(seconds)).ForEachAsync(_ => action(), cancelToken);
		}

		/// <summary>Keep calling a method after a specified number of seconds.</summary>
		/// <param name="delaySeconds">Number of seconds to hold out for before calling for the first time.</param>
		/// <param name="intervalSeconds">Number of seconds to hold out for before calling subsequent times.</param>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		public static void Delay(float delaySeconds, float intervalSeconds, Action action, CancellationToken cancelToken = default)
		{
			UniTaskAsyncEnumerable.Timer(TimeSpan.FromSeconds(delaySeconds), TimeSpan.FromSeconds(intervalSeconds))
								  .ForEachAsync(_ => action(), cancelToken);
		}

		/// <summary>Call a method after a specified condition is satisfied.</summary>
		/// <param name="until">Function</param>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		public static void Delay(Func<bool> until, Action action, CancellationToken cancelToken = default)
		{
			DelayUntil(until, action, cancelToken).Forget();
		}

		/// <summary>Keep calling a method with an interval.</summary>
		/// <param name="seconds">Number of seconds to hold out for between calls.</param>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		public static void Interval(float seconds, Action action, CancellationToken cancelToken = default)
		{
			UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(seconds)).ForEachAsync(_ => action(), cancelToken);
		}

		private static async UniTaskVoid DelayUntil(Func<bool> predicate, Action action, CancellationToken cancelToken)
		{
			try
			{
				await UniTask.WaitUntil(predicate, PlayerLoopTiming.Update, cancelToken);
				action();
			}
			catch (OperationCanceledException)
			{
				// Do nothing if cancelled
			}
		}

		/// <summary>Execute a method every frame.</summary>
		/// <param name="action">Action to perform.</param>
		/// <param name="cancelToken">Handle for cancellation.</param>
		/// <param name="timing">The Update stage at which to execute the function.</param>
		public static void EachFrame(Action action,
									 CancellationToken cancelToken = default,
									 PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			UniTaskAsyncEnumerable.EveryUpdate(timing).ForEachAsync(_ => action(), cancelToken);
		}

		/// <inheritdoc cref="EachFrame(System.Action,System.Threading.CancellationToken,Cysharp.Threading.Tasks.PlayerLoopTiming)" />
		/// <param name="filter">Function to use as filter.</param>
		public static void EachFrame(Func<bool> filter,
									 Action action,
									 CancellationToken cancelToken = default,
									 PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			UniTaskAsyncEnumerable.EveryUpdate(timing).Where(_ => filter()).ForEachAsync(_ => action(), cancelToken);
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			ApplicationFocus?.Invoke(hasFocus);
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			ApplicationPause?.Invoke(pauseStatus);
		}

		private void OnApplicationQuit()
		{
			ApplicationQuit?.Invoke();
		}
	}
}