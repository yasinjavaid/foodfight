using UnityEditor;
using UnityEngine.UI;

namespace Kit.UI.Behaviours
{
#if UNITY_EDITOR
	[CustomEditor(typeof(RaycastGraphic), false)]
	public class RaycastGraphicEditor: Editor
	{
		public override void OnInspectorGUI()
		{
			// Overriding the default inspector to show nothing
		}
	}
#endif

	/// <summary>
	///     A sub-class of the Unity UI <see cref="Graphic" /> that just skips drawing. Useful for providing a raycast target without actually
	///     drawing anything.
	/// </summary>
	public class RaycastGraphic: Graphic
	{
		public override void SetMaterialDirty()
		{
			// Overriding SetMaterialDirty to perform skip flagging materials
		}

		public override void SetVerticesDirty()
		{
			// Overriding SetMaterialDirty to perform skip flagging vertices
		}

		/// Probably not necessary since the chain of calls Rebuild()->UpdateGeometry()->DoMeshGeneration()->OnPopulateMesh() won't happen,
		/// but here really just as a fail-safe.
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			vertexHelper.Clear();
		}
	}
}