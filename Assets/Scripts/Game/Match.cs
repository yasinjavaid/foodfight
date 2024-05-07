using System.Collections.Generic;
using Game.Items;

namespace Game
{
	public enum MapGeneration
	{
		Pregenerated = 0,
		ProceduralSmall = 1,
		ProceduralMedium = 2,
		ProceduralLarge = 3,
	}

	public enum RespawnOptions
	{
		Yes,
		No
	}

	public abstract class Match
	{
		public Dictionary<Pickup, float> PickupSpawnRate = new Dictionary<Pickup, float>();
		public bool HealthPickups = true;
		public bool FriendlyFire = true;

		public MapGeneration MapGeneration = MapGeneration.Pregenerated;
		public int TimeLimit;
		public bool Respawn;

		public List<Player> Players;
		public bool IsOffline;

		public abstract List<MapGeneration> GenerationOptions { get; }
		public abstract List<int> TimeOptions { get; } // In minutes
		public abstract List<bool> RespawnOptions { get; }
	}

	/// <summary>
	///		Levels disintegrate, with holes appearing within a room. Once dissolved, the next rooms starts disappearing.
	///		Last-man-standing wins.
	/// </summary>
	public class CrumblingMatch: Match
	{
		public override List<MapGeneration> GenerationOptions => new List<MapGeneration>
																	{
																		MapGeneration.ProceduralSmall,
																		MapGeneration.ProceduralMedium,
																		MapGeneration.ProceduralLarge
																	};

		public override List<int> TimeOptions => new List<int> {3, 5, 10 };
		public override List<bool> RespawnOptions => new List<bool>() { false };
	}

	/// <summary>
	///		Capture the Flag. An ice-cream cone spawns, which can be picked up. Players gain points when in possession of it.
	///		Drops when killed. Player with most points wins.
	/// </summary>
	public class KingOftheConeMatch: Match
	{
		public override List<MapGeneration> GenerationOptions => new List<MapGeneration>
																	{
																		MapGeneration.Pregenerated,
																		MapGeneration.ProceduralSmall,
																		MapGeneration.ProceduralMedium,
																		MapGeneration.ProceduralLarge
																	};

		public override List<int> TimeOptions => new List<int> {3, 5, 10 };
		public override List<bool> RespawnOptions => new List<bool>() { true };
	}

	/// <summary>
	///		Free-for-all Deathmatch, with boxes that can be used for cover and broken for pickups.
	/// </summary>
	public class MysteryMeatMatch: Match
	{
		public override List<MapGeneration> GenerationOptions => new List<MapGeneration>
																	{
																		MapGeneration.ProceduralSmall,
																		MapGeneration.ProceduralMedium
																	};

		public override List<int> TimeOptions => new List<int> {3, 5, 10 };
		public override List<bool> RespawnOptions => new List<bool>() { true, false };
	}

	/// <summary>
	///		Platforming level with an attacking Chef Boss and an ice-cream cone at the end of the level. Platforming challenges require
	///		weapons.
	/// </summary>
	public class QuestForTheConeMatch: Match
	{
		public override List<MapGeneration> GenerationOptions => new List<MapGeneration>
																	{
																		MapGeneration.Pregenerated
																	};
		public override List<int> TimeOptions => new List<int>();
		public override List<bool> RespawnOptions => new List<bool>() { true };
	}

	/// <summary>
	///		A traditional Deathmatch with Team Deathmatch option.
	/// </summary>
	public class BuffetBrawlMatch: Match
	{
		public override List<MapGeneration> GenerationOptions => new List<MapGeneration>
																	{
																		MapGeneration.Pregenerated,
																		MapGeneration.ProceduralSmall,
																		MapGeneration.ProceduralMedium,
																		MapGeneration.ProceduralLarge
																	};
		public override List<int> TimeOptions => new List<int> {3, 5, 10 };
		public override List<bool> RespawnOptions => new List<bool>() { true, false };
	}
}