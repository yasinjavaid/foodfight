﻿using Cysharp.Threading.Tasks;
using Kit.Pooling;
using UnityEngine;

namespace Kit
{
	/// <summary>Handles spawning/de-spawning and pooling of particle effects.</summary>
	public static class EffectsManager
	{
		/// <summary>The pooling group name to use for effects.</summary>
		public const string Group = "Effects";

		/// <summary>Spawn a <see cref="ParticleSystem" /> and pool it after it finishes.</summary>
		/// <returns>The pool instance.</returns>
		public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab, position);
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		/// <inheritdoc cref="Spawn(ParticleSystem, Vector3)" />
		public static ParticleSystem Spawn(ParticleSystem prefab, Vector3 position, Quaternion rotation)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab, position, rotation);
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		/// <inheritdoc cref="Spawn(ParticleSystem, Vector3)" />
		public static ParticleSystem Spawn(ParticleSystem prefab, Transform parent, bool worldSpace = false)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab, parent, worldSpace);
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		/// <inheritdoc cref="Spawn(ParticleSystem, Vector3)" />
		public static ParticleSystem Spawn(ParticleSystem prefab, Transform parent, Vector3 position)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab);
			Transform transform = particleSystem.transform;
			transform.parent = parent;
			transform.localPosition = position;
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		/// <inheritdoc cref="Spawn(ParticleSystem, Vector3)" />
		public static ParticleSystem Spawn(ParticleSystem prefab, Transform parent, Vector3 position, Quaternion rotation)
		{
			if (prefab == null)
				return null;

			ParticleSystem particleSystem = Pooler.Instantiate(Group, prefab);
			Transform transform = particleSystem.transform;
			transform.parent = parent;
			transform.localPosition = position;
			transform.localRotation = rotation;
			QueueForDestroy(particleSystem);
			return particleSystem;
		}

		/// <summary>Manually de-spawn a particle effect.</summary>
		/// <returns>Whether the instance was successfully de-spawned.</returns>
		public static bool Despawn(Component instance)
		{
			return Pooler.Destroy(instance);
		}

		/// <summary>De-spawn all instances of a particular effect.</summary>
		/// <returns>Whether instances existed and were successfully de-spawned.</returns>
		public static bool DespawnAll(Component prefab)
		{
			return Pooler.DestroyAll(prefab);
		}

		/// <summary>De-spawn all instances of a particular effect by name.</summary>
		/// <returns>Whether instances existed and were successfully de-spawned.</returns>
		public static bool DespawnAll(string name)
		{
			return Pooler.DestroyAll(name);
		}

		/// <summary>De-spawn all particle effects.</summary>
		/// <returns>Whether instances existed and were successfully de-spawned.</returns>
		public static bool DespawnAll()
		{
			return Pooler.DestroyAllInGroup(Group);
		}

		private static void QueueForDestroy(ParticleSystem system)
		{
			if (system.main.loop)
				return;

			ControlHelper.Delay(() => !system.IsAlive(true),
								() => Pooler.Destroy(system),
								system.GetCancellationTokenOnDestroy());
		}
	}
}