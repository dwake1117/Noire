using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExamineUI : UI
{
    public static ExamineUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI examineText;
    [SerializeField] private RawImage examineImage;
    [SerializeField] private ButtonUI closeButton;

    private void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        closeButton.AddListener(() => Hide());
    }

    public void Display(string text, Texture2D image)
    {
        gameObject.SetActive(true);
        examineText.text = text;

        if (examineImage)
        {
            examineImage.texture = image;
            examineImage.GetComponent<RectTransform>().sizeDelta = new Vector2(image.width, image.height);
        }

        Show();
        HideAfterDelay(10);
    }
}
