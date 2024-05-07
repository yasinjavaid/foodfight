using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
	public class PlayerState
	{
		[ShowInInspector]
		public virtual Vector3 Movement { get; set; } = Vector3.zero;

		[ShowInInspector]
		public virtual bool IsGrounded { get; set; } = false;

		[ShowInInspector]
		public virtual bool IsSprinting { get; set; } = false;

		[ShowInInspector]
		public virtual bool IsAiming { get; set; } = false;

		[ShowInInspector]
		public virtual Vector3 AimLocation { get; set; }

		[ShowInInspector]
		public virtual float Health { get; set; } = 0;

		[ShowInInspector]
		public virtual int Ammo { get; set; } = 0;
	}
}