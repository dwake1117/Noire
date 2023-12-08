using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Volume Controller")]
    private FMOD.Studio.VCA sfxVCA;
    private FMOD.Studio.VCA ostVCA;
    
    [Header("BGM")]
    private FMOD.Studio.EventInstance currBgmState;

    [SerializeField] private AnimationCurve volumeAnimCurve;

    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        volumeAnimCurve.Evaluate(0.2f);
    }
    
    public bool IsPlaying(FMOD.Studio.EventInstance instance) 
    {
        instance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    private void InitializeNewBgmEvent(EventReference bgmAudioEvent)
    {
        var bgmState = RuntimeManager.CreateInstance(bgmAudioEvent);
        currBgmState = bgmState;
    }
    
    public void PlayBgmAudio(EventReference bgmAudioEvent)
    {
        if (IsPlaying(currBgmState))
        {
            StopBgmAudio();
        }
        
        InitializeNewBgmEvent(bgmAudioEvent);
        
        if (!IsPlaying(currBgmState))
        {
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(currBgmState, Player.Instance.transform, false);
            currBgmState.start();
        }
    }
    
    public void StopBgmAudio()
    {
        currBgmState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currBgmState.release();
    }

    public float GetVcaVolume(string vca)
    {
        float result;
        if (vca == "Sfx")
        {
            sfxVCA = RuntimeManager.GetVCA("vca:/SfxVCA");
            sfxVCA.getVolume(out result);
            return result;
        }
        else
        {
            ostVCA = RuntimeManager.GetVCA("vca:/OstVCA");
            ostVCA.getVolume(out result);
            return result;
        }
    }
    public void SetSfxVolume(float desVolume)
    {
        sfxVCA = RuntimeManager.GetVCA("vca:/SfxVCA");
        if (!(sfxVCA.isValid()))
        {
            Debug.LogError("sfxVca is not Valid");
        }
        else
        {
            if (0 <= desVolume && desVolume <= 1.25)
            {
                sfxVCA.setVolume(desVolume);
                // sfxVolume = desVolume;
                float vol;
                sfxVCA.getVolume(out vol);
                //Debug.Log("sfx" + vol);
            }
            else
            {
                Debug.LogError("sfxVca desvolume is not valid");
            }
        }
    }
    public void SetOstVolume(float desVolume)
    {
        ostVCA = RuntimeManager.GetVCA("vca:/OstVCA");
        if (!(ostVCA.isValid()))
        {
            Debug.LogError("OstVca is not Valid");
        }
        else
        {
            if (0 <= desVolume && desVolume <= 1.25)
            {
                ostVCA.setVolume(desVolume);
                // ostVolume = desVolume;
                float vol;
                ostVCA.getVolume(out vol);
                //Debug.Log("ost:" +vol);
            }
            else
            {
                Debug.LogError("OstVca desvolume is not valid");
            }
        }
    }
    public void ChangeGlobalParaByName(string name, float value)
    {
        FMOD.Studio.PARAMETER_DESCRIPTION parameterDescription;
        var result =
            RuntimeManager.StudioSystem.getParameterDescriptionByName(name, out parameterDescription);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Setting Global params failed");
            return;
        }

        result = RuntimeManager.StudioSystem.setParameterByID(parameterDescription.id, value);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Setting Global params failed");
        }
    }

    public void PlaySceneBegins ()
    {
        RuntimeManager.PlayOneShot("event:/UI/NewScene");
    }
}
