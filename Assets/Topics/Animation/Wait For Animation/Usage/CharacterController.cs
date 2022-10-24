using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Standing = Animator.StringToHash("Standing");

    private async void Start()
    {
        _animator.SetTrigger(Standing);
        await WaitForAnimation.WaitForComplete(_animator, null, null);
        Debug.Log("Finish Standing!");
        _animator.SetTrigger(Walking);
    }
}