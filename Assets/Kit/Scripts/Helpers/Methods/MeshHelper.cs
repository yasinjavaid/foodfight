using UnityEngine;

namespace Kit
{
	/// <summary>Helper functions for mesh operations.</summary>
	public static class MeshHelper
	{
		/// <summary>Generates a new <see cref="Mesh" /> with four vertices.</summary>
		/// <param name="size">Size of the mesh.</param>
		/// <param name="pivot">Center of the mesh.</param>
		public static Mesh GenerateQuad(float size, Vector2 pivot)
		{
			Vector3[] vertices =
			{
				new Vector3(size - pivot.x, size - pivot.y, 0),
				new Vector3(size - pivot.x, 0    - pivot.y, 0),
				new Vector3(0    - pivot.x, 0    - pivot.y, 0),
				new Vector3(0    - pivot.x, size - pivot.y, 0)
			};

			Vector2[] uv =
			{
				new Vector2(1, 1),
				new Vector2(1, 0),
				new Vector2(0, 0),
				new Vector2(0, 1)
			};

			int[] triangles =
			{
				0, 1, 2,
				2, 3, 0
			};

			return new Mesh
				   {
					   vertices = vertices,
					   uv = uv,
					   triangles = triangles
				   };
		}
	}
}