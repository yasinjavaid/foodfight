using System.Collections.Generic;
using Kit.Containers;

namespace Game
{
	public class GameState
	{
		public Bunch<Currency> Wealth;
		public Dictionary<string, Achievement> Achievements;
	}
}