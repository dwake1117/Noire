using UnityEngine;

public class SoluraEntryController : ChildSceneController
{
    protected override void LateInit()
    {
        ToggleAllInteractables(true);
    }
}