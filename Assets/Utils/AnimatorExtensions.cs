using UnityEditor.Animations;
using UnityEngine;

public static class AnimatorExtensions
{
    public static AnimatorState GetAnimatorStateForClip(this Animator animator, AnimationClip clip, int layer = 0)
    {
        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
        if (animatorController == null)
        {
            return null;
        }
        AnimatorStateMachine stateMachine = animatorController.layers[layer].stateMachine;
        return FindAnimatorState(stateMachine, clip);
    }

    public static AnimatorState FindAnimatorState(AnimatorStateMachine stateMachine, AnimationClip clip)
    {
        foreach (ChildAnimatorState childAnimatorState in stateMachine.states)
        {
            Motion motion = childAnimatorState.state.motion;
            if (motion is BlendTree bt)
            {
                foreach (ChildMotion btChild in bt.children)
                {
                    if (btChild.motion is AnimationClip ac)
                    {
                        if (ac.GetInstanceID() == clip.GetInstanceID())
                        {
                            return childAnimatorState.state;
                        }
                    }
                }
            }
            else if (motion is AnimationClip ac)
            {
                if (ac.GetInstanceID() == clip.GetInstanceID())
                {
                    return childAnimatorState.state;
                }
            }
        }
        return null;
    }
}