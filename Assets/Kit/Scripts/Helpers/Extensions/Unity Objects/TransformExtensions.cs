﻿using UnityEngine;

namespace Kit
{
	/// <summary><see cref="Transform" /> extensions.</summary>
	public static class TransformExtensions
	{
		/// <summary>Returns the <see cref="GameObjectExtensions.GetBounds">bounds of the transform's GameObject</see>.</summary>
		public static Bounds GetBounds(this Transform transform)
		{
			return transform.gameObject.GetBounds();
		}

		/// <summary>Returns whether the <see cref="Transform" /> is the first one on its parent.</summary>
		public static bool IsFirstSibling(this Transform transform)
		{
			return transform.GetSiblingIndex() == 0;
		}

		/// <summary>Returns whether the <see cref="Transform" /> is the last one on its parent.</summary>
		public static bool IsLastSibling(this Transform transform)
		{
			return transform.GetSiblingIndex() == transform.parent.childCount - 1;
		}

		/// <summary>Returns the first child of the <see cref="Transform" />.</summary>
		public static Transform GetFirstChild(this Transform transform)
		{
			return transform.GetChild(0);
		}

		/// <summary>Returns the last child of the <see cref="Transform" />.</summary>
		public static Transform GetLastChild(this Transform transform)
		{
			return transform.GetChild(transform.childCount - 1);
		}

		/// <summary>Face another <see cref="Transform" />.</summary>
		public static void Face(this Transform transform, Transform other)
		{
			Face(transform, other.position);
		}

		/// <summary>Face a position.</summary>
		public static void Face(this Transform transform, Vector3 position)
		{
			transform.rotation = Quaternion.LookRotation(position - transform.position);
		}
	}
}