using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas))]
public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner Instance { get; private set; }

    [SerializeField] private TransitionSO fadeTransitionSlow;
    [SerializeField] private TransitionSO fadeTransitionFast;
    private TransitionSO lastTransition;
    private Canvas transitionCanvas;
    private AsyncOperation loadLevelOperation;
    private AsyncOperation loadChildOperation;
    private bool isLoading = false;
    private string childScene;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        transitionCanvas = GetComponent<Canvas>();
        transitionCanvas.enabled = false;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += HandleSceneChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneChange;
    }

    // loads the next scene slow, using Single loading
    public bool LoadSceneSingle(string scene)
    {
        if (isLoading)
            return false;
        lastTransition = fadeTransitionSlow;
        
        loadLevelOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        
        loadLevelOperation.allowSceneActivation = false;
        transitionCanvas.enabled = true;

        StartCoroutine(Exit(fadeTransitionSlow));
        return true;
    }
    
    // loads the next scene slow, using Single loading, and the its default child scene Additive.
    // This function should only ever be called in LoaderCallback!
    public bool LoadSceneParent(string scene)
    {
        if (isLoading)
            return false;
        
        lastTransition = fadeTransitionSlow;
        
        // load parent first
        loadLevelOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        loadLevelOperation.allowSceneActivation = false;
        
        // load child
        childScene = Loader.TargetSceneInfoObj.ParentOrDefaultChild?.ToString();
        loadChildOperation = SceneManager.LoadSceneAsync(childScene, LoadSceneMode.Additive);
        loadChildOperation.allowSceneActivation = false;
        
        // play animation
        transitionCanvas.enabled = true;
        StartCoroutine(Exit(fadeTransitionSlow, true));

        return true;
    }
    
    // loads the next scene fast, using Additive loading, also unloads the current scene upon finishing.
    public bool LoadSceneChild(string scene)
    {
        if (isLoading)
            return false;

        lastTransition = fadeTransitionFast;
        loadLevelOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        loadLevelOperation.allowSceneActivation = false;

        transitionCanvas.enabled = true;
        StartCoroutine(Exit(fadeTransitionFast, false, true));
        
        return true;
    }

    private IEnumerator Exit(TransitionSO transitionSO, bool parent = false, bool unloadLast = false)
    {
        // start fade out
        GameEventsManager.Instance.GameStateEvents.LoadToggle(false);
        
        isLoading = true;
        yield return StartCoroutine(transitionSO.Exit(transitionCanvas));

        if (unloadLast)
        {
            SceneManager.UnloadSceneAsync(childScene);
            childScene = Loader.TargetScene;
        }

        loadLevelOperation.allowSceneActivation = true;

        if (parent)
        {
            loadChildOperation.allowSceneActivation = true;
        }
    }
    private IEnumerator Enter(TransitionSO transitionSO)
    {
        // start to fade in with next scene
        yield return StartCoroutine(transitionSO.Enter(transitionCanvas));
        isLoading = false;
        
        // finished loading
        transitionCanvas.enabled = false;
        loadLevelOperation = null;
        loadChildOperation = null;
        
        GameEventsManager.Instance.GameStateEvents.LoadToggle(true);
    }

    private void HandleSceneChange(Scene scene, LoadSceneMode mode)
    {
        // never enters scene transition when loading a parent (since the child always will be loaded as well)
        if (StaticInfoObjects.Instance.GetSceneType(scene.name) != SceneType.Parent)
        {
            if (lastTransition)
                StartCoroutine(Enter(lastTransition));
            else
            {
                Debug.Log("Scene transition not found, using default slow fade.");
                StartCoroutine(Enter(fadeTransitionSlow));
            }
        }
    }
}