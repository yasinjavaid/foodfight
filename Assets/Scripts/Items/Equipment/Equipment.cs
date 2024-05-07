namespace Game.Items.Equipment
{
	/// <summary>Items that grant buffs or abilities.</summary>
	/// <remarks>Also for Movers.</remarks>
	public class Equipment: Item
	{
		/// <summary>Time to enable the equipment for. -1 for Unlimited.</summary>
		public float Time = 3.0f;
	}
}