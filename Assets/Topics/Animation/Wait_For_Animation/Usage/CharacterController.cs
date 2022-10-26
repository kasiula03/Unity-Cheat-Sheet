using System;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Standing = Animator.StringToHash("Standing");

    private readonly CancellationTokenProvider _cancellationTokenProvider = new CancellationTokenProvider();

    private void Start()
    {
        StandAndWalk();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StandAndWalk();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Walk();
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenProvider.Cancel();
    }

    private async void StandAndWalk()
    {
        _cancellationTokenProvider.Cancel();
        _animator.SetTrigger(Standing);
        bool completed = await WaitForAnimation.WaitForComplete(_animator,
            _cancellationTokenProvider.GetCancellationToken(), null,
            null);
        Debug.Log("Finish Standing!");
        if (completed)
        {
            _animator.SetTrigger(Walking);
        }
    }

    private void Walk()
    {
        _cancellationTokenProvider.Cancel();
        _animator.SetTrigger(Walking);
    }
}