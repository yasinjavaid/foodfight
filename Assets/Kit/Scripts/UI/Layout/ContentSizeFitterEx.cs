﻿using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;

#endif

namespace Kit.UI.Layout
{
#if UNITY_EDITOR
	[CustomEditor(typeof(ContentSizeFitterEx), false)]
	public class ContentSizeFitterExEditor: OdinEditor
	{
	}
#endif

	/// <summary>A <see cref="ContentSizeFitter" /> that allows you to add padding.</summary>
	public class ContentSizeFitterEx: ContentSizeFitter
	{
		/// <summary>Padding to apply around the fitter.</summary>
		[Tooltip("Padding to apply around the fitter.")]
		public Vector2 Padding;

		protected RectTransform cachedRectTransform;

		public override void SetLayoutHorizontal()
		{
			base.SetLayoutVertical();
			RectTransform.sizeDelta = RectTransform.sizeDelta.AddY(Padding.x);
		}

		public override void SetLayoutVertical()
		{
			base.SetLayoutVertical();
			RectTransform.sizeDelta = RectTransform.sizeDelta.AddY(Padding.y);
		}

		protected RectTransform RectTransform
		{
			get
			{
				if (cachedRectTransform == null)
					cachedRectTransform = GetComponent<RectTransform>();
				return cachedRectTransform;
			}
		}
	}
}