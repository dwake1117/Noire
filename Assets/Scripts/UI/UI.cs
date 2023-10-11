﻿using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UI : MonoBehaviour
{
    [SerializeField] protected GameObject containerGameObject;
    protected CanvasGroup canvasGroup;
    protected bool alternativeGameObject = false;

    protected virtual void Activate() { }
    protected virtual void Deactivate() { }

    public virtual void Show()
    {
        Activate();
        StopAllCoroutines();
        Display(true);
        StartCoroutine(Fade(0, 1));
    }

    public virtual void Hide()
    {
        Deactivate();
        StopAllCoroutines();
        StartCoroutine(Fade(1, 0));
    }

    protected IEnumerator Fade(float start, float end)
    {
        canvasGroup.alpha = start;
        
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(start, end, time);
            yield return null;
        }
        
        canvasGroup.alpha = end;
        
        if(end == 0)
            Display(false);
    }

    private void Display(bool active)
    {
        if (alternativeGameObject)
        {
            containerGameObject.SetActive(active);
        }
        else
        {
            gameObject.SetActive(active);
        }
    }
}