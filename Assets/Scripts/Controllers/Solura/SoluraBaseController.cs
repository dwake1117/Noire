using UnityEngine;

public class SoluraBaseController : ParentSceneController
{
    [SerializeField] private int fogIndex;

    // need to reinitialize in case sometimes changed in PostProcessing Manager
    protected override void LateInit()
    {
        PostProcessingManager.Instance.SetSaturation(-15f);
        PostProcessingManager.Instance.SetContrast(0);
        PostProcessingManager.Instance.SetLensDistortionScale(1);
        PostProcessingManager.Instance.SetLensDistortionIntensity(0);
        PostProcessingManager.Instance.SetChromaticAberrationIntensity(0);
    }
    
    protected override void Init()
    {
        ScriptableRendererFeatureManager.Instance.EnableOnlyOneFog(fogIndex);
        AudioManager.Instance.PlayBgmAudio(bgmAudioEvent);
    }
}