using UnityEngine;
using UnityEngine.UI;

public class DeathMenuUI : MonoBehaviour
{
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        respawnButton.onClick.AddListener(() =>
        {
            LoaderStatic.Load(DataPersistenceManager.Instance.CurrentScene);
        });
        
        mainMenuButton.onClick.AddListener(() =>
        {
            LoaderStatic.Load(GameScene.MainMenuScene);
        });

        Time.timeScale = 1f;
    }

}
