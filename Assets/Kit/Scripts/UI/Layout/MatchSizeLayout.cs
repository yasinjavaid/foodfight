using UnityEngine;

namespace Kit.UI.Layout
{
	/// <summary>Give a UI element the size of this <see cref="rectTransform" /> whenever it's resized.</summary>
	public class MatchSizeLayout: MonoBehaviour
	{
		/// <summary>Transform to resize.</summary>
		[Tooltip("Transform to resize.")]
		public RectTransform Element;

		/// <summary>Padding to apply before resizing.</summary>
		[Tooltip("Padding to apply before resizing.")]
		public Vector2 Padding;

		/// <summary>Whether to resize width.</summary>
		[Tooltip("Whether to resize width.")]
		public bool Width = true;

		/// <summary>Whether to resize height.</summary>
		[Tooltip("Whether to resize height.")]
		public bool Height = true;

		protected RectTransform rectTransform;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		protected void OnRectTransformDimensionsChange()
		{
			if (!Element || !rectTransform)
				return;

			Vector2 newSize = Element.sizeDelta;
			if (Width)
				newSize.x = rectTransform.sizeDelta.x + Padding.x;
			if (Height)
				newSize.y = rectTransform.sizeDelta.y + Padding.y;
			Element.sizeDelta = newSize;
		}
	}
}