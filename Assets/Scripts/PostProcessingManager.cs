using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }
    
    private Volume volume;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    private Bloom bloom;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    
    public Volume GetVolume() => volume;

    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        volume = GetComponent<Volume>();
        
        if(!volume.profile.TryGet(out lensDistortion))
            Debug.LogError("Did not find a lens distortion in PostEffects");
        if(!volume.profile.TryGet(out chromaticAberration))
            Debug.LogError("Did not find a chromatic aberration in PostEffects");
        if(!volume.profile.TryGet(out bloom))
            Debug.LogError("Did not find a bloom effect in PostEffects");
        if(!volume.profile.TryGet(out vignette))
            Debug.LogError("Did not find a vignette effect in PostEffects");
        if(!volume.profile.TryGet(out colorAdjustments))
            Debug.LogError("Did not find a color adjustment in PostEffects");
    }
    
    public void SetLensDistortionIntensity(float val) => lensDistortion.intensity.value = val;
    public void SetLensDistortionScale(float val) => lensDistortion.scale.value = val;
    public void SetChromaticAberrationIntensity(float val) => chromaticAberration.intensity.value = val;
    public void SetSaturation(float val) => colorAdjustments.saturation.value = val;
    public void SetContrast(float val) => colorAdjustments.contrast.value = val;
    public void SetBloomIntensity(float val) => bloom.intensity.value = val;

    public float GetLensDistortionIntensity() => lensDistortion.intensity.value;

    public float GetChromaticAberrationIntensity() => chromaticAberration.intensity.value;

    public void SetVignetteIntensity(float val) => vignette.intensity.value = val;
    
    public float GetBloomIntensity() => bloom.intensity.value;

    private IEnumerator CAImpulseCoroutine(float animationTime, float magnitude)
    {
        float time = 0;
        while (time < animationTime)
        {
            float eval = time / animationTime;
            
            SetChromaticAberrationIntensity(
                Mathf.Lerp(0, magnitude, StaticInfoObjects.Instance.CA_QUICK_IMPULSE.Evaluate(eval)));
            
            time += Time.deltaTime;
            yield return null;
        }
        
        SetChromaticAberrationIntensity(0);
    }
    
    public void CAImpulse(float animationTime=0.2f, float magnitude=0.4f)
    {
        StartCoroutine(CAImpulseCoroutine(animationTime, magnitude));
    }

    private IEnumerator LDImpulseCoroutine(float animationTime, float magnitude)
    {
        float time = 0;
        while (time < animationTime)
        {
            float eval = time / animationTime;
            
            SetLensDistortionIntensity(
                Mathf.Lerp(0, magnitude, StaticInfoObjects.Instance.LD_QUICK_IMPULSE.Evaluate(eval)));
            
            time += Time.deltaTime;
            yield return null;
        }
        
        SetLensDistortionIntensity(0);
    }
    
    public void LDImpulse(float animationTime, float magnitude)
    {
        StartCoroutine(LDImpulseCoroutine(animationTime, magnitude));
    }

}
