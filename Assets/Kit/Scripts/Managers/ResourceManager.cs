﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Kit.Parsers;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

#if MODDING
using Kit.Modding;

#endif

namespace Kit
{
	/// <summary>Locations where game assets can be stored.</summary>
	public enum ResourceFolder
	{
		/// <summary>Main game folder.</summary>
		Data,

		/// <summary>
		///     <para>The streaming assets folder where assets that should not be packaged can be stored.</para>
		///     <list type="bullet">
		///         <item>
		///             <term>Windows</term> <description>.\StreamingAssets</description>
		///         </item>
		///         <item>
		///             <term>macOS</term> <description>./Resources/Data/StreamingAssets</description>
		///         </item>
		///         <item>
		///             <term>iOS</term> <description>./Raw</description>
		///         </item>
		///         <item>
		///             <term>Android</term> <description>jar:file://{Game APK}!/assets</description>
		///         </item>
		///     </list>
		/// </summary>
		StreamingAssets,

		/// <summary>
		///     <para>The persistent data folder where local data can be stored.</para>
		///     <list type="bullet">
		///         <item>
		///             <term>Windows</term> <description>{User}\AppData\LocalLow\{Company}\{Product}</description>
		///         </item>
		///         <item>
		///             <term>macOS</term> <description>{User}/Library/Application Support/{Company}/{Product}</description>
		///         </item>
		///         <item>
		///             <term>iOS</term> <description>/var/mobile/Containers/Data/Application/{GUID}/Documents</description>
		///         </item>
		///         <item>
		///             <term>Android</term> <description>/storage/emulated/0/Android/data/{Package}/files</description>
		///         </item>
		///     </list>
		/// </summary>
		PersistentData,

		/// <summary>Folder(s) for in-game assets that want to be loaded dynamically. Get packaged with the game.</summary>
		Resources
	}

	/// <summary>
	///     A versatile resource management system for loading, unloading, caching, reading, saving and parsing assets with support for
	///     modding and async methods.
	/// </summary>
	/// <remarks>
	///     Can handle file-names without extensions from <see cref="ResourceFolder.Resources" />. Otherwise you have to provide it, as you
	///     can't enumerate and match files in <see cref="ResourceFolder.StreamingAssets" /> on platforms like Android.
	/// </remarks>
	/// <example>
	///     <code>
	/// ResourceManager.Load&lt;Texture&gt;(ResourceFolder.Resources, "Textures/Background");
	/// </code>
	///     <code>
	/// ResourceManager.LoadAsync&lt;GameState&gt;(ResourceFolder.PersistentData, "GameState.json");
	/// </code>
	/// </example>
	public static class ResourceManager
	{
		#region Fields

		/// <summary>Whether to log load and unload events.</summary>
		public const bool LogEvents = true;

		/// <summary>Category name to use for logging messages.</summary>
		public const string LogCategory = "ResourceManager";


		/// <summary>Absolute paths to <see cref="ResourceFolder" />s.</summary>
		// @formatter:off
		public static readonly IReadOnlyDictionary<ResourceFolder, string> Paths = new Dictionary<ResourceFolder, string> {
			{ ResourceFolder.Data, Application.dataPath                       + "/"},
			{ ResourceFolder.StreamingAssets, Application.streamingAssetsPath + "/"},
			{ ResourceFolder.PersistentData, Application.persistentDataPath   + "/"},
			{ ResourceFolder.Resources, Application.dataPath                  + "/Resources/"}
		};
		// @formatter:on

		/// <summary>
		///     Instances of classes to use for parsing data when calling <see cref="Load{T}(string)" autoUpgrade="true" /> /
		///     <see cref="Save(string, object)" autoUpgrade="true" /> methods on a folder other than <see cref="ResourceFolder.Resources" />.
		/// </summary>
		public static readonly List<ResourceParser> Parsers = new List<ResourceParser>
															  {
																  new JsonParser(),
																  new Texture2DParser(),
																  new AudioClipParser(),
																  new TextAssetParser()
															  };

		/// <summary>
		///     <para>Event fired when when any resource is loaded.</para>
		///     <para>
		///         Returns the folder, path, reference to the resource and whether the resource was actually loaded (<see langword="true" />) or
		///         re-used from cache (<see langword="false" />).
		///     </para>
		/// </summary>
		public static event Action<ResourceFolder, string, object, bool> ResourceLoaded;

		/// <summary>
		///     <para>Event fired when when any resource is unloaded.</para>
		///     <para>Returns the folder and path to the resource unloaded.</para>
		/// </summary>
		public static event Action<ResourceFolder, string> ResourceUnloaded;

