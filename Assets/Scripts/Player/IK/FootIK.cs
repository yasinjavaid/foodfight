using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
	public bool Enabled = true;

	[ShowIfGroup("Enabled")]
	[FoldoutGroup("Enabled/Raycast")]
	[LabelText("Layers")]
	public LayerMask RaycastLayers = 1;

	[FoldoutGroup("Enabled/Raycast")]
	[LabelText("Distance")]
	public float RaycastDistance = 0.5f;

	[FoldoutGroup("Enabled/Raycast")]
	[LabelText("Threshold")]
	[Range(-1, 1)]
	public float RaycastThreshold = 0.95f;

	[FoldoutGroup("Enabled/IK")]
	[TitleGroup("Enabled/IK/Left Foot")]
	[LabelText("Position Weight")]
	[Range(0, 1)]
	public float LeftPositionWeight = 0.5f;

	[FoldoutGroup("Enabled/IK")]
	[TitleGroup("Enabled/IK/Left Foot")]
	[LabelText("Rotation Weight")]
	[Range(0, 1)]
	public float LeftRotationWeight = 0.5f;

	[FoldoutGroup("Enabled/IK")]
	[TitleGroup("Enabled/IK/Right Foot")]
	[LabelText("Position Weight")]
	[Range(0, 1)]
	public float RightPositionWeight = 0.5f;

	[FoldoutGroup("Enabled/IK")]
	[TitleGroup("Enabled/IK/Right Foot")]
	[LabelText("Rotation Weight")]
	[Range(0, 1)]
	public float RightRotationWeight = 0.5f;

#if CLIENT_BUILD
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void OnAnimatorIK(int layerIndex)
	{
		SetFoot(AvatarIKGoal.LeftFoot,  LeftPositionWeight,  LeftRotationWeight);
		SetFoot(AvatarIKGoal.RightFoot, RightPositionWeight, RightRotationWeight);
	}

	private void SetFoot(AvatarIKGoal foot, float positionWeight, float rotationWeight)
	{
		if (Enabled)
		{
			animator.SetIKPositionWeight(foot, positionWeight);
			animator.SetIKRotationWeight(foot, rotationWeight);

			Vector3 startPosition = animator.GetIKPosition(foot);
			if (Physics.Raycast(startPosition, Vector3.down, out RaycastHit hit, RaycastDistance, RaycastLayers))
			{
				// Don't apply any modifications if directly moving on a flat surface
				if (Vector3.Dot(hit.normal, Vector3.up) < RaycastThreshold)
				{
					animator.SetIKPosition(foot, hit.point);
					Vector3 cross = Vector3.Cross(hit.normal, animator.GetIKRotation(foot) * Vector3.left);
					Quaternion newRotation = Quaternion.LookRotation(cross, transform.up);
					animator.SetIKRotation(foot, newRotation);
				}
#if UNITY_EDITOR
				Debug.DrawLine(startPosition, hit.point, Color.blue);
#endif
			}
#if UNITY_EDITOR
			else
				Debug.DrawLine(startPosition, startPosition + Vector3.down * RaycastDistance, Color.red);
#endif
		}
		else
		{
			animator.SetIKPositionWeight(foot, 0);
			animator.SetIKRotationWeight(foot, 0);
		}
	}
#endif
}