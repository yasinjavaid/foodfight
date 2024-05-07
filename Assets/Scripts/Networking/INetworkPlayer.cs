using UnityEngine;

namespace Game.Networking
{
	public interface INetworkPlayer
	{
		public Player Player { get; }
		public MonoBehaviour Entity { get; }
		public bool IsLocal { get; }
		public bool IsServer { get; }
		public bool IsObserver { get; }
	}
}