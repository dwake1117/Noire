using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour {
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;
     
    public static PauseMenuManager Instance { get; private set; }

    private bool isGamePaused = false;
    public bool IsGamePaused => isGamePaused;
    
    private void Awake()
    {
        Instance = this;
        
        resumeButton.onClick.AddListener(TogglePauseGame);
        mainMenuButton.onClick.AddListener(OnMainMenuClick);
        optionsButton.onClick.AddListener(OnOptionsMenuClick);
    }

    private void ToggleButtons(bool val)
    {
        resumeButton.interactable = val;
        mainMenuButton.interactable = val;
        optionsButton.interactable = val;
    }
    
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        Hide();
    }

    private void OnDestroy()
    {
        GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction() 
    {
        TogglePauseGame();
    }

    private void OnMainMenuClick()
    {
        ToggleButtons(false);
        TogglePauseGame();
        if (!Loader.Load(GameScene.MainMenuScene))
            ToggleButtons(true);
    }
    
    private void OnOptionsMenuClick()
    {
        Hide();
        OptionsUI.Instance.Show();
    }

    private void TogglePauseGame() 
    {
        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            // Time.timeScale = 0f;
            Show();
        }
        else
        {
            // Time.timeScale = 1f;
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

}