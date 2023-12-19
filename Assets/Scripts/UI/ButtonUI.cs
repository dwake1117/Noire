using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Color textColorTransparent;
    [SerializeField] private Color textColorOpaque;
    [SerializeField] public TextMeshProUGUI buttonText;
    [SerializeField] private CanvasGroup leftIndicator;
    [SerializeField] private CanvasGroup rightIndicator;
    private Button button;
    
    private float scaleAmount = 1.07f;
    private float scaleAnimationTime = .3f;
    private Coroutine scaleOnSelect;
    private Vector3 initialScale;
    
    private float textAlphaPeriod = 1.5f;
    private float textAlphaMultiplier = 0.4f;
    private Coroutine textAlphaCycleOnSelect;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        initialScale = transform.localScale;
        SetIndicatorAlphas(0);
        button.onClick.AddListener(AudioManager.Instance.PlayOnClick);
    }

    public void Disable(bool setTransparent = true)
    {
        button.interactable = false;
        if(setTransparent)
            buttonText.color = textColorTransparent;
        
        if (scaleOnSelect != null)
            StopCoroutine(scaleOnSelect);
        if (textAlphaCycleOnSelect != null)
            StopCoroutine(textAlphaCycleOnSelect);
    }

    public void Enable()
    {
        button.interactable = true;
        buttonText.color = textColorOpaque;
        SetIndicatorAlphas(0);
    }

    public void AddListener(UnityAction call)
    {
        button.onClick.AddListener(call);
    }

    public void SetText(string text)
    {
        buttonText.text = text;
    }

    /// animates a scaled up effect
    private IEnumerator ScaleSelection(bool startAnimation)
    {
        if (!startAnimation)
        {
            SetIndicatorAlphas(0);
            
            float time = 0;
        
            while (time < scaleAnimationTime)
            {
                time += Time.deltaTime;
                
                float eval = time / scaleAnimationTime;
                transform.localScale = Vector3.Lerp(transform.localScale, initialScale, eval);
                
                yield return null;
            }
        }
        else
        {
            SetIndicatorAlphas(1);

            Vector3 endScale = initialScale * scaleAmount;
            float time = 0;
        
            while (time < scaleAnimationTime)
            {
                time += Time.deltaTime;
                
                float eval = time / scaleAnimationTime;
                transform.localScale = Vector3.Lerp(transform.localScale, endScale, eval);
                
                yield return null;
            }   
        }

        scaleOnSelect = null;
    }

    private IEnumerator TextAlphaCycle()
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime / textAlphaPeriod;
            t %= 1;
            buttonText.alpha = Mathf.Lerp(1, textAlphaMultiplier, StaticInfoObjects.Instance.TEXT_ALPHA_CYCLE_CURVE.Evaluate(t));
            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            eventData.selectedObject = gameObject;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable)
        {
            eventData.selectedObject = null;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button.interactable)
        {
            if (scaleOnSelect != null)
                StopCoroutine(scaleOnSelect);
            scaleOnSelect = StartCoroutine(ScaleSelection(true));

            textAlphaCycleOnSelect = StartCoroutine(TextAlphaCycle());
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (button.interactable)
        {
            if (scaleOnSelect != null)
                StopCoroutine(scaleOnSelect);
            scaleOnSelect = StartCoroutine(ScaleSelection(false));

            if (textAlphaCycleOnSelect != null)
            {
                StopCoroutine(textAlphaCycleOnSelect);
                buttonText.alpha = 1;
            }
        }
    }

    private void SetIndicatorAlphas(float alpha)
    {
        if (leftIndicator)
        {
            leftIndicator.alpha = alpha;
            rightIndicator.alpha = alpha;
        }
    }
}