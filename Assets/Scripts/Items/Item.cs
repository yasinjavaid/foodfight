using UnityEngine;

namespace Game.Items
{
	public struct ItemUsed
	{
		public Item Item;
	}

	public struct ItemPickedUp
	{
		public Item Item;
		public Player Player;
	}

	public abstract class Item: MonoBehaviour
	{
		public string ID => name;

		public Player Owner { get; protected set; }

		protected new Transform transform;

		protected virtual void Awake()
        {
        	transform = base.transform;
        }

		protected virtual void Start()
		{
		}

		public Item Spawn()
		{
			return Instantiate(this);
		}

		public virtual void PickedUp(Player player)
		{
			MessageBroker.Instance.Publish(new ItemPickedUp { Item = this, Player = player });
		}

		public virtual void Use()
		{
			MessageBroker.Instance.Publish(new ItemUsed { Item = this });
		}

		public virtual bool CanUse => true;
	}
}