using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : UI
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private ButtonUI soundEffectsButton;
    [SerializeField] private ButtonUI musicButton;
    [SerializeField] private ButtonUI controlsButton;
    [SerializeField] private ButtonUI backButton;
    private const string SOUND_TEXT = "Sound Effects: ";
    private const string MUSIC_TEXT = "Music: ";
    
    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Start()
    {
        soundEffectsButton.AddListener(() => VolChange("Sfx"));
        musicButton.AddListener(() => VolChange("Ost"));
        controlsButton.AddListener(OnControlsButtonClicked);
        backButton.AddListener(OnBackButtonClicked);
        
        gameObject.SetActive(false);
        
        GameInput.Instance.OnPauseToggle += GameInputOnPauseToggle;
    }

    private void OnDestroy()
    {
        GameInput.Instance.OnPauseToggle -= GameInputOnPauseToggle;
    }

    private void GameInputOnPauseToggle()
    {
        Hide();
    }
    
    private void OnBackButtonClicked()
    {
        Hide();
        PauseMenu.Instance.Show();
    }

    private void OnControlsButtonClicked()
    {
        Hide();
        ControlsUI.Instance.Show();
    }

    private void VolChange(string vcaType)
    {
        AudioManager.Instance.SetVolume(vcaType);
        if(vcaType == "Sfx")
            soundEffectsButton.SetText(SOUND_TEXT + AudioManager.Instance.currSfxLevel);
        else
            musicButton.SetText(MUSIC_TEXT + AudioManager.Instance.currOstLevel);
    }
}

