using UnityEngine;

public class SoluraBaseController : ParentSceneController
{
    [SerializeField] private int fogIndex;
    
    protected override void Init()
    {
        ScriptableRendererFeatureManager.Instance.EnableOnlyOneFog(fogIndex);
    }
}