		/// <summary>Default mode for modding in individual calls. Has no effect if MODDING is not defined.</summary>
		private const bool DefaultModding = true;

		/// <summary>
		///     Dictionary that holds references to all loaded resources so we don't have to keep loading them. References are held as a
		///     <see cref="WeakReference" /> so they can be let go when not in use.
		/// </summary>
		private static Dictionary<(Type type, ResourceFolder folder, string file), WeakReference> cachedResources =
			new Dictionary<(Type, ResourceFolder, string), WeakReference>();

		#endregion

		#region Initialization

		static ResourceManager()
		{
			if (LogEvents)
			{
				ResourceLoaded += (folder, file, obj, loaded) =>
									  Debugger.Log(LogCategory, $"{(loaded ? "Loaded" : "Reused")} \"{file}\" from {folder}.");

				ResourceUnloaded += (folder, file) =>
										Debugger.Log(LogCategory, $"Unloaded \"{file}\" from {folder}.");
			}
		}

		#endregion

		#region Loading

		/// <summary>
		///     <para>Load and cache a resource.</para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the asset is loaded with <c>Resources.Load</c>. If it's
		///         not, it's parsed manually with the list of parsers registered.
		///     </para>
		/// </summary>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <param name="modded">
		///     Whether to allow mods to load their version of the asset instead. Useful if you want to allow some assets to be
		///     modded, but not others. Has no effect if MODDING is not defined.
		/// </param>
		/// <param name="merge">
		///     Whether to merge the game version and all mod versions of the asset. Useful to allow modding of configuration files.
		///     Has no effect if MODDING is not defined or <paramref name="modded" /> is false.
		/// </param>
		/// <typeparam name="T">Type of the resource expected.</typeparam>
		/// <returns>Reference to the resource, or <see langword="null" /> if it was not found.</returns>

		#region Load(folder, file)

		public static T Load<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false)
		{
			Type type = typeof(T);
#if MODDING
			if (modded)
			{
				if (merge)
					return (T) LoadMerged(type, folder, file);

				object moddedFile = ModManager.Load(type, folder, file);
				if (moddedFile != null)
					return (T) moddedFile;
			}
#endif
			return (T) LoadUnmodded(type, folder, file);
		}

		/// <inheritdoc cref="Load{T}(ResourceFolder, string, bool, bool)" />
		/// <param name="type">Type of the resource expected.</param>
		public static object Load(Type type, ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false)
		{
#if MODDING
			if (modded)
			{
				if (merge)
					return LoadMerged(type, folder, file);

				object moddedFile = ModManager.Load(type, folder, file);
				if (moddedFile != null)
					return moddedFile;
			}
#endif
			return LoadUnmodded(type, folder, file);
		}

		/// <inheritdoc cref="Load{T}(ResourceFolder, string, bool, bool)" />
		/// <summary>
		///     <para>Load and cache a resource asynchronously.</para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the asset is loaded with <c>Resources.Load</c>. If it's
		///         not, it's parsed manually with the list of parsers registered.
		///     </para>
		/// </summary>
		public static async UniTask<T> LoadAsync<T>(ResourceFolder folder, string file, bool modded = DefaultModding, bool merge = false)
		{
			Type type = typeof(T);
#if MODDING
			if (modded)
			{
				if (merge)
					return (T) await LoadMergedAsync(type, folder, file);

				object moddedFile = await ModManager.LoadAsync(type, folder, file);
				if (moddedFile != null)
					return (T) moddedFile;
			}
#endif
			return (T) await LoadUnmoddedAsync(type, folder, file);
		}

		/// <inheritdoc cref="Load(Type, ResourceFolder, string, bool, bool)" />
		/// <summary>
		///     <para>Load and cache a resource asynchronously.</para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the asset is loaded with <c>Resources.Load</c>. If it's
		///         not, it's parsed manually with the list of parsers registered.
		///     </para>
		/// </summary>
		public static async UniTask<object> LoadAsync(Type type,
													  ResourceFolder folder,
													  string file,
													  bool modded = DefaultModding,
													  bool merge = false)
		{
#if MODDING
			if (modded)
			{
				if (merge)
					return await LoadMergedAsync(type, folder, file);

				object moddedFile = await ModManager.LoadAsync(type, folder, file);
				if (moddedFile != null)
					return moddedFile;
			}
#endif
			return await LoadUnmoddedAsync(type, folder, file);
		}

