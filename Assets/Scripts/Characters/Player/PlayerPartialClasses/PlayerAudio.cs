using UnityEngine;
using FMODUnity;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public partial class Player
{
    FMOD.Studio.EventInstance chargeAttack;
    public void PlaySteps(string path)
    {
        RuntimeManager.PlayOneShot(path, transform.position);
    }

    public void PlaySwoosh(string path)
    {
        RuntimeManager.PlayOneShot(path, transform.position);
    }

    public void PlayOnHitSound()
    {
        RuntimeManager.PlayOneShot("event:/Character/Player/PlayerOnHit", transform.position);
    }
    
    public void PlayCharge()
    {   
        StopPlayCharge();
        chargeAttack = RuntimeManager.CreateInstance("event:/Character/Player/PlayerChargeAttack");
        chargeAttack.start();
        RuntimeManager.AttachInstanceToGameObject(chargeAttack, Player.Instance.transform, false);
    }

    public void StopPlayCharge()
    {
        if (AudioManager.Instance.IsPlaying(chargeAttack))
        {
            chargeAttack.stop(STOP_MODE.ALLOWFADEOUT);
            chargeAttack.release();
        }


    }

    public void PlayDashSound()
    {
        //RuntimeManager.PlayOneShot("");
    }
    public void PlayDeathSound()
    {
        AudioManager.Instance.StopBgmAudio();
        RuntimeManager.PlayOneShot("event:/Character/Player/PlayerDie", transform.position);
    }

    public void StopDeathSound()
    {
    }

}