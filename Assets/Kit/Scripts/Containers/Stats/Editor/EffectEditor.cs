#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;
using XLua.Cast;

namespace Kit.Containers.Editor
{
	public class EffectDrawer: OdinValueDrawer<Effect>
	{
		public const float EffectWidth = 120;

		protected override void DrawPropertyLayout(GUIContent label)
		{
			SirenixEditorGUI.BeginIndentedHorizontal();
			Effect effect = ValueEntry.SmartValue;
			if (effect.GetValue == null)
				effect.AssignSimpleGetValue();
			effect.Stat = SirenixEditorGUI.DynamicPrimitiveField(label, effect.Stat);
			string input = SirenixEditorGUI.DynamicPrimitiveField(null, Effect.Convert(effect), GUILayout.MaxWidth(EffectWidth));
			try
			{
				(effect.Type, effect.Value) = Effect.Parse(input);
			}
			catch
			{
				// Ignore parsing errors
			}

			ValueEntry.SmartValue = effect;
			SirenixEditorGUI.EndIndentedHorizontal();
		}
	}

	public class EffectProcessor: OdinAttributeProcessor<Effect>
	{
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new HideReferenceObjectPickerAttribute());
		}
	}
}
#endif