		/// <summary>Load a resource from cache.</summary>
		/// <param name="type">Type of the resource expected.</param>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>Reference to the resource, or <see langword="null" /> if it was not found.</returns>
		private static object LoadCached(Type type, ResourceFolder folder, string file)
		{
			if (!cachedResources.TryGetValue((type, folder, file), out WeakReference weakReference))
				return null;

			object reference = weakReference.Target;
			if (reference == null)
				return null;

			ResourceLoaded?.Invoke(folder, file, reference, false);
			return reference;
		}

		/// <summary>
		///     <para>Load and cache a resource without regarding mods.</para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the asset is loaded with <c>Resources.Load</c>. If it's
		///         not, it's parsed manually with the list of parsers registered.
		///     </para>
		/// </summary>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <typeparam name="T">Type of the resource expected.</typeparam>
		/// <returns>Reference to the resource, or <see langword="null" /> if it was not found.</returns>
		public static T LoadUnmodded<T>(ResourceFolder folder, string file)
		{
			return (T) LoadUnmodded(typeof(T), folder, file);
		}

		/// <inheritdoc cref="LoadUnmodded{T}(ResourceFolder, string)" />
		/// <param name="type">Type of the resource expected.</param>
		public static object LoadUnmodded(Type type, ResourceFolder folder, string file)
		{
			object reference = LoadCached(type, folder, file);
			if (reference != null)
				return reference;

			if (folder == ResourceFolder.Resources)
			{
				string fileNoExt = Path.ChangeExtension(file, null);
				reference = Resources.Load(fileNoExt, type);
			}
			else
			{
				string fullPath = GetPath(folder, file);
				reference = LoadEx(type, fullPath).reference;
			}

			if (reference == null)
				return null;

			// Important to use [key], not Add(key) because the latter generates an error if key exists
			cachedResources[(type, folder, file)] = new WeakReference(reference);
			ResourceLoaded?.Invoke(folder, file, reference, true);
			return reference;
		}

		/// <inheritdoc cref="LoadUnmodded{T}(ResourceFolder, string)" />
		/// <summary>
		///     <para>Load and cache a resource asynchronously without regarding mods.</para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the asset is loaded with  If it's not, it's parsed
		///         manually with the list of parsers registered.
		///     </para>
		/// </summary>
		public static async UniTask<T> LoadUnmoddedAsync<T>(ResourceFolder folder, string file)
		{
			return (T) await LoadUnmoddedAsync(typeof(T), folder, file);
		}

		/// <inheritdoc cref="LoadUnmodded(Type, ResourceFolder, string)" />
		/// <summary>
		///     <para>Load and cache a resource asynchronously without regarding mods.</para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the asset is loaded with  If it's not, it's parsed
		///         manually with the list of parsers registered.
		///     </para>
		/// </summary>
		public static async UniTask<object> LoadUnmoddedAsync(Type type, ResourceFolder folder, string file)
		{
			object reference = LoadCached(type, folder, file);
			if (reference != null)
				return reference;

			if (folder == ResourceFolder.Resources)
			{
				string fileNoExt = Path.ChangeExtension(file, null);
				reference = await Resources.LoadAsync(fileNoExt, type);
			}
			else
			{
				string fullPath = GetPath(folder, file);
				reference = (await LoadExAsync(type, fullPath)).reference;
			}

			if (reference == null)
				return null;

			cachedResources[(type, folder, file)] = new WeakReference(reference);
			ResourceLoaded?.Invoke(folder, file, reference, true);
			return reference;
		}

		#endregion

		#region LoadMerged(folder, file)

#if MODDING
		/// <summary>
		///     <para>
		///         Load a resource merging the game version with all the mod versions and cache it. Useful to allow modding of configuration files
		///         like Json.
		///     </para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the base asset is loaded with  If it's not, it's parsed
		///         manually with the list of parsers registered.
		///     </para>
		/// </summary>
		/// <param name="folder">The folder to load the resource from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <typeparam name="T">Type of the resource expected.</typeparam>
		/// <returns>Reference to the resource, or <see langword="null" /> if it was not found.</returns>
		public static T LoadMerged<T>(ResourceFolder folder, string file)
		{
			return (T) LoadMerged(typeof(T), folder, file);
		}

		/// <inheritdoc cref="LoadMerged{T}(ResourceFolder, string)" />
		/// <param name="type">Type of the resource expected.</param>
		public static object LoadMerged(Type type, ResourceFolder folder, string file)
		{
			object reference = LoadCached(type, folder, file);
			if (reference != null)
				return reference;

