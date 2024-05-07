namespace Kit.Pooling
{
	/// <summary>What to do when a pool reaches its limit?</summary>
	public enum PoolLimitMode
	{
		/// <summary>Stop giving new instances.</summary>
		StopGiving,

		/// <summary>Reuse the first instance given.</summary>
		ReuseFirst,

		/// <summary>Create a new instance but do not pool it when it gets destroyed.</summary>
		DestroyAfterUse
	}
}