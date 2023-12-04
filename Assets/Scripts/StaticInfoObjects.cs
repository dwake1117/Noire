using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// class for storing all CONST fields such as static dictionaries, animation curves, etc
///
/// Also provides useful access functions to them
/// </summary>


public class StaticInfoObjects : MonoBehaviour
{
    public static StaticInfoObjects Instance { get; private set; }

    [SerializeField] public AnimationCurve FADE_ANIM_CURVE;
    [SerializeField] public AnimationCurve CA_DEATH_CURVE; // chromatic aberration curve
    [SerializeField] public AnimationCurve LD_DEATH_CURVE; // lens distortion curve
    
    [SerializeField] public AnimationCurve CA_QUICK_IMPULSE; // chromatic aberration curve
    [SerializeField] public AnimationCurve LD_QUICK_IMPULSE; // lens distortion curve
    
    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // maps scene -> (load type, load mode, initial position)
    public readonly Dictionary<GameScene, SceneInfo> LOADING_INFO = new()
    {
        { GameScene.MainMenuScene, new SceneInfo(SceneType.Single, null, Vector3.zero) },
        { GameScene.DeathScene, new SceneInfo(SceneType.Single, null, Vector3.zero) },
        { GameScene.LoadingScene, new SceneInfo(SceneType.Single, null, Vector3.zero) },
        
        { GameScene.SoluraBase, new SceneInfo(SceneType.Parent, GameScene.SoluraEntry, new Vector3(0, 10, 0)) },
        { GameScene.SoluraEntry, new SceneInfo(SceneType.Child, GameScene.SoluraBase, new Vector3(0, 10, 0)) },
        { GameScene.SoluraCliffHouses, new SceneInfo(SceneType.Child, GameScene.SoluraBase, Vector3.zero) },
        
        { GameScene.BedrockPlains, new SceneInfo(SceneType.Single, null, Vector3.zero) },
        
        { GameScene.TheShorelines, new SceneInfo(SceneType.Single, null, Vector3.zero) },
    };
    
    public readonly Dictionary<string, GameScene> GAMESCENES = new()
    {
        { "MainMenuScene", GameScene.MainMenuScene },
        { "DeathScene", GameScene.DeathScene },
        { "LoadingScene", GameScene.LoadingScene },
        
        { "BedrockPlains", GameScene.BedrockPlains },
        
        { "SoluraBase", GameScene.SoluraBase },
        { "SoluraEntry", GameScene.SoluraEntry },
        { "SoluraCliffHouses", GameScene.SoluraCliffHouses },
        
        { "TheShorelines", GameScene.TheShorelines },
    };
    public SceneType GetSceneType(string scene)
    {
        return LOADING_INFO[GAMESCENES[scene]].Type;
    }

    public readonly Dictionary<DreamState, Color> VORONOI_INDICATOR = new()
    {
        { DreamState.Neutral, Color.black },
        { DreamState.Lucid, Color.cyan },
        { DreamState.Deep, Color.magenta },
    };
}