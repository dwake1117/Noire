using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// THE UI baseclass. Provides useful functions for transitioning.
/// <remarks>Only use this on parent MonoBehaviours that needs to fade in/out!
/// Otherwise it will come with unnecessary overhead</remarks>
/// 
/// Inheritors have access to
/// <code>Show()</code>
/// Calls Activate() on the object, enables it, (or the container object if alternativeGameObject is toggled on),
/// and begins the Fade transition.
/// 
/// <code>Hide()</code>
/// Calls Deactivate() on the object, disables it, and begins the Fade out transition
/// <code>Init()</code>
/// Obtains the canvasGroup and rectTransform components from the gameObject.
/// 
/// NOTE: Inheritors should selectively implement the following event methods:
/// <code>Activate</code> Called before Show()'s fade transitions
/// <code>Deactivate</code> Called before Hide()'s fade transitions
/// <code>LateActivate</code> Called after Show()'s fade transitions
/// <code>LateDeactivate</code> Called after Hide()'s fade transitions
/// 
/// </summary>

[RequireComponent(typeof(CanvasGroup))]
public class UI : MonoBehaviour
{
    [SerializeField] protected GameObject containerGameObject;
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;
    protected bool alternativeGameObject = false;
    protected float animationTime = .4f;
    
    private Coroutine fadeCoroutine;

    private bool CanAnimate => fadeCoroutine == null;
    
    protected virtual void Activate() { }
    protected virtual void Deactivate() { }
    protected virtual void LateActivate() { }
    protected virtual void LateDeactivate() { }

    protected virtual void Awake()
    {
        Init();
    }

    /// Shows an UI element by fading. If another fade coroutine is going at the same time,
    /// this operation is canceled, and Activate will not be called. 
    public bool Show(bool activate=true)
    {
        if (CanAnimate)
        {
            Display(true);
            if(activate)
                Activate();
            fadeCoroutine = StartCoroutine(Fade(0, 1));
            return true;
        }

        return false;
    }

    /// Shows an UI element by fading. Stops and overwrites any concurrent coroutines.
    public void ForceShow(bool activate=true)
    {
        if(fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        Display(true);
        if(activate)
            Activate();
        fadeCoroutine = StartCoroutine(Fade(0, 1));
    }

    /// Hides an UI element by fading. If another fade coroutine is going at the same time,
    /// this operation is canceled, and Deactivate will not be called.
    public bool Hide(bool deactivate=true)
    {
        if (CanAnimate && gameObject.activeSelf)
        {
            if(deactivate)
                Deactivate();
            fadeCoroutine = StartCoroutine(Fade(1, 0));
            return true;
        }

        return false;
    }

    /// Hides an UI element by fading. Stops and overwrites any concurrent coroutines.
    public void ForceHide(bool deactivate=true)
    {
        if(fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        if(deactivate)
            Deactivate();
        if (gameObject.activeSelf)
            fadeCoroutine = StartCoroutine(Fade(1, 0));
    }

    /// Fades the canvasGroup given a starting/ending alpha, and evaluates it along the FADE_ANIM_CURVE. 
    /// <param name="start">The starting alpha</param>
    /// <param name="end">The ending alpha</param>
    protected IEnumerator Fade(float start, float end)
    {
        float time = 0;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            float eval = time / animationTime;

            canvasGroup.alpha = Mathf.Lerp(
                start, 
                end, 
                StaticInfoObjects.Instance.FADE_ANIM_CURVE.Evaluate(eval)
            );
            yield return null;
        }
        
        canvasGroup.alpha = end;
        
        if (end == 0) // if the fade is an hide transition
        {
            Display(false);
            LateDeactivate();
        }
        else
        {
            LateActivate();
        }

        fadeCoroutine = null;
    }

    private void Display(bool active)
    {
        if (alternativeGameObject)
            containerGameObject.SetActive(active);
        else
            gameObject.SetActive(active);
    }

    protected void Init()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
}