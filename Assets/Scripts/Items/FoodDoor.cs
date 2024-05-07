using System;
using Game.Items.Weapons;
using UnityEngine;

namespace Game.Items
{
	public enum FoodType
	{
		Undefined,
		Pea,
		Corn,
		Onion,
		Pepper
	}

	public enum DoorState
	{
		Opened,
		Closed
	}

	public class FoodDoor: MonoBehaviour
	{
		public DoorState State = DoorState.Closed;
		public FoodType FoodType;

		private void Start()
		{
			if (IsOpen)
				Open();
			else
				Close();
		}

		private void Collide(Projectile projectile)
		{
			if (projectile.FoodType == FoodType)
				Toggle();
		}

		public void Toggle()
		{
			if (IsOpen)
				Close();
			else
				Open();
		}

		public void Open()
		{
			State = DoorState.Opened;
			throw new NotImplementedException();

		}

		public void Close()
		{
			State = DoorState.Closed;
			throw new NotImplementedException();
		}

		public bool IsOpen => State   == DoorState.Opened;
		public bool IsClosed => State == DoorState.Closed;
	}
}