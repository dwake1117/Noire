using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExamineUI : UI
{
    public static ExamineUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI examineText;
    [SerializeField] private UnityEngine.UI.RawImage examineImage;
    [SerializeField] private ButtonUI closeButton;

    private void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        closeButton.AddListener(Hide);
    }

    public void Display(string text, Texture2D image)
    {
        Debug.Log("here");
        gameObject.SetActive(true);
        examineText.text = '-' + text;
        examineImage.texture = image;
        int x = image.width;
        int y = image.height;
        examineImage.GetComponent<RectTransform>().sizeDelta = new Vector2(x, y);
        Show();
        StartCoroutine(HideText());
    }

    IEnumerator HideText()
    {
        yield return new WaitForSeconds(10);
        Hide();
    }
}
