namespace Kit.Pooling
{
	/// <summary>How should an instance get informed of pooling events?</summary>
	public enum PoolMessageMode
	{
		/// <summary>Do not inform of pooling events.</summary>
		None,

		/// <summary>Call <see cref="IPooled" /> methods on just the instance component.</summary>
		IPooledComponent,

		/// <summary>Call <see cref="IPooled" /> methods on the all instance <see cref="GameObject" /> components.</summary>
		IPooledGameObject,

		/// <summary>Call <see cref="IPooled" /> methods directly on the instance <see cref="GameObject" /> hierarchy.</summary>
		IPooledHierarchy,

		/// <summary>Call <see cref="Component.SendMessage(string)" /> on instance <see cref="GameObject" />s.</summary>
		SendMessage,

		/// <summary>Call <see cref="Component.BroadcastMessage(string)" /> on instance  <see cref="GameObject" />s.</summary>
		BroadcastMessage
	}
}