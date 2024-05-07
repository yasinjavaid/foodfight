using Cysharp.Threading.Tasks;
using Kit;

namespace Game
{
	public static class DataManager
	{
		public const ResourceFolder DataFolder = ResourceFolder.StreamingAssets;
		public const string GameDataFile = "Data/GameData.json";
		public const string GameStateFile = "Data/GameState.json";
		public const bool ClearGameState = true;

		public static GameData GameData;
		public static GameState GameState;

		// Makes sure GameData is always loaded in any scene in editor and dev builds
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		static DataManager()
		{
			LoadData();
		}
#endif

		public static void LoadData()
		{
			if (GameData == null)
				GameData = LoadGameData();
			if (GameState == null)
				GameState = LoadGameState(ClearGameState);
		}

		public static async UniTask LoadDataAsync()
		{
			if (GameData == null)
				GameData = await LoadGameDataAsync();
			if (GameState == null)
				GameState = await LoadGameStateAsync(ClearGameState);
		}

		public static GameData LoadGameData()
		{
			return ResourceManager.Load<GameData>(DataFolder, GameDataFile);
		}

		public static UniTask<GameData> LoadGameDataAsync()
		{
			return ResourceManager.LoadAsync<GameData>(DataFolder, GameDataFile);
		}

		public static GameState LoadGameState(bool clearGameState = false)
		{
			if (clearGameState || !ResourceManager.Exists(ResourceFolder.PersistentData, GameStateFile))
				return ResourceManager.Load<GameState>(DataFolder, GameStateFile);
			return ResourceManager.Load<GameState>(ResourceFolder.PersistentData, GameStateFile);
		}

		public static UniTask<GameState> LoadGameStateAsync(bool clearGameState = false)
		{
			if (clearGameState || !ResourceManager.Exists(ResourceFolder.PersistentData, GameStateFile))
				return ResourceManager.LoadAsync<GameState>(DataFolder, GameStateFile);
			return ResourceManager.LoadAsync<GameState>(ResourceFolder.PersistentData, GameStateFile);
		}

		public static void SaveGameState()
		{
			if (!IsGameDataLoaded)
				return;

			ResourceManager.Save(ResourceFolder.PersistentData, GameStateFile, GameState);
		}

		public static UniTask SaveGameStateAsync()
		{
			return IsGameStateLoaded ?
					   ResourceManager.SaveAsync(ResourceFolder.PersistentData, GameStateFile, GameState) :
					   UniTask.CompletedTask;
		}

		public static bool IsGameDataLoaded => GameData   != null;
		public static bool IsGameStateLoaded => GameState != null;
	}
}