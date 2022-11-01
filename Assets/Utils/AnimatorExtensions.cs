using UnityEngine;

public static class AnimatorExtensions
{
    public static string GetCurrentAnimationName(this Animator animator)
    {
        return animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
    }
}