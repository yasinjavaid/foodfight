using Game;
using Sirenix.OdinInspector;
using UnityEngine;

public class LookIK : MonoBehaviour
{
	public bool Enabled = true;

	[ShowIfGroup("Enabled")]

	[ShowIf("Enabled")]
	[Range(0, 1)]
	public float Weight = 1.0f;

	[ShowIf("Enabled")]
	public float Smoothing = 10.0f;

#if CLIENT_BUILD
	private Animator animator;
	private Player player;

	private Vector3 target;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		player = transform.parent.GetComponent<Player>();
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (Enabled)
		{
			target = Vector3.Lerp(target, LookAtPosition, GameManager.Instance.DeltaTime * Smoothing);
			animator.SetLookAtWeight(Weight);
			animator.SetLookAtPosition(target);
		}
		else
			animator.SetLookAtWeight(0);
	}

	public Vector3 LookAtPosition => player.State.AimLocation;
#endif
}
