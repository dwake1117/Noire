using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader 
{
    private static string LoadScene = GameScene.LoadingScene.ToString();
    public const GameScene FirstScene = GameScene.BedrockPlains;

    public static SceneInfo TargetSceneInfoObj;
    public static string TargetScene;

    public static Action SceneLoadedCallback; 
    
    // Loads a GameScene. Returns true upon successful loading.
    public static bool Load(GameScene nextScene, Action callback=null)
    {
        SceneLoadedCallback = callback;
        
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
    public static bool Load(string nextScene, Action callback=null)
    {
        return Load(StaticInfoObjects.Instance.GAMESCENES[nextScene], callback);
    }
    
    public static bool Load(Scene nextScene, Action callback=null)
    {
        return Load(StaticInfoObjects.Instance.GAMESCENES[nextScene.name], callback);
    }

    public static void Respawn()
    {
        Load(TargetScene, Player.Instance.Respawn);
    }
}
