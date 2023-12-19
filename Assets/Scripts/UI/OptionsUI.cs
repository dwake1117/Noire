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
    private string SOUND_TEXT => $"Sound Effects: {AudioManager.Instance.currSfxLevel}/{AudioManager.maxSounLevels}";
    private string MUSIC_TEXT => $"Music: {AudioManager.Instance.currOstLevel}/{AudioManager.maxSounLevels}";
    
    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Start()
    {
        soundEffectsButton.SetText(SOUND_TEXT);
        musicButton.SetText(MUSIC_TEXT);
        
        soundEffectsButton.AddListener(() => VolChange("Sfx"));
        musicButton.AddListener(() => VolChange("Ost"));
        controlsButton.AddListener(OnControlsButtonClicked);
        backButton.AddListener(Back);
        
        gameObject.SetActive(false);
        
        GameInput.Instance.OnPauseToggle += OnPause;
    }
    
    private void OnDestroy()
    {
        GameInput.Instance.OnPauseToggle -= OnPause;
    }
    
    private void ToggleButtons(bool enable)
    {
        if (enable)
        {
            soundEffectsButton.Enable();
            musicButton.Enable();
            controlsButton.Enable();
            backButton.Enable();
        }
        else
        {
            soundEffectsButton.Disable();
            musicButton.Disable();
            controlsButton.Disable();
            backButton.Disable();
        }
    }

    protected override void Activate()
    {
        ToggleButtons(true);
    }

    protected override void Deactivate()
    {
        ToggleButtons(false);
    }

    private void OnPause()
    {
        if (PauseMenu.Instance.gameObject.activeSelf || ControlsUI.Instance.gameObject.activeSelf)
            return;
        
        if(Hide())
            PauseMenu.Instance.Show(false);
    }
    
    private void Back()
    {
        if(Hide())
            PauseMenu.Instance.Show(false);
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
            soundEffectsButton.SetText(SOUND_TEXT);
        else
            musicButton.SetText(MUSIC_TEXT);
    }
}

