#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Kit.Containers.Editor
{
	public class AsyncReactivePropertyProcessor<T>: OdinAttributeProcessor<AsyncReactiveProperty<T>> where T: struct
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}

	public class AsyncReactivePropertyDrawer<T>: OdinValueDrawer<AsyncReactiveProperty<T>> where T: struct
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			var property = ValueEntry.SmartValue;
			property.Value = SirenixEditorGUI.DynamicPrimitiveField(label, property.Value);
		}
	}

	public class ReadOnlyAsyncReactivePropertyProcessor<T>: OdinAttributeProcessor<ReadOnlyAsyncReactiveProperty<T>> where T: struct
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}

	public class ReadOnlyAsyncReactivePropertyDrawer<T>: OdinValueDrawer<ReadOnlyAsyncReactiveProperty<T>> where T: struct
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			GUIHelper.PushGUIEnabled(false);
			SirenixEditorGUI.DynamicPrimitiveField(label, ValueEntry.SmartValue.Value);
			GUIHelper.PopGUIEnabled();
		}
	}

	public class AsyncReactiveListProcessor<T>: OdinAttributeProcessor<AsyncReactiveList<T>> where T: IUpgrade
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}

	public class AsyncReactiveListDrawer<T>: OdinValueDrawer<AsyncReactiveList<T>> where T: IUpgrade
	{
		protected override void Initialize()
		{
			base.Initialize();
			if (ValueEntry.SmartValue == null)
				Property.Tree.DelayActionUntilRepaint(() => ValueEntry.SmartValue = new AsyncReactiveList<T>());
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			CallNextDrawer(label);
		}
	}
}
#endif