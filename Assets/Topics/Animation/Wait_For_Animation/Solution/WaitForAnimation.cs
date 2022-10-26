using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WaitForAnimation : MonoBehaviour
{
    private Animator _animator;
    private Action _onTransitionComplete;
    private Action _onAnimationComplete;

    private bool _framePassed;

    public static async Task<bool> WaitForComplete(Animator animator, CancellationToken cancellationToken,
        Action onTransitionComplete, Action onAnimationComplete)
    {
        GameObject gameObject = new GameObject
        {
            hideFlags = HideFlags.HideInHierarchy
        };

        WaitForAnimation animation = gameObject.AddComponent<WaitForAnimation>();

        animation.Initialize(animator, onTransitionComplete, onAnimationComplete);
        
        //Wait for transition to begin
        await animation.NextFrame();
        await animation.WaitForTransition(cancellationToken);
        if (!cancellationToken.IsCancellationRequested)
        {
            bool result = await animation.WaitForCompleteAnimation(cancellationToken);
            DestroyImmediate(gameObject);
            return result;
        }
        else
        {
            DestroyImmediate(gameObject);
            return false;
        }
    }

    public static async Task WaitForTransitionComplete(Animator animator, CancellationToken cancellationToken,
        Action onTransitionComplete)
    {
        GameObject gameObject = new GameObject
        {
            hideFlags = HideFlags.HideInHierarchy
        };

        WaitForAnimation animation = gameObject.AddComponent<WaitForAnimation>();
        animation.Initialize(animator, onTransitionComplete, null);

        //Wait for transition to begin
        await animation.NextFrame();
        await animation.WaitForTransition(cancellationToken);
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

    private async Task<bool> WaitForTransition(CancellationToken cancellationToken)
    {
        AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);
        float duration = transitionInfo.duration;
        float durationLeft = (1 - transitionInfo.normalizedTime) * duration;
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(durationLeft), cancellationToken);
        }
        catch (TaskCanceledException _)
        {
            return false;
        }

        _onTransitionComplete?.Invoke();
        return true;
    }

    private async Task<bool> WaitForCompleteAnimation(CancellationToken cancellationToken)
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        float currentProgress = state.normalizedTime;
        float animationLength = state.length;
        float timeLeft = (1 - currentProgress) * animationLength;
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(timeLeft), cancellationToken);
        }
        catch (TaskCanceledException _)
        {
            return false;
        }
        _onAnimationComplete?.Invoke();
        return true;
    }
}