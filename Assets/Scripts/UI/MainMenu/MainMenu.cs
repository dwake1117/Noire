using UnityEngine;
using UnityEngine.UI;

public class MainMenu : UI
{
    public static MainMenu Instance { get; private set; }
    
    [SerializeField] private ButtonUI newGameButton;
    [SerializeField] private ButtonUI continueGameButton;
    [SerializeField] private ButtonUI loadGameButton;
    [SerializeField] private ButtonUI quitGameButton;
    [SerializeField] private ButtonUI settingsButton;

    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Start() 
    {
        newGameButton.AddListener(OnNewGameClicked);
        continueGameButton.AddListener(OnContinueGameClicked);
        loadGameButton.AddListener(OnLoadGameClicked);
        quitGameButton.AddListener(OnQuitGameClicked);
        settingsButton.AddListener(OnSettingsClicked);
        
        Show();
    }

    private void ToggleButtons(bool enable)
    {
        if (enable)
        {
            newGameButton.Enable();
            continueGameButton.Enable();
            loadGameButton.Enable();
            quitGameButton.Enable();
        }
        else
        {
            newGameButton.Disable();
            continueGameButton.Disable();
            loadGameButton.Disable();
            quitGameButton.Disable();
        }
    }

    private void OnNewGameClicked()
    {
        Hide();
        SaveSlotsMenu.Instance.DisplayNewGameMenu();
    }

    private void OnContinueGameClicked()
    {
        ToggleButtons(false);
        if(!Loader.Load(DataPersistenceManager.Instance.LastCheckPointScene))
            ToggleButtons(true);
    }
    
    private void OnLoadGameClicked() 
    {
        Hide();
        SaveSlotsMenu.Instance.DisplayLoadGameMenu();
    }

    private void OnQuitGameClicked()
    {
        Hide();
        ToggleButtons(false);
        Invoke(nameof(Application.Quit), animationTime);
    }

    private void OnSettingsClicked()
    {
        Hide();
        OptionsMainMenu.Instance.Show();
    }
    
    // UI baseclass
    protected override void Activate()
    {
        if (!DataPersistenceManager.Instance.HasGameData()) 
        {
            continueGameButton.Disable();
            loadGameButton.Disable();
        }
    }
}