			ResourceParser parser;
			string fullPath = GetPath(folder, file);
			if (folder == ResourceFolder.Resources)
			{
				string fileNoExt = Path.ChangeExtension(file, null);
				reference = Resources.Load(fileNoExt, type);
				if (reference == null)
					return null;
				parser = RankParsers(type, fullPath).FirstOrDefault().parser;
			}
			else
			{
				(reference, parser) = LoadEx(type, fullPath);
				if (reference == null)
					return null;
			}

			object merged = reference;
			if (parser != null)
				try
				{
					if (parser.ParseMode == ParseMode.Text)
					{
						var textList = ModManager.ReadTextAll(folder, file);
						foreach (string text in textList)
							parser.Merge(merged, text);
					}
					else
					{
						var bytesList = ModManager.ReadBytesAll(folder, file);
						foreach (var bytes in bytesList)
							parser.Merge(merged, bytes);
					}
				}
				catch (Exception e)
				{
					Debugger.Log(LogCategory, e.Message, LogType.Error);
				}

			cachedResources[(type, folder, file)] = new WeakReference(merged);
			ResourceLoaded?.Invoke(folder, file, merged, true);
			return merged;
		}

		/// <inheritdoc cref="LoadMerged{T}(ResourceFolder, string)" />
		/// <summary>
		///     <para>
		///         Load a resource asynchronously merging the game version with all the mod versions and cache it. Useful to allow modding of
		///         configuration files like Json.
		///     </para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the base asset is loaded with <c>Resources.Load</c>. If
		///         it's not, it's parsed manually with the list of parsers registered.
		///     </para>
		/// </summary>
		public static async UniTask<T> LoadMergedAsync<T>(ResourceFolder folder, string file)
		{
			return (T) await LoadMergedAsync(typeof(T), folder, file);
		}

		/// <inheritdoc cref="LoadMerged(Type, ResourceFolder, string)" />
		/// <summary>
		///     <para>
		///         Load a resource asynchronously merging the game version with all the mod versions and cache it. Useful to allow modding of
		///         configuration files like Json.
		///     </para>
		///     <para>
		///         If <paramref name="folder" /> is <see cref="ResourceFolder.Resources" /> the base asset is loaded with <c>Resources.Load</c>. If
		///         it's not, it's parsed manually with the list of parsers registered.
		///     </para>
		/// </summary>
		public static async UniTask<object> LoadMergedAsync(Type type, ResourceFolder folder, string file)
		{
			object reference = LoadCached(type, folder, file);
			if (reference != null)
				return reference;

			ResourceParser parser;
			string fullPath = GetPath(folder, file);
			if (folder == ResourceFolder.Resources)
			{
				string fileNoExt = Path.ChangeExtension(file, null);
				reference = await Resources.LoadAsync(fileNoExt, type);
				if (reference == null)
					return null;
				parser = RankParsers(type, fullPath).FirstOrDefault().parser;
			}
			else
			{
				(reference, parser) = await LoadExAsync(type, fullPath);
				if (reference == null)
					return null;
			}

			object merged = reference;
			if (parser != null)
				try
				{
					if (parser.ParseMode == ParseMode.Text)
					{
						var textList = await ModManager.ReadTextAllAsync(folder, file);
						foreach (string text in textList)
							parser.Merge(merged, text);
					}
					else
					{
						var bytesList = await ModManager.ReadBytesAllAsync(folder, file);
						foreach (var bytes in bytesList)
							parser.Merge(merged, bytes);
					}
				}
				catch (Exception e)
				{
					Debugger.Log(LogCategory, e.Message, LogType.Error);
				}

			cachedResources[(type, folder, file)] = new WeakReference(merged);
			ResourceLoaded?.Invoke(folder, file, merged, true);
			return merged;
		}
#endif

		#endregion

		#region Load(fullPath)

		/// <summary>Load a resource from an absolute path with the list of parsers registered. Does not cache.</summary>
		/// <param name="fullPath">Absolute path to the resource.</param>
		/// <typeparam name="T">Type of the resource expected.</typeparam>
		/// <returns>Reference to the resource, or <see langword="null" /> if it was not found.</returns>
		public static T Load<T>(string fullPath)
		{
			return (T) LoadEx(typeof(T), fullPath).reference;
		}

		/// <inheritdoc cref="Load{T}(string)" />
		/// <param name="type">Type of the resource expected.</param>
		public static object Load(Type type, string fullPath)
		{
			return LoadEx(type, fullPath).reference;
		}

