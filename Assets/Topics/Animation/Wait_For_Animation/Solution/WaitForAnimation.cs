using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;

public class WaitForAnimation : MonoBehaviour
{
    private Animator _animator;
    private Action _onTransitionComplete;
    private Action _onAnimationComplete;

    private bool _framePassed;

    public static async Task<bool> TriggerAndWaitForCompletion(Animator animator, CancellationToken cancellationToken,
        string triggerParameter, Action onTransitionComplete = null, Action onAnimationComplete = null)
    {
        animator.SetTrigger(triggerParameter);
        return await WaitForComplete(animator, cancellationToken, onTransitionComplete, onAnimationComplete,
            triggerParameter);
    }

    public static async Task<bool> TriggerAndWaitForTransition(Animator animator, CancellationToken cancellationToken,
        string triggerParameter, Action onTransitionComplete = null)
    {
        animator.SetTrigger(triggerParameter);
        await WaitForTransitionComplete(animator, cancellationToken, onTransitionComplete, triggerParameter);
        return !cancellationToken.IsCancellationRequested;
    }

    public static async Task<bool> WaitForComplete(Animator animator, CancellationToken cancellationToken,
        Action onTransitionComplete, Action onAnimationComplete, string parameter)
    {
        GameObject gameObject = new GameObject
        {
            hideFlags = HideFlags.HideInHierarchy
        };

        WaitForAnimation animation = gameObject.AddComponent<WaitForAnimation>();

        animation.Initialize(animator, onTransitionComplete, onAnimationComplete);

        //Wait for transition to begin
        await animation.NextFrame();

        await animation.WaitForTransition(cancellationToken, parameter);
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
        Action onTransitionComplete, string parameter)
    {
        GameObject gameObject = new GameObject
        {
            hideFlags = HideFlags.HideInHierarchy
        };

        WaitForAnimation animation = gameObject.AddComponent<WaitForAnimation>();
        animation.Initialize(animator, onTransitionComplete, null);

        //Wait for transition to begin
        await animation.NextFrame();
        await animation.WaitForTransition(cancellationToken, parameter);
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

    private async Task<bool> WaitForTransition(CancellationToken cancellationToken, string parameter)
    {
        if (!_animator.IsInTransition(0))
        {
            AnimationClip currentClip = _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            AnimatorState currentClipState =
                _animator.GetAnimatorStateForClip(currentClip);

            AnimatorStateTransition transition = FindTransition(currentClipState, parameter);

            if (transition == null)
            {
                transition =
                    FindAnyStateTransition(_animator.runtimeAnimatorController as AnimatorController, parameter);
            }

            if (transition != null && transition.hasExitTime)
            {
                AnimatorStateInfo animationState = _animator.GetCurrentAnimatorStateInfo(0);

                float remainingExitTime = CalculateRemainingTime(animationState, transition);
                Debug.Log("Wait for Exit Time");
                try
                {
                    //TODO: Is fixed duration
                    await Task.Delay(TimeSpan.FromSeconds(remainingExitTime * animationState.length),
                        cancellationToken);
                    Debug.Log("Exit time finished");
                }
                catch (TaskCanceledException _)
                {
                    return false;
                }
            }
        }

        AnimatorTransitionInfo transitionInfo = _animator.GetAnimatorTransitionInfo(0);

        if (transitionInfo.normalizedTime < 1)
        {
            float duration = transitionInfo.duration;
            float durationLeft = (1 - transitionInfo.normalizedTime) * duration;
            try
            {
                Debug.Log("Wait for transition");
                await Task.Delay(TimeSpan.FromSeconds(durationLeft), cancellationToken);
                Debug.Log("Transition finished");
            }
            catch (TaskCanceledException _)
            {
                return false;
            }
        }

        _onTransitionComplete?.Invoke();
        return true;
    }

    private static float CalculateRemainingTime(AnimatorStateInfo animationState, AnimatorStateTransition transition)
    {
        float currentTime = animationState.normalizedTime;

        float currentStep = currentTime - Mathf.FloorToInt(currentTime);
        float differentToTransition = 0;
        if (currentStep > transition.exitTime)
        {
            differentToTransition = (1 - currentStep) + transition.exitTime;
        }
        else
        {
            differentToTransition = transition.exitTime - currentStep;
        }
        return differentToTransition;
    }

    private AnimatorStateTransition FindAnyStateTransition(AnimatorController animatorController, string parameter)
    {
        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
        {
            foreach (AnimatorCondition animatorCondition in transition.conditions)
            {
                if (animatorCondition.parameter == parameter)
                {
                    return transition;
                }
            }
        }
        return null;
    }

    private AnimatorStateTransition FindTransition(AnimatorState currentClipState, string parameter)
    {
        foreach (AnimatorStateTransition transition in currentClipState.transitions)
        {
            foreach (AnimatorCondition animatorCondition in transition.conditions)
            {
                if (animatorCondition.parameter == parameter)
                {
                    return transition;
                }
            }
        }

        return null;
    }

    private async Task<bool> WaitForCompleteAnimation(CancellationToken cancellationToken)
    {
        string name = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        float currentProgress = state.normalizedTime;
        float animationLength = state.length;
        if (currentProgress < 1)
        {
            float timeLeft = (1 - currentProgress) * animationLength;
            try
            {
                Debug.Log($"Wait for animation {name}");
                await Task.Delay(TimeSpan.FromSeconds(timeLeft), cancellationToken);
                Debug.Log($"Animation finished {name}");
            }
            catch (TaskCanceledException _)
            {
                return false;
            }
        }
        _onAnimationComplete?.Invoke();
        return true;
    }
}