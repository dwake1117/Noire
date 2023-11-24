using UnityEngine;
using UnityEngine.SceneManagement;
public struct SceneInfo
{
    public SceneType Type {get; private set;}
    
    // each child scene should specify a parent. If parent is null, the scene is a standalone scene.
    // We also need to specify a default child if the scene is of Type = Parent
    public GameScene? ParentOrDefaultChild { get; private set; }
    
    public Vector3 InitialPosition {get; private set;}

    public SceneInfo(SceneType type, GameScene? parentOrDefaultChild, Vector3 initialPosition)
    {
        Type = type;
        ParentOrDefaultChild = parentOrDefaultChild;
        InitialPosition = initialPosition;
    }
}