		/// <inheritdoc cref="Load{T}(string)" />
		/// <returns>Reference to the resource and the parser used to decode it.</returns>
		public static (object reference, ResourceParser parser) LoadEx(Type type, string fullPath)
		{
			string text = null;
			byte[] bytes = null;
			foreach ((ResourceParser parser, float _) in RankParsers(type, fullPath))
				try
				{
					if (parser.ParseMode == ParseMode.Text)
					{
						if (text == null)
						{
							text = ReadText(fullPath);
							if (text == null)
								return default;
						}

						return (parser.Read(type, text, fullPath), parser);
					}

					if (bytes == null)
					{
						bytes = ReadBytes(fullPath);
						if (bytes == null)
							return default;
					}

					return (parser.Read(type, bytes, fullPath), parser);
				}
				catch (Exception)
				{
					// Ignore parsing errors, continue on to the next parser
				}

			return default;
		}

		/// <inheritdoc cref="Load{T}(string)" />
		/// <summary>Load a resource asynchronously from an absolute path with the list of parsers registered. Does not cache.</summary>
		public static async UniTask<T> LoadAsync<T>(string fullPath)
		{
			return (T) (await LoadExAsync(typeof(T), fullPath)).reference;
		}

		/// <inheritdoc cref="Load(Type, string)" />
		/// <summary>Load a resource asynchronously from an absolute path with the list of parsers registered. Does not cache.</summary>
		public static async UniTask<object> LoadAsync(Type type, string fullPath)
		{
			return (await LoadExAsync(type, fullPath)).reference;
		}

		/// <inheritdoc cref="LoadEx(Type, string)" />
		/// <summary>Load a resource asynchronously from an absolute path with the list of parsers registered. Does not cache.</summary>
		public static async UniTask<(object reference, ResourceParser parser)> LoadExAsync(Type type, string fullPath)
		{
			string text = null;
			byte[] bytes = null;
			foreach ((ResourceParser parser, float _) in RankParsers(type, fullPath))
				try
				{
					if (parser.ParseMode == ParseMode.Text)
					{
						if (text == null)
						{
							text = await ReadTextAsync(fullPath);
							if (text == null)
								return default;
						}

						return (parser.Read(type, text, fullPath), parser);
					}

					if (bytes == null)
					{
						bytes = await ReadBytesAsync(fullPath);
						if (bytes == null)
							return default;
					}

					return (parser.Read(type, bytes, fullPath), parser);
				}
				catch (Exception)
				{
					// Ignore parsing errors, continue on to the next parser
				}

			return default;
		}

		#endregion

		#region Unload

		/// <summary>Unload a resource from cache and memory.</summary>
		/// <param name="reference">Reference to the resource.</param>
		/// <returns>Whether the resource was successfully unloaded.</returns>
		public static bool Unload(object reference)
		{
			if (reference == null)
				return false;

#if MODDING
			if (ModManager.Unload(reference))
				return true;
#endif

			(Type type, ResourceFolder folder, string file) key = cachedResources.FirstOrDefault(kvp => kvp.Value.Target == reference).Key;

			if (reference is Object unityObject)
			{
				if (key.file == null || key.folder == ResourceFolder.Resources)
					Resources.UnloadAsset(unityObject);
				else
					unityObject.Destroy();
			}

			// Because of FirstOrDefault, if key is not found "file" will be null
			if (key.file != null)
			{
				cachedResources.Remove(key);
				ResourceUnloaded?.Invoke(key.folder, key.file);
				return true;
			}

			return false;
		}

		/// <summary>Unload a resource from cache and memory.</summary>
		/// <param name="folder">The folder from where the resource was loaded.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <typeparam name="T">Type of the resource.</typeparam>
		/// <returns>Whether the resource was successfully unloaded.</returns>
		public static bool Unload<T>(ResourceFolder folder, string file)
		{
			return Unload(typeof(T), folder, file);
		}

		/// <inheritdoc cref="Unload{T}(ResourceFolder, string)" />
		/// <param name="type">Type of the resource.</param>
		public static bool Unload(Type type, ResourceFolder folder, string file)
		{
#if MODDING
			if (ModManager.Unload(type, folder, file))
				return true;
#endif

			(Type type, ResourceFolder folder, string file) key = (type, folder, file);
			if (!cachedResources.TryGetValue(key, out WeakReference weakReference))
				return false;

			if (weakReference.Target is Object unityObject)
			{
				if (folder == ResourceFolder.Resources)
					Resources.UnloadAsset(unityObject);
				else
					unityObject.Destroy();
			}

			cachedResources.Remove(key);
			ResourceUnloaded?.Invoke(folder, file);
			return true;
		}

