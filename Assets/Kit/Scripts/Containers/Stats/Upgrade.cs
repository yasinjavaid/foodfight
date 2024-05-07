using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Containers
{
	/// <summary>
	///     Any entity (POCO or <see cref="Sirenix.OdinInspector.SerializedMonoBehaviour" />) that wishes to have stats should implement this
	///     interface and provide a list of <see cref="IUpgrade" />s.
	/// </summary>
	public interface IUpgradeable
	{
		/// <summary>Return a list of all the upgrades applied.</summary>
		AsyncReactiveList<IUpgrade> GetUpgrades();
	}

	[Serializable]
	/// <summary>A concrete implementation of <see cref="IUpgrade" /> for simple use-cases.</summary>
	public class Upgrade: IUpgrade
	{
		[SerializeField]
		[LabelText("ID")]
		protected string id;

		[SerializeField]
		protected List<Effect> effects;

		public string ID { get => id; set => id = value; }
		public List<Effect> Effects { get => effects; set => effects = value; }

		public Upgrade(): this("")
		{
		}

		public Upgrade(string id)
		{
			ID = id;
			Effects = new List<Effect>();
		}

		public Upgrade(string id, IEnumerable<Effect> effects): this(id)
		{
			this.AddEffects(effects);
		}
	}

	/// <summary>A list of effects to be identified by an ID.</summary>
	public interface IUpgrade
	{
		/// <summary>ID of the upgrade.</summary>
		public string ID { get; }

		/// <summary>List of effects this upgrade causes.</summary>
		public List<Effect> Effects { get; }
	}

	public static class UpgradeExtensions
	{
		/// <summary>Add specified <see cref="Effect" />s.</summary>
		public static void AddEffects(this IUpgrade upgrade, IEnumerable<Effect> effects)
		{
			upgrade.Effects.AddRange(effects);
		}

		/// <summary>Add the specified <see cref="Effect" />.</summary>
		public static void AddEffect(this IUpgrade upgrade, Effect effect)
		{
			upgrade.Effects.Add(effect);
		}

		/// <summary>Create a new <see cref="Effect" /> and add it.</summary>
		/// <param name="stat">The stat ID.</param>
		/// <param name="value">The effect Type and Value represented as a string.</param>
		public static void AddEffect(this IUpgrade upgrade, string stat, string value)
		{
			upgrade.Effects.Add(new Effect(stat, value));
		}

		/// <summary>Create a new <see cref="Effect" /> and add it.</summary>
		/// <param name="stat">The stat ID.</param>
		/// <param name="type">The effect's Type.</param>
		/// <param name="value">The effect's Value.</param>
		public static void AddEffect(this IUpgrade upgrade, string stat, EffectType type, float value)
		{
			upgrade.Effects.Add(new Effect(stat, type, value));
		}

		/// <summary>Create a new dynamically-updating <see cref="Effect" /> and add it.</summary>
		/// <param name="stat">The stat ID.</param>
		/// <param name="getValue">A function that calculates the effect's value.</param>
		public static void AddEffect(this IUpgrade upgrade, string stat, Func<float> getValue)
		{
			upgrade.Effects.Add(new Effect(stat, getValue));
		}

		/// <summary>Remove the specified <see cref="Effect" />.</summary>
		public static void RemoveEffect(this IUpgrade upgrade, Effect effect)
		{
			upgrade.Effects.Remove(effect);
		}

		/// <summary>Remove all <see cref="Effect" />s associated with the given stat ID.</summary>
		public static void RemoveEffects(this IUpgrade upgrade, string stat)
		{
			upgrade.Effects.RemoveAll(e => e.Stat == stat);
		}

		/// <summary>Add the upgrade to an upgradeable's list.</summary>
		/// <param name="upgradeable">The <see cref="IUpgradeable" /> to add it to.</param>
		public static void Apply(this IUpgrade upgrade, IUpgradeable upgradeable)
		{
			upgradeable.GetUpgrades().Add(upgrade);
		}

		/// <summary>Find an <see cref="IUpgrade" /> with its ID.</summary>
		public static IUpgrade Find(IUpgradeable upgradeable, string id)
		{
			return upgradeable.GetUpgrades().FirstOrDefault(upgrade => upgrade.ID == id);
		}

		/// <summary>Remove an <see cref="IUpgrade" /> with its ID.</summary>
		public static bool RemoveFrom(IUpgradeable upgradeable, string id)
		{
			IUpgrade previous = Find(upgradeable, id);
			if (previous == null)
				return false;

			upgradeable.GetUpgrades().Remove(previous);
			return true;
		}
	}
}