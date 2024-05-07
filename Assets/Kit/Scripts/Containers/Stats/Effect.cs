﻿using System;

namespace Kit.Containers
{
	/// <summary>Represents how an <see cref="Effect" /> affects the value.</summary>
	public enum EffectType
	{
		/// <summary>It adds or subtracts a constant.</summary>
		Constant,

		/// <summary>It adds or subtracts a certain percentage.</summary>
		Percentage,

		/// <summary>It multiplies by a certain number.</summary>
		Multiplier
	}

	/// <summary>Denotes a change in the value of a stat.</summary>
	[Serializable]
	public class Effect
	{
		/// <summary>The stat to which the change should apply.</summary>
		public string Stat;

		/// <summary>How does it change the value?</summary>
		public EffectType Type;

		/// <summary>The amount of change.</summary>
		public float Value;

		/// <summary>The function used to calculate the effect value. Returns <see cref="Value" /> by default. Can be overriden.</summary>
		public Func<float> GetValue;

		/// <summary>Create a new <see cref="Effect" />.</summary>
		/// <param name="stat">The ID of the stat.</param>
		/// <param name="value">The Type and Value represented as a string.</param>
		/// <see cref="Parse" />
		/// <example>
		///     <code>new Effect("Health", "+50%")</code> <code>new Effect("Damage", "x2")</code>
		/// </example>
		public Effect(string stat, string value)
		{
			Stat = stat;
			(Type, Value) = Parse(value);
			AssignSimpleGetValue();
		}

		/// <summary>Create a new <see cref="Effect" />.</summary>
		/// <param name="stat">The ID of the stat.</param>
		/// <param name="type">The effect type.</param>
		/// <param name="value">The amount of the effect.</param>
		/// <example>new Effect("Ammo", EffectType.Constant, +5)</example>
		public Effect(string stat, EffectType type, float value)
		{
			Stat = stat;
			Type = type;
			Value = value;
			AssignSimpleGetValue();
		}

		/// <summary>Create a new <see cref="Effect" /> that allows to update depending on other factors.</summary>
		/// <param name="stat">The ID of the stat.</param>
		/// <param name="value">A function that returns a float to perform Value calculations at runtime.</param>
		/// <example>new Effect("Health", () => Level * 2)</example>
		public Effect(string stat, Func<float> getValue)
		{
			Stat = stat;
			Value = 0;
			GetValue = getValue;
		}

		/// <summary>Assign the default <see cref="GetValue" /> function (which just returns the <see cref="GetValue" />).</summary>
		public void AssignSimpleGetValue()
		{
			GetValue = () => Value;
		}

		public override string ToString()
		{
			return Stat + ": " + Convert(this);
		}

		/// <summary>
		///     <para>Converts a string to <see cref="EffectType" /> and value.</para>
		///     <para>
		///         "x" at the beginning of a string translates to <see cref="EffectType.Multiplier" />, a string ending with "%" means
		///         <see cref="EffectType.Percentage" />, while neither of those means <see cref="EffectType.Constant" />. Rest of the string should be
		///         a number denoting the value.
		///     </para>
		/// </summary>
		/// <returns>The <see cref="EffectType" /> and value in the form of a tuple.</returns>
		public static (EffectType, float) Parse(string str)
		{
			(EffectType type, float value) output;
			if (str.StartsWith("x", StringComparison.Ordinal))
			{
				output.type = EffectType.Multiplier;
				output.value = System.Convert.ToSingle(str.Substring(1));
			}
			else if (str.EndsWith("%", StringComparison.Ordinal))
			{
				output.type = EffectType.Percentage;
				output.value = System.Convert.ToSingle(str.Substring(0, str.Length - 1));
			}
			else
			{
				output.type = EffectType.Constant;
				output.value = System.Convert.ToSingle(str);
			}

			return output;
		}

		/// <summary>Converts the <see cref="EffectType" /> and value to a human-readable string.</summary>
		/// <remarks>Conversion is same as the <see cref="Parse" /> function, except in reverse.</remarks>
		/// <seealso cref="Parse" />
		public static string Convert(Effect effect)
		{
			string output = "";

			if (effect == null)
				return "";

			if (effect.Type != EffectType.Multiplier && effect.GetValue() > 0)
				output += "+";

			switch (effect.Type)
			{
				case EffectType.Constant:
					output += effect.GetValue();
					break;

				case EffectType.Multiplier:
					output += "x" + effect.GetValue();
					break;

				case EffectType.Percentage:
					output += effect.GetValue() + "%";
					break;
			}

			return output;
		}
	}
}