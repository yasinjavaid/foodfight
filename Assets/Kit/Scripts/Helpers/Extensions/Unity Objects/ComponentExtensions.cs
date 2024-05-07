using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kit
{
	/// <summary><see cref="Component" /> extensions.</summary>
	public static class ComponentExtensions
	{
		/// <summary>Returns whether the <see cref="Component" /> is a part of a prefab.</summary>
		public static bool IsPrefab(this Component component)
		{
			return component.gameObject.IsPrefab();
		}

		/// <summary>
		///     <para>Returns the bounds of the <see cref="Component" />.</para>
		/// </summary>
		/// <remarks>
		///     Works directly for <see cref="Renderer" />s and <see cref="Collider" />s, otherwise returns the
		///     <see cref="GameObjectExtensions.GetBounds">bounds of the GameObject</see>.
		/// </remarks>
		public static Bounds GetBounds(this Component component)
		{
			switch (component)
			{
				case Transform t:
					return t.GetBounds();

				case Renderer r:
					return r.bounds;

				case Collider c:
					return c.bounds;

				default:
					return component.transform.GetBounds();
			}
		}

		/// <summary>
		///		Duplicates an existing <see cref="Component"/> and its properties to another <see cref="GameObject" />.
		/// </summary>
		public static T CopyTo<T>(this T original, GameObject destination) where T : Component
		{
			System.Type type = original.GetType();
			T dst = destination.GetComponent(type) as T;
			if (!dst) dst = destination.AddComponent(type) as T;

			var fields = type.GetFields();
			foreach (FieldInfo field in fields.Where(field => !field.IsStatic))
				field.SetValue(dst, field.GetValue(original));

			var props = type.GetProperties();
			foreach (PropertyInfo prop in props.Where(prop => prop.CanWrite && prop.CanWrite && prop.Name != "name"))
				prop.SetValue(dst, prop.GetValue(original, null), null);
			return dst;
		}
	}
}