using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Items.Weapons
{
	public class PepperGrinder: Weapon
	{
		[FoldoutGroup("Positioning")]
		public List<Transform> PelletPositions;

		protected override void FireProjectile(Vector3 position, Quaternion rotation)
		{
			foreach (Transform pelletTransform in PelletPositions)
				base.FireProjectile(pelletTransform.position, rotation * pelletTransform.localRotation);
		}
	}
}