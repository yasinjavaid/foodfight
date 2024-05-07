using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kit.Containers
{
	/// <summary> A <see cref="List{T}" /> that fires events when items are added or removed.</summary>
	public class AsyncReactiveList<T>: List<T>
	{
		private TriggerEvent<int> countChangedEvent = default;

		/// <inheritdoc cref="List{T}.Add(T)" />
		public new void Add(T item)
		{
			base.Add(item);
			countChangedEvent.SetResult(Count);
		}

		/// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})" />
		public new void AddRange(IEnumerable<T> items)
		{
			base.AddRange(items);
			countChangedEvent.SetResult(Count);
		}

		/// <inheritdoc cref="List{T}.Insert(int, T)" />
		public new void Insert(int index, T item)
		{
			base.Insert(index, item);
			countChangedEvent.SetResult(Count);
		}

		/// <inheritdoc cref="List{T}.InsertRange(int, IEnumerable{T})" />
		public new void InsertRange(int index, IEnumerable<T> items)
		{
			base.InsertRange(index, items);
			countChangedEvent.SetResult(Count);
		}

		/// <inheritdoc cref="List{T}.Remove(T)" />
		public new bool Remove(T item)
		{
			bool result = base.Remove(item);
			if (result)
				countChangedEvent.SetResult(Count);
			return result;
		}

		/// <inheritdoc cref="List{T}.RemoveAt(int)" />
		public new void RemoveAt(int index)
		{
			base.RemoveAt(index);
			countChangedEvent.SetResult(Count);
		}

		/// <inheritdoc cref="List{T}.RemoveRange(int, int)" />
		public new void RemoveRange(int index, int count)
		{
			base.RemoveRange(index, count);
			if (count > 0)
				countChangedEvent.SetResult(Count);
		}

		/// <inheritdoc cref="List{T}.RemoveAll" />
		public new int RemoveAll(Predicate<T> match)
		{
			int previousCount = Count;
			int result = base.RemoveAll(match);
			int newCount = Count;
			if (previousCount != newCount)
				countChangedEvent.SetResult(newCount);
			return result;
		}

		/// <inheritdoc cref="List{T}.Clear" />
		public new void Clear()
		{
			if (Count <= 0)
				return;

			base.Clear();
			countChangedEvent.SetResult(0);
		}

		/// <summary>Returns a <see cref="IUniTaskAsyncEnumerable{T}" /> that triggers when the number of items in the list change.</summary>
		public IUniTaskAsyncEnumerable<int> EveryCountChanged()
		{
			return new CountChangedEnumerable(this);
		}

		private class CountChangedEnumerable: IUniTaskAsyncEnumerable<int>
		{
			private AsyncReactiveList<T> parent;

			public CountChangedEnumerable(AsyncReactiveList<T> parent)
			{
				this.parent = parent;
			}

			public IUniTaskAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken token = new CancellationToken())
			{
				return new CountChangedHandler(parent, token);
			}
		}

		private class CountChangedHandler: MoveNextSource, IUniTaskAsyncEnumerator<int>, ITriggerHandler<int>
		{
			public int Current { get; private set; }

			private AsyncReactiveList<T> parent;
			private CancellationToken cancellationToken;
			private CancellationTokenRegistration cancellationTokenRegistration;
			private bool isDisposed;

			public ITriggerHandler<int> Prev { get; set; }
			public ITriggerHandler<int> Next { get; set; }

			public CountChangedHandler(AsyncReactiveList<T> parent, CancellationToken cancellationToken)
			{
				this.parent = parent;
				this.cancellationToken = cancellationToken;
				parent.countChangedEvent.Add(this);
				TaskTracker.TrackActiveTask(this, 3);

				if (cancellationToken.CanBeCanceled)
					cancellationTokenRegistration = cancellationToken.RegisterWithoutCaptureExecutionContext(CancellationCallback, this);
			}

			public UniTask<bool> MoveNextAsync()
			{
				completionSource.Reset();
				return new UniTask<bool>(this, completionSource.Version);
			}

			public UniTask DisposeAsync()
			{
				if (!isDisposed)
				{
					isDisposed = true;
					TaskTracker.RemoveTracking(this);
					cancellationTokenRegistration.Dispose();
					completionSource.TrySetCanceled(cancellationToken);
					parent.countChangedEvent.Remove(this);
				}

				return default;
			}

			public void OnNext(int newValue)
			{
				Current = newValue;
				completionSource.TrySetResult(true);
			}

			public void OnCompleted()
			{
				completionSource.TrySetResult(false);
			}

			public void OnError(Exception ex)
			{
				completionSource.TrySetException(ex);
			}

			public void OnCanceled(CancellationToken token)
			{
				DisposeAsync().Forget();
			}

			private static void CancellationCallback(object state)
			{
				CountChangedHandler self = (CountChangedHandler) state;
				self.DisposeAsync().Forget();
			}
		}
	}
}