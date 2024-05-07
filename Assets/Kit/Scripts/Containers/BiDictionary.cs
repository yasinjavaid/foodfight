using System;
using System.Collections.Generic;

namespace Kit.Containers
{
	/// <summary>A 1-to-1 dictionary that can be used to lookup with either item as key.</summary>
	/// <typeparam name="TFirst">First type.</typeparam>
	/// <typeparam name="TSecond">Second type.</typeparam>
	public class BiDictionary<TFirst, TSecond>
	{
		private IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
		private IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

		#region Common

		/// <summary>Gets or sets the value of second item.</summary>
		public TSecond this[TFirst first]
		{
			get => Get(first);
			set => Add(first, value);
		}

		/// <summary>Gets or sets the value of first item.</summary>
		public TFirst this[TSecond second]
		{
			get => Get(second);
			set => Add(value, second);
		}

		/// <summary>The number of pairs stored in the dictionary.</summary>
		public int Count => firstToSecond.Count;

		/// <summary>Removes all items from the dictionary.</summary>
		public void Clear()
		{
			firstToSecond.Clear();
			secondToFirst.Clear();
		}

		#endregion

		#region Exception-throwing methods

		/// <summary>Tries to add a pair to the dictionary.</summary>
		/// <exception cref="ArgumentException">Throws an exception if either element is already in the dictionary.</exception>
		public void Add(TFirst first, TSecond second)
		{
			if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
				throw new ArgumentException($"Duplicate {nameof(first)} or {nameof(second)}");

			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
		}

		/// <summary>Find the <typeparamref name="TSecond" /> corresponding to a <typeparamref name="TFirst" />.</summary>
		/// <exception cref="ArgumentException">Throws an exception if the <typeparamref name="TFirst" /> is not in the dictionary.</exception>
		public TSecond Get(TFirst first)
		{
			if (!firstToSecond.TryGetValue(first, out TSecond second))
				throw new ArgumentException(nameof(first));

			return second;
		}

		/// <summary>Find the <typeparamref name="TFirst" /> corresponding to a <typeparamref name="TSecond" />.</summary>
		/// <exception cref="ArgumentException">Throws an exception if the <typeparamref name="TSecond" /> is not in the dictionary.</exception>
		public TFirst Get(TSecond second)
		{
			if (!secondToFirst.TryGetValue(second, out TFirst first))
				throw new ArgumentException(nameof(second));

			return first;
		}

		/// <summary>Remove the pair corresponding to a <typeparamref name="TFirst" />.</summary>
		/// <exception cref="ArgumentException">Throws an exception if the <typeparamref name="TFirst" /> is not in the dictionary.</exception>
		public void Remove(TFirst first)
		{
			if (!firstToSecond.TryGetValue(first, out TSecond second))
				throw new ArgumentException(nameof(first));

			firstToSecond.Remove(first);
			secondToFirst.Remove(second);
		}

		/// <summary>Remove the pair corresponding to a <typeparamref name="TSecond" />.</summary>
		/// <exception cref="ArgumentException">Throws an exception if the <typeparamref name="TSecond" /> is not in the dictionary.</exception>
		public void Remove(TSecond second)
		{
			if (!secondToFirst.TryGetValue(second, out TFirst first))
				throw new ArgumentException(nameof(second));

			secondToFirst.Remove(second);
			firstToSecond.Remove(first);
		}

		#endregion

		#region Try methods

		/// <summary>Tries to add the pair to the dictionary.</summary>
		/// <returns><see langword="true" /> if successfully added, <see langword="false" /> if either element is already in the dictionary.</returns>
		public bool TryAdd(TFirst first, TSecond second)
		{
			if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
				return false;

			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
			return true;
		}

		/// <summary>Find the <typeparamref name="TSecond" /> corresponding to the <typeparamref name="TFirst" />.</summary>
		/// <returns><see langword="true" /> if <paramref name="first" /> is in the dictionary, <see langword="false" /> otherwise.</returns>
		public bool TryGet(TFirst first, out TSecond second)
		{
			return firstToSecond.TryGetValue(first, out second);
		}

		/// <summary>Find the <typeparamref name="TFirst" /> corresponding to the <typeparamref name="TSecond" />.</summary>
		/// <returns><see langword="true" /> if <paramref name="second" /> is in the dictionary, <see langword="false" /> otherwise .</returns>
		public bool TryGet(TSecond second, out TFirst first)
		{
			return secondToFirst.TryGetValue(second, out first);
		}

		/// <summary>Remove the pair containing a <typeparamref name="TFirst" />, if there is one.</summary>
		/// <returns>If <paramref name="first" /> is not in the dictionary, returns <see langword="false" />, otherwise <see langword="true" />.</returns>
		public bool TryRemove(TFirst first)
		{
			if (!firstToSecond.TryGetValue(first, out TSecond second))
				return false;

			firstToSecond.Remove(first);
			secondToFirst.Remove(second);
			return true;
		}

		/// <summary>Remove the pair containing a <typeparamref name="TSecond" />, if there is one.</summary>
		/// <returns>If <paramref name="second" /> is not in the dictionary, returns <see langword="false" />, otherwise <see langword="true" />.</returns>
		public bool TryRemove(TSecond second)
		{
			if (!secondToFirst.TryGetValue(second, out TFirst first))
				return false;

			secondToFirst.Remove(second);
			firstToSecond.Remove(first);
			return true;
		}

		#endregion
	}
}