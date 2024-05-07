using Kit;
using UnityEngine;

namespace Game
{
	public interface ILeveled
	{
		int Level { get; }
		int MaxLevel { get; }
	}

	public static class LeveledExtensions
	{
		public static bool IsMaxed(this ILeveled leveled)
		{
			return leveled.Level >= leveled.MaxLevel;
		}
	}

	public interface IXPLeveled
	{
		int XP { get; set; }
		int[] XPCurve { get; }

		void OnGainedXP(int amount);
		void OnLeveledUp();
	}

	public static class XPLeveledExtensions
	{
		public static void GainXP(this IXPLeveled leveled, int xp)
		{
			bool levelUp = leveled.GetXPToNextLevel() <= xp;

			leveled.XP += xp;
			leveled.OnGainedXP(xp);

			if (levelUp)
				leveled.OnLeveledUp();
		}

		public static int GetMaxLevel(this IXPLeveled leveled)
		{
			return leveled.XPCurve.Length + 1;
		}

		public static int GetLevelFromXP(this IXPLeveled leveled)
		{
			int level = leveled.XPCurve.FindIndex(xp => xp >= leveled.XP) + 1;
			return Mathf.Clamp(level, 1, leveled.GetMaxLevel());
		}

		public static bool IsMaxed(this IXPLeveled leveled)
		{
			return leveled.GetLevelFromXP() >= leveled.GetMaxLevel();
		}

		public static int GetNextLevelXP(this IXPLeveled leveled)
		{
			int level = leveled.GetLevelFromXP();
			return level < leveled.GetMaxLevel() ? leveled.XPCurve[level - 1] : int.MaxValue;
		}

		public static int GetPreviousLevelXP(this IXPLeveled leveled)
		{
			int level = leveled.GetLevelFromXP();
			return level > 1 ? leveled.XPCurve[level - 2] : 0;
		}

		public static int GetXPToNextLevel(this IXPLeveled leveled)
		{
			return leveled.GetNextLevelXP() - leveled.XP;
		}
	}
}