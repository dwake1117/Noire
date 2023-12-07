using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Provides functions to temporarily slow down time effects.
/// TODO: potentially need to store/check coroutines to prevent multiple coroutines being called???
/// </summary>


public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
        
    private const float defaultSlowdownFactor = 0.02f;
    private const float defaultSlowdownLength = 0.05f;
    private float slowdownLength;
    private float slowdownFactor;
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

        slowdownLength = defaultSlowdownLength;
        slowdownFactor = defaultSlowdownFactor;
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

    /// slows time by specified factor and length. If not specified, default is used.
    public void DoSlowMotion(float factor=defaultSlowdownFactor, float duration=defaultSlowdownLength)
    {
        slowdownFactor = factor;
        slowdownLength = duration;
        SlowTime();
    }
    
    /// slows time after a specified delay
    public void DoSlowMotionWithDelay(float delay, float factor=defaultSlowdownFactor, float duration=defaultSlowdownLength)
    {
        StartCoroutine(DelaySlowMotionCoroutine(delay, factor, duration));
    }

    private IEnumerator DelaySlowMotionCoroutine(float delay, float factor, float duration)
    {
        yield return new WaitForSeconds(delay);
        DoSlowMotion(factor, duration);
    }
}