using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Containers
{
	/// <summary>A generic class that holds how much of a particular item you carry. Can be used to create things like Inventories or Wallets.</summary>
	/// <typeparam name="T">Base type of items.</typeparam>
	/// <example>
	///     <code>
	/// Bag&lt;Currency&gt; wallet = new Bag&lt;Currency&gt;();
	/// Bunch&lt;Currency&gt; cost = new Bunch&lt;Currency&gt;(Currency.Diamonds, 10);
	/// wallet[Currency.Diamonds] = 50;
	/// wallet[Currency.Coins] = 25;
	/// wallet -= cost;
	/// </code>
	/// </example>
	/// <remarks>
	///     To create nested inventories, create a <c>Bag&lt;Bag&lt;T&gt;&gt;</c>. To create inventories that can have nested and non-nested
	///     items, create a <c>Bag&lt;object&gt;</c>.
	/// </remarks>
	[DictionaryDrawerSettings(KeyLabel = "Item", ValueLabel = "Amount")]
	public class Bag<T>: Dictionary<T, int>
	{
		/// <summary>Event called whenever a new item is added.</summary>
		public event Action<T, int> Added;

		/// <summary>Event called whenever an item's amount is changed.</summary>
		public event Action<T, int> Changed;

		/// <summary>Event called whenever an item is removed.</summary>
		public event Action<T, int> Removed;

		/// <summary>
		///     A function that should return the maximum amount of items of a particular type can be there. The amounts will be capped to this
		///     function's values if specified. Ignored if <see langword="null" />.
		/// </summary>
		public Func<T, int> Max;

		/// <summary>Returns the current amount of an item.</summary>
		public new int this[T item]
		{
			get => TryGetValue(item, out int value) ? value : 0;
			set
			{
				int clamped = Max != null ? Mathf.Min(value, Max(item)) : value;
				if (clamped > 0)
				{
					bool contained = ContainsKey(item);
					base[item] = clamped;
					if (contained)
						Changed?.Invoke(item, clamped);
					else
						Added?.Invoke(item, clamped);
				}
				else
				{
					if (base.Remove(item))
						Removed?.Invoke(item, 0);
				}
			}
		}

		/// <summary>Adds the amount of items.</summary>
		public new void Add(T item, int amount = 1)
		{
			this[item] += amount;
		}

		/// <summary>Adds the amount of items.</summary>
		public void Add(KeyValuePair<T, int> kvp)
		{
			Add(kvp.Key, kvp.Value);
		}

		/// <summary>Adds the amount of items.</summary>
		public void Add(Bunch<T> bunch)
		{
			Add(bunch.Item, bunch.Amount);
		}

		/// <summary>Adds the amount of items from another dictionary.</summary>
		public void Add(IDictionary<T, int> bag)
		{
			foreach (var field in bag)
				Add(field);
		}

		/// <summary>Removes the amount of items.</summary>
		public bool Remove(T item, int amount = 1)
		{
			if (!Contains(item, amount))
				return false;

			this[item] -= amount;

			return true;
		}

		/// <summary>Removes the amount of items.</summary>
		public bool Remove(KeyValuePair<T, int> kvp)
		{
			return Remove(kvp.Key, kvp.Value);
		}

		/// <summary>Removes the amount of items.</summary>
		public bool Remove(Bunch<T> bunch)
		{
			return Remove(bunch.Item, bunch.Amount);
		}

		/// <summary>Removes the amount of items from another dictionary.</summary>
		public bool Remove(IDictionary<T, int> bag)
		{
			if (!Contains(bag))
				return false;

			foreach ((T key, int value) in bag)
				this[key] -= value;

			return true;
		}

		/// <summary>Sets the amount of specified items to 0.</summary>
		public bool RemoveAll(T item)
		{
			if (!ContainsKey(item))
				return false;

			this[item] = 0;

			return true;
		}

		/// <summary>Sets the amount of specified items to 0.</summary>
		public bool RemoveAll(IEnumerable<T> items)
		{
			bool success = true;
			foreach (T item in items)
				if (ContainsKey(item))
					this[item] = 0;
				else
					success = false;
			return success;
		}


		/// <summary>Returns whether it contains the specified amount of an item.</summary>
		public bool Contains(T item, int amount)
		{
			return this[item] >= amount;
		}

		/// <summary>Returns whether it contains the specified amount of an item.</summary>
		public bool Contains(Bunch<T> bunch)
		{
			return Contains(bunch.Item, bunch.Amount);
		}

		/// <summary>Returns whether it contains the specified amount of an item.</summary>
		public bool Contains(KeyValuePair<T, int> kvp)
		{
			return Contains(kvp.Key, kvp.Value);
		}

		/// <summary>Returns whether it contains the specified amounts of items.</summary>
		public bool Contains(IDictionary<T, int> bag)
		{
			return bag.All(Contains);
		}

		/// <summary>Add an item to the bag.</summary>
		public static Bag<T> operator +(Bag<T> bag, T item)
		{
			bag.Add(item);
			return bag;
		}

		/// <summary>Add items to the bag.</summary>
		public static Bag<T> operator +(Bag<T> bagTo, IDictionary<T, int> bagFrom)
		{
			bagTo.Add(bagFrom);
			return bagTo;
		}

		/// <summary>Add an item to the bag.</summary>
		public static Bag<T> operator +(Bag<T> bag, Bunch<T> bunch)
		{
			bag.Add(bunch);
			return bag;
		}

		/// <summary>Add an item to the bag.</summary>
		public static Bag<T> operator +(Bag<T> bag, KeyValuePair<T, int> kvp)
		{
			bag.Add(kvp);
			return bag;
		}

		/// <summary>Remove an item from the bag.</summary>
		public static Bag<T> operator -(Bag<T> bag, T item)
		{
			bag.Remove(item);
			return bag;
		}

		/// <summary>Remove items from the bag.</summary>
		public static Bag<T> operator -(Bag<T> bagTo, IDictionary<T, int> bagFrom)
		{
			bagTo.Remove(bagFrom);
			return bagTo;
		}

		/// <summary>Remove an item from the bag.</summary>
		public static Bag<T> operator -(Bag<T> bag, Bunch<T> bunch)
		{
			bag.Remove(bunch);
			return bag;
		}

		/// <summary>Remove an item from the bag.</summary>
		public static Bag<T> operator -(Bag<T> bag, KeyValuePair<T, int> kvp)
		{
			bag.Remove(kvp);
			return bag;
		}

		/// <summary>Convert the dictionary to a enumerable of Bunches.</summary>
		public IEnumerable<Bunch<T>> AsBunches()
		{
			return this.Select(kvp => new Bunch<T>(kvp));
		}

		/// <summary>Convert the dictionary to a List of Bunches.</summary>
		public List<Bunch<T>> ToBunches()
		{
			return AsBunches().ToList();
		}
	}

	/// <summary>
	///     <see cref="Bunch{T}" /> is just <c>KeyValuePair&lt;<typeparamref name="T" />, int&gt;</c> with operators for use with
	///     <see cref="Bag{T}" />s (would've just derived from <c>KeyValuePair</c> but you can't inherit structs).
	/// </summary>
	/// <example>
	///     <code>
	/// Bunch&lt;Currency&gt; base = new Bunch&lt;Currency&gt;(Currency.Diamonds, 10);
	/// Bunch&lt;Currency&gt; bonus = base * 4;
	/// </code>
	/// </example>
	[Serializable]
	public struct Bunch<T>
	{
		/// <summary>The item in question.</summary>
		public T Item;

		/// <summary>The number of items.</summary>
		public int Amount;

		/// <summary>Create a <see cref="Bunch{T}" /> from a <see cref="KeyValuePair{T,TValue}" />.</summary>
		public Bunch(KeyValuePair<T, int> pair): this(pair.Key, pair.Value)
		{
		}

		/// <summary>Create a bunch from another one.</summary>
		public Bunch(Bunch<T> other): this(other.Item, other.Amount)
		{
		}

		/// <summary>Create a bunch with the specified item and amount.</summary>
		public Bunch(T item, int amount = 0)
		{
			Item = item;
			Amount = amount;
		}

		/// <summary>Multiply the amount by a number.</summary>
		public static Bunch<T> operator *(Bunch<T> bunch, float multiply)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount * multiply));
		}

		/// <summary>Divide the amount by a number.</summary>
		public static Bunch<T> operator /(Bunch<T> bunch, float divide)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount * divide));
		}

		/// <summary>Add a certain amount.</summary>
		public static Bunch<T> operator +(Bunch<T> bunch, float plus)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount + plus));
		}

		/// <summary>Subtract a certain amount.</summary>
		public static Bunch<T> operator -(Bunch<T> bunch, float minus)
		{
			return new Bunch<T>(bunch.Item, (int) (bunch.Amount - minus));
		}

		/// <summary>Multiply the amount by a number.</summary>
		public static Bunch<T> operator *(Bunch<T> bunch, int multiply)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount * multiply);
		}

		/// <summary>Divide the amount by a number.</summary>
		public static Bunch<T> operator /(Bunch<T> bunch, int divide)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount * divide);
		}

		/// <summary>Add a certain amount.</summary>
		public static Bunch<T> operator +(Bunch<T> bunch, int plus)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount + plus);
		}

		/// <summary>Subtract a certain amount.</summary>
		public static Bunch<T> operator -(Bunch<T> bunch, int minus)
		{
			return new Bunch<T>(bunch.Item, bunch.Amount - minus);
		}

		/// <summary>Multiply the amount by another bunch's amount.</summary>
		public static Bunch<T> operator *(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount * bunch2.Amount);
		}

		/// <summary>Divide the amount by a another bunch's amount.</summary>
		public static Bunch<T> operator /(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount / bunch2.Amount);
		}

		/// <summary>Add another bunch's amount to this.</summary>
		public static Bunch<T> operator +(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount + bunch2.Amount);
		}

		/// <summary>Subtract another bunch's amount from this.</summary>
		public static Bunch<T> operator -(Bunch<T> bunch1, Bunch<T> bunch2)
		{
			return new Bunch<T>(bunch1.Item, bunch1.Amount - bunch2.Amount);
		}

		/// <summary>Convert to <c>KeyValuePair&lt;<typeparamref name="T" />, int&gt;</c>.</summary>
		public KeyValuePair<T, int> ToKVP()
		{
			return new KeyValuePair<T, int>(Item, Amount);
		}
	}
}