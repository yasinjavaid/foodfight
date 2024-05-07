using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class AdjustLayerWeights : StateMachineBehaviour
{
    public float[] Weights;

    [Min(0)]
    [SuffixLabel("seconds")]
    public float Time = 0.0f;

    private float[] oldWeights;

    private void Awake()
    {
        oldWeights = new float[Weights.Length];
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < oldWeights.Length; i++)
            oldWeights[i] = animator.GetLayerWeight(i);

        for (int i = 0; i < Weights.Length; i++)
            SetLayerWeight(animator, i, Weights[i]);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < oldWeights.Length; i++)
            SetLayerWeight(animator, i, oldWeights[i]);
    }

    private void SetLayerWeight(Animator animator, int index, float weight)
    {
        if (Time > 0.0f)
            DOTween.To(set => animator.SetLayerWeight(index, set), animator.GetLayerWeight(index), weight, Time);
        else
            animator.SetLayerWeight(index, weight);
    }
}
