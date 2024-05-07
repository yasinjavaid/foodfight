using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kit.Containers
{
	/// <summary>
	///     <para>Represents the base and current value of a single stat.</para>
	///     <para>
	///         Multiple instances of this class be used instead of the <see cref="Stats" /> class if access through individual variables is
	///         desired.
	///     </para>
	/// </summary>
	public class Stat: IDisposable
	{
		/// <summary>The <see cref="IUpgradeable" /> to use for calculating current value.</summary>
		public IUpgradeable Upgradeable;

		/// <summary>The ID of the stat.</summary>
		public string ID;

		/// <summary>The base value property of the stat.</summary>
		public readonly StatBaseProperty Base = new StatBaseProperty();

		protected ReadOnlyAsyncReactiveProperty<float> current;
		protected CancellationTokenSource cancelSource;

		public Stat()
		{
		}

		public Stat(IUpgradeable upgradeable, string id)
		{
			Upgradeable = upgradeable;
			ID = id;
		}

		/// <summary>The current value property of the stat.</summary>
		public ReadOnlyAsyncReactiveProperty<float> Current
		{
			get
			{
				if (current == null)
				{
					cancelSource = new CancellationTokenSource();
					current = Stats.CreateCurrentProperty(Base, Upgradeable, ID, cancelSource.Token);
				}

				return current;
			}
		}

		/// <summary>The base value of the stat.</summary>
		public float BaseValue
		{
			get => Base.Value;
			set => Base.Value = value;
		}

		/// <summary>The current value of the stat.</summary>
		public float CurrentValue => Current.Value;

		/// <summary>Returns the base and current value for display.</summary>
		public override string ToString()
		{
			return $"{ID} — Base: {BaseValue}, Current: {CurrentValue}";
		}

		public void Dispose()
		{
			if (current != null)
			{
				cancelSource.Cancel();
				cancelSource.Dispose();
				Current.Dispose();
			}

			Base.Dispose();
		}
	}
}