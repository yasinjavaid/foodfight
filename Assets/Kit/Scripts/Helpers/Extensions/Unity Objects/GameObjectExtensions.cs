using UnityEngine;

namespace Kit
{
	/// <summary><see cref="GameObject" /> extensions.</summary>
	public static class GameObjectExtensions
	{
		/// <summary>Returns whether a <see cref="GameObject" /> is a prefab.</summary>
		public static bool IsPrefab(this GameObject go)
		{
			return go.scene.name == null;
		}

		/// <summary>
		///     <para>Returns the bounds of all <see cref="Renderer" />s attached in the <see cref="GameObject" />'s hierarchy.</para>
		/// </summary>
		public static Bounds GetBounds(this GameObject gameObject)
		{
			var renderers = gameObject.GetComponentsInChildren<Renderer>();
			if (renderers.Length <= 0)
				return new Bounds { center = gameObject.transform.position };

			Bounds result = renderers[0].bounds;
			for (int i = 1; i < renderers.Length; i++)
				result.Encapsulate(renderers[i].bounds);
			return result;
		}
	}
}