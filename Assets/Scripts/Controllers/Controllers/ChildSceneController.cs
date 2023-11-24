using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// See ParentSceneController, except children doesnt have any title text appears
/// </summary>

public class ChildSceneController : MonoBehaviour
{
    [Header("Interactable Objects")]
    [SerializeField] protected InteractableObject[] unaffectedInteractableObjects;
    
    [Header("Audio")]
    [SerializeField] protected BGMAudio bgmAudio;
    
    protected List<InteractableObject> interactablesList;

    private void Awake()
    {
        SceneManager.sceneLoaded += FindAllInteractables;
        Init();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= FindAllInteractables;
    }

    private void Start()
    {
        foreach (var obj in unaffectedInteractableObjects)
            obj.Enable();
        LateInit();
    }

    protected virtual void Init() { }

    protected virtual void LateInit() { }
    
    protected void ToggleAllInteractables(bool active)
    {
        if (active)
            foreach (var interactable in interactablesList)
                interactable.Enable();
        else
            foreach (var interactable in interactablesList)
                interactable.Disable();
    }
    
    protected void FindAllInteractables(Scene scene, LoadSceneMode mode)
    {
        interactablesList = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<InteractableObject>()
            .Except(unaffectedInteractableObjects)
            .ToList();
    }
}