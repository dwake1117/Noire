using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    public Transform screenspaceLookAt;
    
    [Header("Camera zoom/pan")]
    // here orthographic camera is used so FOV is actually m_Lens.OrthographicSize
    [SerializeField] private float FOVmax;
    [SerializeField] private float FOVmin;
    [SerializeField] private float FOVDefault;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Camera effects")] 
    private const float shakeDuration = .5f;
    private const float shakeMagnitude = 10f;
    private Coroutine shakeCoroutine;
    private CinemachineBasicMultiChannelPerlin shakeNoise;
    
    private float targetFOV;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        shakeNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
        shakeNoise.m_AmplitudeGain = 0;
        virtualCamera.m_Lens.OrthographicSize = FOVDefault;
        targetFOV = virtualCamera.m_Lens.OrthographicSize;
    }

    private void Start()
    {
        virtualCamera.m_Lens.OrthographicSize = FOVDefault;
    }

    private void Update()
    {
        HandleCameraZoom();
    }

    private void HandleCameraZoom() {
        float zoomVal = GameInput.Instance.GetZoomVal();
        if (zoomVal > 0)
            targetFOV -= .5f;
        if (zoomVal < 0)
            targetFOV += .5f;

        targetFOV = Mathf.Clamp(targetFOV, FOVmin, FOVmax);
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetFOV, Time.deltaTime * zoomSpeed); ;
    }

    public void CameraShake(float duration=shakeDuration, float magnitude=shakeMagnitude)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        shakeNoise.m_AmplitudeGain = magnitude;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            shakeNoise.m_AmplitudeGain = Mathf.Lerp(magnitude, 0, time / duration);
            
            yield return null;
        }

        shakeNoise.m_AmplitudeGain = 0;
        shakeCoroutine = null;
    }
}
