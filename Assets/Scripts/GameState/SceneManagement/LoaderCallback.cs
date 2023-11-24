using System;
using System.Collections;
using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(LoadLevelAsync());
    }
    
    private IEnumerator LoadLevelAsync()
    {
        var info = Loader.TargetSceneInfoObj;
        yield return new WaitForSeconds(1.6f);

        switch (info.Type)
        {
            case SceneType.Single:
                SceneTransitioner.Instance.LoadSceneSingle(Loader.TargetScene);
                break;
            case SceneType.Parent:
                SceneTransitioner.Instance.LoadSceneParent(Loader.TargetScene);
                break;
        }
    }
}