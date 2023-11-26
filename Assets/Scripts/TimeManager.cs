using System.Collections;
using UnityEngine;

/// <summary>
/// Provides functions to temporarily slow down time effects.
/// TODO: potentially need to store/check coroutines to prevent multiple coroutines being called???
/// </summary>


public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
        
    [SerializeField] private float slowdownFactor = 0.03f;
    [SerializeField] private float defaultSlowdownLength = 0.05f;
    public float slowdownLength = 0.05f;

    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        Time.timeScale += (1 / slowdownLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0, 1);
    }

    private void SlowTime()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
    
    // slows time by `slowdownFactor` for default length
    public void DoSlowMotion()
    {
        slowdownLength = defaultSlowdownLength;
        SlowTime();
    }

    // slows time by `slowdownFactor` for specified length
    public void DoSlowMotion(float duration)
    {
        slowdownLength = duration;
        SlowTime();
    }
    
    // slows time by `slowdownFactor` for default length after specified delay
    public void DoSlowMotionWithDelay(float delay)
    {
        StartCoroutine(DelaySlowMotionCoroutine(delay, defaultSlowdownLength));
    }
    
    // slows time by `slowdownFactor` for specified length after specified delay
    public void DoSlowMotionWithDelay(float delay, float duration)
    {
        StartCoroutine(DelaySlowMotionCoroutine(delay, duration));
    }

    private IEnumerator DelaySlowMotionCoroutine(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        DoSlowMotion(duration);
    }
}