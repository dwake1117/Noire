using UnityEngine;
using UnityEngine.UI;

public class DeathMenuUI : MonoBehaviour
{
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        respawnButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.ValleyofSolura);
        });
        
        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        Time.timeScale = 1f;
    }

}
