using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides functions to temporarily slow down time effects.
/// TODO: potentially need to store/check coroutines to prevent multiple coroutines being called???
/// </summary>


public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
        
    [SerializeField] [Range(0, 1)] private float slowdownFactor = 0.03f;
    [SerializeField] [Range(0, 1)] private float defaultSlowdownLength = 0.04f;
    private float slowdownLength;
    private Coroutine slowtimeCoroutine;

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
    
    private IEnumerator SlowTimeCoroutine()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        yield return new WaitForSecondsRealtime(slowdownLength);

        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;

        slowtimeCoroutine = null;
    }
    
    private void SlowTime()
    {
        if (slowtimeCoroutine == null)
        {
            slowtimeCoroutine = StartCoroutine(SlowTimeCoroutine());
        }
        else
        {
            Time.timeScale = slowdownFactor;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
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
    
    /// slows time by `slowdownFactor` for default length after `delay` seconds
    public void DoSlowMotionWithDelay(float delay)
    {
        StartCoroutine(DelaySlowMotionCoroutine(delay, defaultSlowdownLength));
    }
    
    /// slows time by `slowdownFactor` for specified length after specified delay
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