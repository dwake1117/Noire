using UnityEngine;

public partial class Player
{
    public void PlaySteps(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path, transform.position);
    }
    public void PlaySwoosh(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path, transform.position);
    }

    public void PlayCharge(string path)
    {
        //FMODUnity
    }
}