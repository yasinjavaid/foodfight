using Sirenix.OdinInspector;
using UnityEngine;

namespace Kit.Behaviours
{
	/// <summary>Follow a <see cref="Transform" /> while keeping a certain distance.</summary>
	public class FollowTransform: MonoBehaviour
	{
		/// <summary>The target <see cref="Transform" /> to follow.</summary>
		[Tooltip("The target transform to follow.")]
		public Transform Target;

		/// <summary>The distance to keep while following.</summary>
		[Tooltip("The distance to keep while following.")]
		public float Distance = 10.0f;

		/// <summary>The speed at which to follow.</summary>
		[Tooltip("The speed at which to follow.")]
		public float MoveSpeed = 10.0f;

		/// <summary>Should it keep facing the target?</summary>
		[Tooltip("Should it keep facing the target?")]
		public bool Face = true;

		/// <summary>The speed at which to face.</summary>
		[ShowIf(nameof(Face))]
		[Tooltip("The speed at which to face.")]
		public float RotateSpeed = 5.0f;

		protected new Transform transform;

		private void Awake()
		{
			transform = base.transform;
		}

		/// <summary>Start following again if stopped.</summary>
		public void Follow()
		{
			enabled = true;
		}

		/// <summary>Assigns <see cref="Distance"/> depending on the current distance between the camera and the target.</summary>
		public void RecalculateDistance()
		{
			if (Target == null)
				return;

			Distance = (Target.position - transform.position).magnitude;
		}

		/// <summary>Follow a different <see cref="Transform" /> using the current distance between them.</summary>
		public void Follow(Transform target)
		{
			Target = target;
			enabled = target != null;
		}

		/// <summary>Follow a different <see cref="Transform" /> at a specified distance.</summary>
		public void Follow(Transform target, float distance)
		{
			Distance = distance;
			Follow(target);
		}

		/// <summary>Stop following.</summary>
		public void Stop()
		{
			enabled = false;
		}

		private void LateUpdate()
		{
			if (Target == null)
			{
				Stop();
				return;
			}

			Vector3 targetPosition = Target.position;
			Vector3 newPosition = targetPosition - transform.forward * Distance;
			transform.position = Vector3.Lerp(transform.position, newPosition, MoveSpeed * Time.deltaTime);
			if (Face)
			{
				Quaternion newRotation = Quaternion.LookRotation(targetPosition - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, RotateSpeed * Time.deltaTime);
			}
		}
	}
}