		/// <summary>Clear the cache and (optionally) unload assets not in use.</summary>
		/// <param name="unload">Whether to unload assets.</param>
		public static void ClearCache(bool unload = false)
		{
			cachedResources.Clear();
			if (unload)
				Resources.UnloadUnusedAssets();
		}

		/// <summary>Clear the cache and unload assets not in use asynchronously.</summary>
		public static async UniTask ClearCacheAsync()
		{
			cachedResources.Clear();
			await Resources.UnloadUnusedAssets();
		}

		#endregion

		#endregion

		#region Reading

		/// <summary>Read the contents of a file in text-mode.</summary>
		/// <param name="folder">The folder to read the file from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <param name="modded">Whether to allow mods to provide their version of the file instead. Has no effect if MODDING is not defined.</param>
		/// <returns>Contents of the file as a <see cref="string" />.</returns>
		public static string ReadText(ResourceFolder folder, string file, bool modded = DefaultModding)
		{
#if MODDING
			if (modded)
			{
				string moddedFile = ModManager.ReadText(folder, file);
				if (moddedFile != null)
					return moddedFile;
			}
#endif
			return ReadText(GetPath(folder, file));
		}

		/// <inheritdoc cref="ReadText(ResourceFolder, string, bool)" />
		/// <summary>Read the contents of a file asynchronously in text-mode.</summary>
		public static async UniTask<string> ReadTextAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
		{
#if MODDING
			if (modded)
			{
				string moddedFile = await ModManager.ReadTextAsync(folder, file);
				if (moddedFile != null)
					return moddedFile;
			}
#endif
			return await ReadTextAsync(GetPath(folder, file));
		}

		/// <summary>Read the contents of a file in binary-mode.</summary>
		/// <param name="folder">The folder to read the file from.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <param name="modded">Whether to allow mods to provide their version of the file instead. Has no effect if MODDING is not defined.</param>
		/// <returns>Contents of the file as a byte array.</returns>
		public static byte[] ReadBytes(ResourceFolder folder, string file, bool modded = DefaultModding)
		{
#if MODDING
			if (modded)
			{
				var moddedFile = ModManager.ReadBytes(folder, file);
				if (moddedFile != null)
					return moddedFile;
			}
#endif
			return ReadBytes(GetPath(folder, file));
		}

		/// <inheritdoc cref="ReadBytes(ResourceFolder, string, bool)" />
		/// <summary>Read the contents of a file asynchronously in text-mode.</summary>
		public static async UniTask<byte[]> ReadBytesAsync(ResourceFolder folder, string file, bool modded = DefaultModding)
		{
#if MODDING
			if (modded)
			{
				var moddedFile = await ModManager.ReadBytesAsync(folder, file);
				if (moddedFile != null)
					return moddedFile;
			}
#endif
			return await ReadBytesAsync(GetPath(folder, file));
		}

		/// <inheritdoc cref="ReadText(ResourceFolder, string, bool)" />
		/// <param name="fullPath">Absolute path to the file.</param>
		public static string ReadText(string fullPath)
		{
			try
			{
				return File.ReadAllText(fullPath);
			}
			catch (Exception e)
			{
				Debugger.Log(LogCategory, e.Message, LogType.Error);
				return null;
			}
		}

		/// <inheritdoc cref="ReadText(string)" />
		/// <summary>Read the contents of a file asynchronously in text-mode.</summary>
		public static async UniTask<string> ReadTextAsync(string fullPath)
		{
			UnityWebRequest request = await WebAsync(fullPath);
#if UNITY_2020_1_OR_NEWER
			if (request.result != UnityWebRequest.Result.Success)
				return null;
#else
			if (request.isHttpError || request.isNetworkError)
				return null;
#endif
			return request.downloadHandler.text;
		}

		/// <inheritdoc cref="ReadBytes(ResourceFolder, string, bool)" />
		/// <param name="fullPath">Absolute path to the file.</param>
		public static byte[] ReadBytes(string fullPath)
		{
			try
			{
				return File.ReadAllBytes(fullPath);
			}
			catch (Exception e)
			{
				Debugger.Log(LogCategory, e.Message, LogType.Error);
				return null;
			}
		}

		/// <inheritdoc cref="ReadBytes(string)" />
		/// <summary>Read the contents of a file asynchronously in binary-mode.</summary>
		public static async UniTask<byte[]> ReadBytesAsync(string fullPath)
		{
			UnityWebRequest request = await WebAsync(fullPath);
#if UNITY_2020_1_OR_NEWER
			if (request.result != UnityWebRequest.Result.Success)
				return null;
#else
			if (request.isHttpError || request.isNetworkError)
				return null;
#endif
			return request.downloadHandler.data;
		}

