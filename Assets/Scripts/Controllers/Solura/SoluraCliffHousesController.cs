using UnityEngine;

public class SoluraCliffHousesController : ChildSceneController
{
    protected override void LateInit()
    {
        ToggleAllInteractables(true);
    }
}