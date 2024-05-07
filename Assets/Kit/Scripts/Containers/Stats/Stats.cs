using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace Kit.Containers
{
	/// <summary>
	///     Represents the base and current values of stats of an entity as a dictionary. Can be used with both POCO objects or
	///     <see cref="UnityEngine.MonoBehaviour" />s with Odin's <see cref="Sirenix.OdinInspector.SerializedMonoBehaviour" />.
	/// </summary>
	/// <remarks>
	///     The class is highly-optimized as the current values are only updated when the <see cref="IUpgradeable" /> adds or removes an
	///     <see cref="IUpgrade" /> or the base value of the property changes. When used in a <see cref="UnityEngine.MonoBehaviour" />, it also
	///     provides custom inspectors for viewing and changing base values.
	/// </remarks>
	/// <example>
	///     <code>
	///  public class Player: IUpgradeable
	///  {
	///  	public Stats Stats;
	///  	public AsyncReactiveList&lt;Upgrade&gt; Upgrades = new AsyncReactiveList&lt;Upgrade&gt;();
	///
	///  	public void Initialize()
	///  	{
	///  		Stats = new Stats(this);
	///  		Stats["Health"] = 100.0f;
	///  		Stats["Damage"] = 15.0f;
	///
	///  		Upgrade upgrade = new Upgrade("SturdyHelmet");
	///  		upgrade.AddEffect("Health", "+50");
	///  		upgrade.AddEffect("Damage", "+25%");
	///  		Upgrades.Add(upgrade);
	///
	///  		Debug.Log(Stats.ToString());
	///  	}
	///
	/// 	public AsyncReactiveList&lt;Upgrade&gt; GetUpgrades()
	/// 	{
	/// 		return Upgrades;
	/// 	}
	///  }
	///  </code>
	/// </example>
	public class Stats: Dictionary<string, StatBaseProperty>, IDisposable
	{
		/// <summary>The <see cref="IUpgradeable" /> to use for calculating current values.</summary>
		public IUpgradeable Upgradeable;

		protected readonly Dictionary<string, ReadOnlyAsyncReactiveProperty<float>> currentProperties =
			new Dictionary<string, ReadOnlyAsyncReactiveProperty<float>>();

		protected readonly CancellationTokenSource cancelSource = new CancellationTokenSource();

		public Stats()
		{
		}

		public Stats(IUpgradeable upgradeable)
		{
			Upgradeable = upgradeable;
		}

		/// <summary>Create a new stat collection from a dictionary.</summary>
		public Stats(IUpgradeable upgradeable, Dictionary<string, float> dictionary): this(upgradeable)
		{
			Add(dictionary);
		}

		/// <summary>Returns the current value of a stat or allows to set the base value.</summary>
		public new float this[string stat]
		{
			get => GetCurrentValue(stat);
			set => SetBaseValue(stat, value);
		}

		/// <summary>Set the base value of a stat.</summary>
		public void Add(string stat, float value)
		{
			SetBaseValue(stat, value);
		}

		/// <summary>Set the base values of stats from a dictionary.</summary>
		public void Add(Dictionary<string, float> dictionary)
		{
			foreach ((string stat, float value) in dictionary)
				SetBaseValue(stat, value);
		}

		/// <summary>Get the base value property of a stat.</summary>
		public StatBaseProperty GetBaseProperty(string stat)
		{
			return this.GetOrDefault(stat);
		}

		/// <summary>Set the base value property of a stat.</summary>
		public void SetBaseProperty(string stat, StatBaseProperty value)
		{
			base[stat] = value;
		}

		/// <summary>Get the base value of a stat.</summary>
		public float GetBaseValue(string stat)
		{
			return GetBaseProperty(stat).Value;
		}

		/// <summary>Set the base value of a stat.</summary>
		public void SetBaseValue(string stat, float value)
		{
			if (TryGetValue(stat, out StatBaseProperty property))
				property.Value = value;
			else
			{
				StatBaseProperty baseProperty = new StatBaseProperty(value);
				SetBaseProperty(stat, baseProperty);
			}
		}

		/// <summary>Get the current value property of a stat.</summary>
		public ReadOnlyAsyncReactiveProperty<float> GetCurrentProperty(string stat)
		{
			if (currentProperties.TryGetValue(stat, out var property))
				return property;

			var currentProperty = CreateCurrentProperty(GetBaseProperty(stat), Upgradeable, stat, cancelSource.Token);
			currentProperties.Add(stat, currentProperty);

			return currentProperty;
		}


		/// <summary>Get the current value of a stat.</summary>
		public float GetCurrentValue(string stat)
		{
			return GetCurrentProperty(stat).Value;
		}

		public void Dispose()
		{
			cancelSource.Cancel();
			cancelSource.Dispose();

			foreach (var property in this)
				property.Value.Dispose();

			foreach (var property in currentProperties)
				property.Value.Dispose();
		}

		/// <summary>Create a property that changes values when the base property changes or an upgrade is added or removed.</summary>
		/// <param name="baseProperty">The base property.</param>
		/// <param name="upgradeable">The <see cref="IUpgradeable" /> to get the upgrade list from.</param>
		/// <param name="stat">The stat name.</param>
		/// <returns>A read-only <see cref="AsyncReactiveProperty{T}" />.</returns>
		public static ReadOnlyAsyncReactiveProperty<float> CreateCurrentProperty(AsyncReactiveProperty<float> baseProperty,
																				 IUpgradeable upgradeable,
																				 string stat,
																				 CancellationToken cancelToken)
		{
			// We get the last upgrades that were changed and aggregate them, and then we use CombineLatest
			// to use these aggregates in changing base values
			var observable = UniTaskAsyncEnumerable.EveryUpdate()
								  .Select(_ => GetAggregates(upgradeable, stat))
								  .Prepend(GetAggregates(upgradeable, stat))
								  .CombineLatest(baseProperty, CalculateValue);

			// var observable = upgradeable.GetUpgrades()
			// 							.EveryCountChanged()
			// 							.Select(_ => GetAggregates(upgradeable, stat))
			// 							.Prepend(GetAggregates(upgradeable, stat))
			// 							.CombineLatest(baseProperty, CalculateValue);

			return new ReadOnlyAsyncReactiveProperty<float>(observable, cancelToken);
		}

		/// <summary>Calculate aggregates of an stat.</summary>
		/// <param name="upgradeable">The <see cref="IUpgradeable" /> to get the list of upgrades from.</param>
		/// <param name="stat">Stat to calculate aggregates of.</param>
		/// <returns>
		///     A tuple with <see cref="EffectType.Constant" />, <see cref="EffectType.Percentage" /> and <see cref="EffectType.Multiplier" />
		///     sums respectively.
		/// </returns>
		public static (float, float, float) GetAggregates(IUpgradeable upgradeable, string stat)
		{
			return GetAggregates(GetEffects(upgradeable, stat));
		}

		/// <summary>Calculate the current value of an stat based on base value.</summary>
		/// <param name="upgradeable">The <see cref="IUpgradeable" /> to get the list of upgrades from.</param>
		/// <param name="stat">The stat name to calculate value of.</param>
		/// <param name="baseValue">The base value to use.</param>
		/// <returns>Current value of a stat.</returns>
		public static float CalculateValue(IUpgradeable upgradeable, string stat, float baseValue)
		{
			return CalculateValue(GetAggregates(upgradeable, stat), baseValue);
		}

		/// <summary>Get all upgrades and effects on an <see cref="IUpgradeable" /> for a given stat.</summary>
		public static IEnumerable<(IUpgrade upgrade, IEnumerable<Effect> effects)> GetEffectsAndUpgrades(IUpgradeable upgradeable,
			string stat)
		{
			return upgradeable.GetUpgrades()
							  .Where(u => u != null)
							  .Select(u => (upgrade: u, effects: u.Effects.Where(e => e.Stat == stat)))
							  .Where(g => g.effects.Any());
		}

		/// <summary>Get all effects on an <see cref="IUpgradeable" /> for a given stat.</summary>
		public static IEnumerable<Effect> GetEffects(IUpgradeable upgradeable, string stat)
		{
			return upgradeable.GetUpgrades()
							  .Where(u => u != null)
							  .SelectMany(u => u.Effects)
							  .Where(e => e.Stat == stat);
		}

		/// <summary>Calculate aggregates from a list of effects.</summary>
		/// <param name="effects">The list of effect to calculate aggregates of.</param>
		/// <returns>
		///     A tuple with <see cref="EffectType.Constant" />, <see cref="EffectType.Percentage" /> and <see cref="EffectType.Multiplier" />
		///     sums respectively.
		/// </returns>
		public static (float, float, float) GetAggregates(IEnumerable<Effect> effects)
		{
			float constantSum = 0, percentSum = 100, multiplierSum = 1;
			foreach (Effect effect in effects)
				switch (effect.Type)
				{
					case EffectType.Constant:
						constantSum += effect.GetValue();
						break;

					case EffectType.Percentage:
						percentSum += effect.GetValue();
						break;

					case EffectType.Multiplier:
						multiplierSum *= effect.GetValue();
						break;
				}

			return (constantSum, percentSum, multiplierSum);
		}

		/// <summary>Calculate current value from a base value and aggregates.</summary>
		/// <param name="aggregates">The aggregate tuple to use.</param>
		/// <param name="baseValue">The base value to use.</param>
		/// <returns>Current value.</returns>
		public static float CalculateValue((float value, float percent, float multiplier) aggregates, float baseValue)
		{
			return (baseValue + aggregates.value) * aggregates.percent / 100 * aggregates.multiplier;
		}

		/// <summary>Returns all the base and current values as a formatted table.</summary>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("{0, -20}{1, -15}{2, -15}", "Stat", "Base", "Current");
			builder.AppendLine();
			foreach ((string stat, StatBaseProperty baseProperty) in this)
			{
				builder.AppendFormat("{0, -20}{1, -15}{2, -15}", stat, baseProperty.Value, GetCurrentValue(stat));
				builder.AppendLine();
			}

			return builder.ToString();
		}
	}

	/// <summary>A more inspector-friendly <see cref="AsyncReactiveProperty{T}" /> for use in <see cref="Stats" />.</summary>
	[Serializable]
	public class StatBaseProperty: AsyncReactiveProperty<float>
	{
		public StatBaseProperty(): this(0) { }

		public StatBaseProperty(float initialValue): base(initialValue) { }

		public static implicit operator StatBaseProperty(float f)
		{
			return new StatBaseProperty(f);
		}
	}
}