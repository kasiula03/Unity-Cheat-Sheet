using System;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private static readonly string Walking = "Walking";
    private static readonly string WalkingBlendTree = "WalkingBlendTree";
    private static readonly string Standing = "Standing";

    private readonly CancellationTokenProvider _cancellationTokenProvider = new CancellationTokenProvider();

    private string _currentWalking = Walking;

    private void Start()
    {
        StandAndWalk();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_currentWalking == Walking)
            {
                _currentWalking = WalkingBlendTree;
            }
            else
            {
                _currentWalking = Walking;
            }
            StandAndWalk();
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenProvider.Cancel();
    }

    private async void StandAndWalk()
    {
        _cancellationTokenProvider.Cancel();

        bool completed = await WaitForAnimation.TriggerAndWaitForCompletion(_animator,
            _cancellationTokenProvider.GetCancellationToken(), Standing);
        if (completed)
        {
            Debug.Log("Finish Standing!");
        }
        else
        {
            Debug.Log("Stand interrupted");
        }

        if (completed)
        {
            string walkingParameter = _currentWalking;
            bool result = await WaitForAnimation.TriggerAndWaitForTransition(_animator,
                _cancellationTokenProvider.GetCancellationToken(), walkingParameter);
            if (!result)
            {
                Debug.Log(walkingParameter);
                _animator.ResetTrigger(walkingParameter);
            }
            Debug.Log("Start Walking");
        }
    }


    private void Walk()
    {
        _cancellationTokenProvider.Cancel();
        _animator.SetTrigger(Walking);
    }
}