		private static UnityWebRequestAsyncOperation WebAsync(string filePath)
		{
			UnityWebRequest request = UnityWebRequest.Get(LocalToUrlPath(filePath));
			return request.SendWebRequest();
		}

		#endregion

		#region Saving/Deleting

		/// <summary>Save the contents of an object to a file.</summary>
		/// <remarks>A parser is chosen based on the type of the object to serialize it.</remarks>
		/// <param name="folder">The folder where the file should exist.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <param name="contents">The object to save.</param>
		/// <returns>Whether the file was successfully saved.</returns>
		public static bool Save(ResourceFolder folder, string file, object contents)
		{
			return Save(GetPath(folder, file), contents);
		}

		/// <inheritdoc cref="Save(ResourceFolder, string, object)" />
		/// <summary>Save the contents of an object asynchronously to a file.</summary>
		public static UniTask<bool> SaveAsync(ResourceFolder folder, string file, object contents)
		{
			return SaveAsync(GetPath(folder, file), contents);
		}

		/// <inheritdoc cref="Save(ResourceFolder, string, object)" />
		/// <param name="fullPath">Absolute path to the file.</param>
		public static bool Save(string fullPath, object contents)
		{
			foreach ((ResourceParser parser, float _) in RankParsers(contents.GetType(), fullPath))
				try
				{
					object output = parser.Write(contents, fullPath);
					if (output != null)
						return parser.ParseMode == ParseMode.Text ?
								   SaveText(fullPath, (string) output) :
								   SaveBytes(fullPath, (byte[]) output);
				}
				catch
				{
					// Ignore parsing errors, continue on to the next parser
				}

			return false;
		}

		/// <inheritdoc cref="SaveAsync(ResourceFolder, string, object)" />
		/// <param name="fullPath">Absolute path to the file.</param>
		public static UniTask<bool> SaveAsync(string fullPath, object contents)
		{
			foreach ((ResourceParser parser, float _) in RankParsers(contents.GetType(), fullPath))
				try
				{
					object output = parser.Write(contents, fullPath);
					if (output != null)
						return parser.ParseMode == ParseMode.Text ?
								   SaveTextAsync(fullPath, (string) output) :
								   SaveBytesAsync(fullPath, (byte[]) output);
				}
				catch
				{
					// Ignore parsing errors, continue on to the next parser
				}

			return UniTask.FromResult(false);
		}

		/// <summary>Save text content to a file.</summary>
		/// <param name="folder">The folder where the file should exist.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <param name="contents">The string to save.</param>
		/// <param name="append">Whether to append to the file, or overwrite it.</param>
		/// <returns>Whether the file was successfully saved.</returns>
		public static bool SaveText(ResourceFolder folder, string file, string contents, bool append = false)
		{
			return SaveText(GetPath(folder, file), contents, append);
		}

		/// <inheritdoc cref="SaveText(ResourceFolder, string, string)" />
		/// <summary>Save text content asynchronously to a file.</summary>
		public static UniTask<bool> SaveTextAsync(ResourceFolder folder, string file, string contents, bool append = false)
		{
			return SaveTextAsync(GetPath(folder, file), contents, append);
		}

		/// <summary>Save binary content to a file.</summary>
		/// <param name="folder">The folder where the file should exist.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <param name="bytes">The byte array to save.</param>
		/// <param name="append">Whether to append to the file or overwrite it.</param>
		/// <returns>Whether the file was successfully saved.</returns>
		public static bool SaveBytes(ResourceFolder folder, string file, byte[] bytes)
		{
			return SaveBytes(GetPath(folder, file), bytes);
		}

		/// <inheritdoc cref="SaveBytes(ResourceFolder, string, byte[])" />
		/// <summary>Save binary content asynchronously to a file.</summary>
		public static UniTask<bool> SaveBytesAsync(ResourceFolder folder, string file, byte[] bytes)
		{
			return SaveBytesAsync(GetPath(folder, file), bytes);
		}

		/// <inheritdoc cref="SaveText(ResourceFolder, string, string)" />
		/// <param name="fullPath">Absolute path to the file.</param>
		public static bool SaveText(string fullPath, string contents, bool append = false)
		{
			try
			{
				CreateDirectoryForFile(fullPath);
				if (append)
					File.AppendAllText(fullPath, contents);
				else
					File.WriteAllText(fullPath, contents);
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log(LogCategory, e.Message, LogType.Error);
				return false;
			}
		}

