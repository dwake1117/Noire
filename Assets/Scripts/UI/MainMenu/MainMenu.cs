using UnityEngine;
using UnityEngine.UI;

public class MainMenu : UI
{
    public static MainMenu Instance { get; private set; }
    
    [Header("Menu Navigation")]
    [SerializeField] private SaveSlotsMenu saveSlotsMenu;
    
    [Header("Menu Buttons")]
    [SerializeField] private ButtonUI newGameButton;
    [SerializeField] private ButtonUI continueGameButton;
    [SerializeField] private ButtonUI loadGameButton;
    [SerializeField] private ButtonUI quitGameButton;

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
        saveSlotsMenu.NewGameMenu();
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
        saveSlotsMenu.LoadGameMenu();
    }

    private void OnQuitGameClicked()
    {
        ToggleButtons(false);
        Application.Quit();
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