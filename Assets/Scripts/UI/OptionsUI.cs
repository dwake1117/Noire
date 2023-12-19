using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : UI
{
    public static OptionsUI Instance { get; private set; }
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
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
        sfxSlider.onValueChanged.AddListener(delegate(float level) { VolChange("Sfx", level); });
        musicSlider.onValueChanged.AddListener(delegate(float level) { VolChange("Ost", level); });
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
        musicSlider.enabled = enable;
        sfxSlider.enabled = enable;
        if (enable)
        {
            controlsButton.Enable();
            backButton.Enable();
        }
        else
        {
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

    private void VolChange(string vcaType, float level)
    {
        // TODO: play slider volume change sfx
        AudioManager.Instance.SetVolume(vcaType, level);
    }
}

