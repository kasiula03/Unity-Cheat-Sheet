using System;
using System.Threading.Tasks;
using UnityEngine;

public class WaitForAnimation : MonoBehaviour
{
    private Animator _animator;
    private Action _onTransitionComplete;
    private Action _onAnimationComplete;
    private bool _framePassed;

    public static async Task WaitForComplete(Animator animator, Action onTransitionComplete, Action onAnimationComplete)
    {
        GameObject gameObject = new GameObject
        {
            hideFlags = HideFlags.HideInHierarchy
        };

        WaitForAnimation animation = gameObject.AddComponent<WaitForAnimation>();
        animation.Initialize(animator, onTransitionComplete, onAnimationComplete);

        //Wait for transition to begin
        await animation.NextFrame();
        await animation.WaitForTransition();
        await animation.WaitForCompleteAnimation();
        DestroyImmediate(gameObject);
    }

    private void Initialize(Animator animator, Action onTransitionComplete, Action onAnimationComplete)
    {
        _animator = animator;
        _onTransitionComplete = onTransitionComplete;
        _onAnimationComplete = onAnimationComplete;
    }

    private async Task NextFrame()
    {
        while (!_framePassed)
        {
            await Task.Delay(50);
        }
    }

    private void Update()
    {
        _framePassed = true;
    }

    private async Task WaitForTransition()
    {
        AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);
        float duration = transitionInfo.duration;
        float durationLeft = (1 - transitionInfo.normalizedTime) * duration;
        await Task.Delay(TimeSpan.FromSeconds(durationLeft));
        _onTransitionComplete?.Invoke();
    }

    private async Task WaitForCompleteAnimation()
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        float currentProgress = state.normalizedTime;
        float animationLength = state.length;
        float timeLeft = (1 - currentProgress) * animationLength;
        await Task.Delay(TimeSpan.FromSeconds(timeLeft));
        _onAnimationComplete?.Invoke();
    }
}