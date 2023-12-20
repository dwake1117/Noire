using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ConfirmationPopupMenu : UI
{
    public static ConfirmationPopupMenu Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        Instance = this;
        
        canvasGroup = GetComponent<CanvasGroup>();
        
        gameObject.SetActive(false);
    }

    public void ActivateMenu(string text, UnityAction confirmAction, UnityAction cancelAction)
    {
        displayText.text = text;
        Show();

        // note - this only removes listeners added through code
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(() => {
            Hide();
            confirmAction();
        });
        cancelButton.onClick.AddListener(() => {
            Hide();
            cancelAction();
        });
    }
}