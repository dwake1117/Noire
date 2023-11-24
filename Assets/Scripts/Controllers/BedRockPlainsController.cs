using System.Collections;
using FlatKit;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class BedRockPlainsController : ParentSceneController, IDataPersistence
{
    [Header("Opening Lights Animation (Lantern Interact)")]
    [SerializeField] private Light mainLight;
    [SerializeField] private GameObject streetlampLight;
    [SerializeField] private float finalIntensity;
    [SerializeField] private AnimationCurve openLightsIntensityCurve;
    [SerializeField] private float animationTime = 3;
    
    [SerializeField] private int fogIndex;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystemBase dustParticles;
    [SerializeField] private ParticleSystemBase fireflies;
    
    [Header("Audio Manager")]
    [SerializeField] private AudioManager audioManager;
    
    private bool lightsOpened;
    
    protected override void Init()
    {
        audioManager.ChangeGlobalParaByName("Walking Surfaces", 1);
        mainLight.intensity = 0.01f;
        streetlampLight.gameObject.SetActive(false);
        ScriptableRendererFeatureManager.Instance.EnableOnlyOneFog(fogIndex);
    }

    protected override void LateInit()
    {
        if (lightsOpened)
        {
            PostProcessingManager.Instance.SetVignetteIntensity(0);
            Begin();
        }
        else
        {
            PostProcessingManager.Instance.SetVignetteIntensity(1);
            GameEventsManager.Instance.BedrockPlainsEvents.OnLampInteract += OpenLights;
            ToggleAllInteractables(false);
        }
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.BedrockPlainsEvents.OnLampInteract -= OpenLights;
    }
    
    // if light is opened
    private void Begin()
    {
        mainLight.intensity = finalIntensity;
        bgmAudio.PlayBgmAudio();
        dustParticles.Play();
        fireflies.Play();
        ToggleAllInteractables(true);
    }
    
    private void OpenLights()
    {
        lightsOpened = true;
        streetlampLight.gameObject.SetActive(true);
        DataPersistenceManager.Instance.SaveGame();
        StartCoroutine(PlayOpeningLightsAnimation());
        StartCoroutine(UnObscureSight());
    }

    private IEnumerator PlayOpeningLightsAnimation()
    {
        yield return new WaitForSeconds(.2f);
        float time = 0;
        while (time < 1)
        {
            mainLight.intensity = Mathf.Lerp(
                0, 
                finalIntensity, 
                openLightsIntensityCurve.Evaluate(time)
            );
            time += Time.deltaTime / animationTime;
            yield return null;
        }

        Begin();
    }

    // turns the vignette to desired level
    private IEnumerator UnObscureSight()
    {
        float time = 0;
        while (time < 1)
        {
            PostProcessingManager.Instance.SetVignetteIntensity(Mathf.Lerp(
                1, 
                0, 
                openLightsIntensityCurve.Evaluate(time)
            ));
            time += Time.deltaTime / animationTime;
            yield return null;
        }
    }

    #region IDataPersistence
    
    // this is called before START. AWAKE -> SCENE LOAD -> START
    // we will initialize the scene in START
    public void LoadData(GameData gameData)
    {
        lightsOpened = gameData.LightsOpen;
    }
    
    public void SaveData(GameData gameData)
    {
        gameData.LightsOpen = lightsOpened;
    }
    #endregion
}