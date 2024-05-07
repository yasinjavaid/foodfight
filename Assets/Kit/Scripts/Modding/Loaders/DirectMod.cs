﻿#if MODDING
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kit.Modding.Loaders
{
	/// <summary>A <see cref="IModLoader" /> that loads resources directly from a folder.</summary>
	/// <seealso cref="IModLoader" />
	public class DirectModLoader: IModLoader
	{
		/// <inheritdoc />
		public Mod LoadMod(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			DirectMod mod = new DirectMod(path);
			ModMetadata metadata = mod.Load<ModMetadata>(ModManager.MetadataFile);
			if (metadata == null)
			{
				Debugger.Log(ModManager.LogCategory, $"Could not load metadata for mod \"{path}\"", LogType.Warning);
				return null;
			}

			mod.Metadata = metadata;
			return mod;
		}

		/// <inheritdoc />
		public async UniTask<Mod> LoadModAsync(string path)
		{
			FileAttributes attributes = File.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
				return null;

			DirectMod mod = new DirectMod(path);
			ModMetadata metadata = await mod.LoadAsync<ModMetadata>(ModManager.MetadataFile);
			if (metadata == null)
			{
				Debugger.Log(ModManager.LogCategory, $"Could not load metadata for mod \"{path}\"", LogType.Warning);
				return null;
			}

			mod.Metadata = metadata;
			return mod;
		}
	}

	/// <summary>A mod that loads resources directly from a folder.</summary>
	/// <seealso cref="Mod" />
	public class DirectMod: Mod
	{
		/// <summary>Path to the mod.</summary>
		public string Path { get; }

		/// <summary>Create a new instance with the given path.</summary>
		/// <param name="path">The path.</param>
		public DirectMod(string path)
		{
			Path = path + "/";
		}

		/// <inheritdoc />
		public override string ReadText(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				return File.ReadAllText(fullPath);
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override async UniTask<string> ReadTextAsync(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				using (StreamReader stream = new StreamReader(fullPath))
					return await stream.ReadToEndAsync();
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override byte[] ReadBytes(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				return File.ReadAllBytes(fullPath);
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override async UniTask<byte[]> ReadBytesAsync(string path)
		{
			try
			{
				string fullPath = GetFullPath(path);
				using (FileStream stream = new FileStream(fullPath, FileMode.Open))
				{
					var data = new byte[stream.Length];
					await stream.ReadAsync(data, 0, (int) stream.Length);
					return data;
				}
			}
			catch
			{
				return null;
			}
		}

		/// <inheritdoc />
		public override IEnumerable<string> FindFiles(string path)
		{
			string fullPath = GetFullPath(path);
			if (File.Exists(fullPath))
				return EnumerableExtensions.One(path);

			if (System.IO.Path.HasExtension(path))
				return Enumerable.Empty<string>();

			// There are three ways to match files without an extension:
			// 1) Extract the extension of found files and strap it to input path
			// 2) Extract the directory of input path, and strap it to found filenames
			// 3) Remove the absolute path part from found file paths
			// Only the third can correct typos in input path, but since we yield input path if it exists anyway...

			try
			{
				string fullDir = System.IO.Path.GetDirectoryName(fullPath);
				string fullFile = System.IO.Path.GetFileName(fullPath);
				var foundPaths = Directory.EnumerateFiles(fullDir, $"{fullFile}.*");
				return foundPaths.Select(p => path + System.IO.Path.GetExtension(p));
			}
			catch
			{
				return Enumerable.Empty<string>();
			}
		}

		/// <inheritdoc />
		public override bool Exists(string path)
		{
			return File.Exists(GetFullPath(path));
		}

		/// <summary>Gets the full path to a file.</summary>
		/// <param name="subPath">Path excluding the mod location.</param>
		public string GetFullPath(string subPath)
		{
			return Path + subPath;
		}
	}
}
#endif