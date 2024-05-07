using Sirenix.OdinInspector;
using UnityEngine;

public class HandIK : MonoBehaviour
{
	public bool Enabled = true;

	[ShowIf("Enabled")]
	public AvatarIKGoal[] HandBones = { AvatarIKGoal.RightHand };

	[ShowIf("Enabled")]
	[Range(0, 1)]
	public float HandPositionWeight = 1.0f;

	[ShowIf("Enabled")]
	[Range(0, 1)]
	public float HandRotationWeight = 1.0f;

	[ShowIf("Enabled")]
	public Vector3 HandPosition;

	[ShowIf("Enabled")]
	public Quaternion HandRotation;

#if CLIENT_BUILD
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (Enabled)
		{
			foreach (AvatarIKGoal bone in HandBones)
			{
				animator.SetIKPositionWeight(bone, HandPositionWeight);
				animator.SetIKRotationWeight(bone, HandRotationWeight);
				animator.SetIKPosition(bone, HandPosition);
				animator.SetIKRotation(bone, HandRotation);
			}
		}
		else
		{
			foreach (AvatarIKGoal bone in HandBones)
			{
				animator.SetIKPositionWeight(bone, 0);
				animator.SetIKRotationWeight(bone, 0);
			}
		}
	}
#endif
}