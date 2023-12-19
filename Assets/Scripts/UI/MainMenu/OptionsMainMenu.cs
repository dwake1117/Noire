using UnityEngine;
using UnityEngine.UI;

public class OptionsMainMenu : UI
{
    public static OptionsMainMenu Instance { get; private set; }
    
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private ButtonUI controlsButton;
    [SerializeField] private ButtonUI backButton;
    
    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Start()
    {
        sfxSlider.value = AudioManager.Instance.defaultSoundLevel;
        musicSlider.value = AudioManager.Instance.defaultSoundLevel;
        
        sfxSlider.onValueChanged.AddListener(delegate(float level) { VolChange("Sfx", level); });
        musicSlider.onValueChanged.AddListener(delegate(float level) { VolChange("Ost", level); });
        
        controlsButton.AddListener(OnControlsButtonClicked);
        backButton.AddListener(Back);
        
        gameObject.SetActive(false);
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
    
    private void Back()
    {
        if(Hide())
            MainMenu.Instance.Show(false);
    }

    private void OnControlsButtonClicked()
    {
        Hide();
        ControlsMainMenu.Instance.Show();
    }

    private void VolChange(string vcaType, float level)
    {
        // TODO: play slider volume change sfx
        AudioManager.Instance.SetVolume(vcaType, level);
    }
}