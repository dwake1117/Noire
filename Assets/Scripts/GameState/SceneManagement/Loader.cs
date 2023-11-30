using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader 
{
    private static string LoadScene = GameScene.LoadingScene.ToString();
    public const GameScene FirstScene = GameScene.BedrockPlains;

    public static SceneInfo TargetSceneInfoObj;
    public static string TargetScene;
    
    // THE function to call to load any scene. Returns true upon successful loading.
    public static bool Load(GameScene nextScene)
    {
        TargetScene = nextScene.ToString();
        TargetSceneInfoObj = StaticInfoObjects.Instance.LOADING_INFO[nextScene];
        
        switch (TargetSceneInfoObj.Type)
        {
            case SceneType.Single:
                return SceneTransitioner.Instance.LoadSceneSingle(LoadScene);
            case SceneType.Parent:
                return SceneTransitioner.Instance.LoadSceneSingle(LoadScene);
            case SceneType.Child:
                return SceneTransitioner.Instance.LoadSceneChild(TargetScene);
            default:
                return false;
        }
    }
    
    // overloading: load using string scene name
    public static bool Load(string nextScene)
    {
        return Load(StaticInfoObjects.Instance.GAMESCENES[nextScene]);
    }
    
    public static bool Load(Scene nextScene)
    {
        return Load(StaticInfoObjects.Instance.GAMESCENES[nextScene.name]);
    }

    public static void Respawn()
    {
        Load(GameScene.SoluraBase);
    }
}
