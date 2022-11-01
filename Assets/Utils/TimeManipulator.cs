using UnityEngine;

public class TimeManipulator : MonoBehaviour
{
    [SerializeField] private float _desireTimeScale;

    private float _currentTimeScale;

    private void Start()
    {
        _currentTimeScale = Time.timeScale;
    }

    private void Update()
    {
        if (_desireTimeScale != _currentTimeScale)
        {
            Time.timeScale = _desireTimeScale;
            _currentTimeScale = _desireTimeScale;
        }
    }
}