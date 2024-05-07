using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;

#endif

namespace Kit.UI
{
	/// <summary>
	///     A <see cref="SoftReference{T}" /> (path-only) to a <see cref="Window" />. Here because Unity can be picky about serializing
	///     generic variables.
	/// </summary>
	[Serializable]
	public class WindowReference: SoftReference<Window>
	{
	}

	/// <summary>A <see cref="SoftReference{T}" /> (path) to a scene.</summary>
	[Serializable]
#if UNITY_EDITOR
	public class SceneReference: SoftReference<SceneAsset>
#else
	public class SceneReference: SoftReference<Object>
#endif
	{
#if UNITY_EDITOR
		// Scenes do not need to be under a "Resources" folder for them to be loaded using their paths,
		// so we're turning off validation for them
		protected override bool OnValidate(SceneAsset newAsset)
		{
			return true;
		}
#endif
	}

	/// <summary>
	///     A class that allows one to select assets in the inspector without hard-referencing them. Saves their path instead which can later
	///     be used to load with <see cref="Load" /> or manually with <see cref="ResourceManager" /> or
	///     <see cref="Resources.Load(string)" qualifyHint="true" />.
	/// </summary>
	/// <remarks>Can be used directly as a string without needing to call <see cref="ToString" />.</remarks>
	/// <typeparam name="T">Type of the unity object. Used to filter assets.</typeparam>
	[Serializable]
	[InlineProperty]
	public class SoftReference<T> where T: Object
	{
		/// <summary>Text to trim paths till.</summary>
		public const string ResourcesFolder = "/Resources/";

		[HideLabel]
		[ReadOnly]
		[SerializeField]
		protected string fullPath;

		/// <summary>Path to the asset with the location to <see cref="ResourcesFolder" /> removed.</summary>
		public string Path => TrimPath(fullPath);

		/// <summary>Full path to the asset.</summary>
		public string FullPath => fullPath;

		/// <summary>Load the asset with <see cref="ResourceManager" />.</summary>
		/// <returns>Reference to the asset.</returns>
		public T Load()
		{
			return ResourceManager.Load<T>(ResourceFolder.Resources, fullPath);
		}

		/// <summary>Load the asset asynchronously with <see cref="ResourceManager" />.</summary>
		public UniTask<T> LoadAsync()
		{
			return ResourceManager.LoadAsync<T>(ResourceFolder.Resources, fullPath);
		}

		/// <summary>Returns the trimmed path to the asset.</summary>
		public override string ToString()
		{
			return Path;
		}

		/// <summary>Returns the trimmed path to the asset.</summary>
		public static implicit operator string(SoftReference<T> reference)
		{
			return reference.Path;
		}

		private string TrimPath(string inputPath)
		{
			int resourcesIndex = inputPath.IndexOf(ResourcesFolder, StringComparison.Ordinal);
			return resourcesIndex > 0 ? inputPath.Substring(resourcesIndex + ResourcesFolder.Length) : inputPath;
		}

#if UNITY_EDITOR
		[ShowInInspector]
		[HideLabel]
		[AssetsOnly] [AssetList]
		[OnInspectorGUI(nameof(OnDraw))]
		[OnValueChanged(nameof(OnChanged))]
		[ValidateInput(nameof(OnValidate), "The asset must be under a \"Resources\" folder for it be to be loaded at runtime.")]
		protected T asset;

		protected virtual void OnDraw()
		{
			if (asset == null && fullPath != string.Empty)
				UnityEditorEventUtility.DelayAction(() => asset = LoadAsset_Editor());
		}

		protected virtual void OnChanged()
		{
			SetAsset_Editor(asset);
		}

		protected virtual bool OnValidate(T newAsset)
		{
			return newAsset == null || AssetDatabase.GetAssetPath(newAsset).Contains(ResourcesFolder);
		}

		public virtual void SetAsset_Editor(T newAsset)
		{
			asset = newAsset;
			fullPath = newAsset != null ? AssetDatabase.GetAssetPath(newAsset) : string.Empty;
		}

		public virtual T LoadAsset_Editor()
		{
			return AssetDatabase.LoadAssetAtPath<T>(fullPath);
		}
#endif
	}
}