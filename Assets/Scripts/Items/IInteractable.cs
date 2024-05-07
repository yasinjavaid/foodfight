using UnityEngine;

namespace Game.Items
{
	public interface IInteractable
	{
		public void Interact(Player player);
		public Vector3 Position { get; }
	}
}