		/// <inheritdoc cref="SaveText(string, string)" />
		/// <summary>Save text content asynchronously to a file.</summary>
		public static async UniTask<bool> SaveTextAsync(string fullPath, string contents, bool append = false)
		{
			try
			{
				CreateDirectoryForFile(fullPath);
				using (StreamWriter stream = new StreamWriter(fullPath, append))
					await stream.WriteAsync(contents);
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log(LogCategory, e.Message, LogType.Error);
				return false;
			}
		}

		/// <inheritdoc cref="SaveBytes(ResourceFolder, string, byte[])" />
		/// <param name="fullPath">Absolute path to the file.</param>
		public static bool SaveBytes(string fullPath, byte[] bytes)
		{
			try
			{
				CreateDirectoryForFile(fullPath);
				File.WriteAllBytes(fullPath, bytes);
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log(LogCategory, e.Message, LogType.Error);
				return false;
			}
		}

		/// <inheritdoc cref="SaveBytes(string, byte[])" />
		/// <summary>Save binary content asynchronously to a file.</summary>
		public static async UniTask<bool> SaveBytesAsync(string fullPath, byte[] bytes)
		{
			try
			{
				CreateDirectoryForFile(fullPath);
				using (FileStream stream = new FileStream(fullPath, FileMode.Create))
					await stream.WriteAsync(bytes, 0, bytes.Length);
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log(LogCategory, e.Message, LogType.Error);
				return false;
			}
		}

		/// <summary>Delete a file.</summary>
		/// <param name="folder">The folder where the file should exist.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		/// <returns>Whether the file was successfully deleted.</returns>
		public static bool Delete(ResourceFolder folder, string file)
		{
			return Delete(GetPath(folder, file));
		}

		/// <inheritdoc cref="Delete(ResourceFolder, string)" />
		/// <param name="fullPath">Absolute path to the file.</param>
		/// <returns><see langword="true" /> if the file didn't exist or was successfully deleted, <see langword="false" /> otherwise.</returns>
		public static bool Delete(string fullPath)
		{
			try
			{
				File.Delete(fullPath);
				return true;
			}
			catch (Exception e)
			{
				Debugger.Log(LogCategory, e.Message, LogType.Error);
				return false;
			}
		}

		/// <summary>Returns whether a file exists.</summary>
		/// <param name="folder">The folder where to check.</param>
		/// <param name="file">The path and file-name relative to the <paramref name="folder" />.</param>
		public static bool Exists(ResourceFolder folder, string file)
		{
			return Exists(GetPath(folder, file));
		}

		/// <inheritdoc cref="Exists(ResourceFolder, string)" />
		/// <param name="fullPath">Absolute path to the file.</param>
		public static bool Exists(string fullPath)
		{
			return File.Exists(fullPath);
		}

		#endregion

		#region Other

		private static IEnumerable<(ResourceParser parser, float certainty)> RankParsers(Type type, string fullPath)
		{
			return Parsers.Select(parser => (parser, certainty: parser.CanParse(type, fullPath)))
						  .Where(tuple => tuple.certainty > 0)
						  .OrderByDescending(tuple => tuple.certainty);
		}

		/// <summary>Returns whether a path matches one of the extensions specified.</summary>
		public static bool MatchExtension(string path, IEnumerable<string> extensions)
		{
			string extracted = Path.GetExtension(path);
			return extensions.Any(e => e.Equals(extracted, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>Returns whether a path matches the extension specified.</summary>
		public static bool MatchExtension(string path, string extension)
		{
			string extracted = Path.GetExtension(path);
			return extension.Equals(extracted, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Returns whether two paths are equal.</summary>
		public static bool ComparePath(string path1, string path2)
		{
			return path1.Equals(path2, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Create directories for a file.</summary>
		public static void CreateDirectoryForFile(string fullPath)
		{
			string directory = Path.GetDirectoryName(fullPath);
			if (directory != null && !Directory.Exists(directory))
				Directory.CreateDirectory(directory);
		}

		/// <summary>Get the absolute path to a folder.</summary>
		public static string GetPath(ResourceFolder folder)
		{
			return Paths[folder];
		}

		/// <summary>Get the absolute path to a file.</summary>
		public static string GetPath(ResourceFolder folder, string file)
		{
			return GetPath(folder) + file;
		}

		private static string LocalToUrlPath(string path)
		{
			if (!path.Contains("file://"))
				path = "file://" + path;
			return path;
		}

		#endregion
	}
}