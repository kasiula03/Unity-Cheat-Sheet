using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WaitForAnimation
{
    private Animator _animator;
    private Action _onTransitionComplete;
    private Action _onAnimationComplete;


    public static async Task<bool> TriggerAndWaitForCompletion(Animator animator, CancellationToken cancellationToken,
        string triggerParameter, Action onTransitionComplete = null, Action onAnimationComplete = null)
    {
        animator.SetTrigger(triggerParameter);
        return await WaitForComplete(animator, cancellationToken, onTransitionComplete, onAnimationComplete);
    }

    public static async Task<bool> TriggerAndWaitForTransition(Animator animator, CancellationToken cancellationToken,
        string triggerParameter, Action onTransitionComplete = null)
    {
        animator.SetTrigger(triggerParameter);
        await WaitForTransitionComplete(animator, cancellationToken, onTransitionComplete);
        return !cancellationToken.IsCancellationRequested;
    }

    public static async Task<bool> WaitForComplete(Animator animator, CancellationToken cancellationToken,
        Action onTransitionComplete, Action onAnimationComplete)
    {
        WaitForAnimation animation = new WaitForAnimation();
        animation.Initialize(animator, onTransitionComplete, onAnimationComplete);

        await animation.WaitForOldTransition(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        await animation.WaitForTransition(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }


        bool result = await animation.WaitForCompleteAnimation(cancellationToken);
        return result;
    }

    public static async Task WaitForTransitionComplete(Animator animator, CancellationToken cancellationToken,
        Action onTransitionComplete)
    {
        GameObject gameObject = new GameObject
        {
            hideFlags = HideFlags.HideInHierarchy
        };

        WaitForAnimation animation = new WaitForAnimation();
        animation.Initialize(animator, onTransitionComplete, null);

        await animation.WaitForOldTransition(cancellationToken);
        await animation.WaitForTransition(cancellationToken);
    }

    private void Initialize(Animator animator, Action onTransitionComplete, Action onAnimationComplete)
    {
        _animator = animator;
        _onTransitionComplete = onTransitionComplete;
        _onAnimationComplete = onAnimationComplete;
    }

    private async Task WaitForOldTransition(CancellationToken cancellationToken)
    {
        if (_animator.IsInTransition(0))
        {
            Debug.Log("Wait for old transition to finished");
            float oldTransitionDurationLeft = GetCurrentTransitionRemainingTime();
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(oldTransitionDurationLeft / Time.timeScale), cancellationToken);
            }
            catch (TaskCanceledException _)
            {
                return;
            }
        }
    }


    private async Task<bool> WaitForTransition(CancellationToken cancellationToken)
    {
        if (!_animator.IsInTransition(0))
        {
            Debug.Log("Wait for transition");
            while (!_animator.IsInTransition(0))
            {
                try
                {
                    await Task.Delay(50, cancellationToken);
                }
                catch (TaskCanceledException _)
                {
                    return false;
                }
            }
        }

        Debug.Log("Transition Started");
        float durationLeft = GetCurrentTransitionRemainingTime();
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(durationLeft / Time.timeScale), cancellationToken);
        }
        catch (TaskCanceledException _)
        {
            return false;
        }
        Debug.Log("Transition Finished");
        _onTransitionComplete?.Invoke();
        return true;
    }

    private float GetCurrentTransitionRemainingTime()
    {
        AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);
        float duration = transitionInfo.duration;
        float durationLeft = (1 - transitionInfo.normalizedTime) * duration;
        return durationLeft;
    }

    private async Task<bool> WaitForCompleteAnimation(CancellationToken cancellationToken)
    {
        string animationName = _animator.GetCurrentAnimationName();
        Debug.Log($"Wait for animation {animationName}");

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        float currentProgress = state.normalizedTime;

        float animationLength = state.length;
        float timeLeft = (1 - currentProgress) * animationLength;
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(timeLeft / Time.timeScale), cancellationToken);
        }
        catch (TaskCanceledException _)
        {
            return false;
        }

        _onAnimationComplete?.Invoke();
        return true;
    }
}