using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

public partial class Player
{
    [Header("---------- Player Animator ---------- ")]
    [SerializeField] private ParticleSystemBase normalSlash;
    [SerializeField] private float normalSlashOffset = 1f;
    [SerializeField] private float normalSlashOffsetY = 1f;
    [SerializeField] private ParticleSystemBase chargedSlash;
    [SerializeField] private float chargedSlashOffset = 2f;
    [SerializeField] private float chargedSlashOffsetY = 2f;
    [SerializeField] private ParticleSystemBase particleOutwardSplash;
    
    [Header("Dash Effects")]
    [SerializeField] private VisualEffect dashSmokePuff;
    [SerializeField] private float dashSmokePuffOffset = 2f;
    [SerializeField] private float dashSmokePuffOffsetY = .2f;
    [SerializeField] private VisualEffect runSmokePuff;
    [SerializeField] private Vector3 runSmokePuffOffset = new(0f, 0.89f, 0f);
    [SerializeField] private float deathAnimationTime = 1f;
    
    private const string WALK = "PlayerWalk";
    private const string IDLE = "PlayerIdle";
    private const string FALL = "PlayerFall";
    private const string RUN = "PlayerRun";
    private const string KNOCKBACK = "PlayerKnockback";
    private const string DIE = "PlayerDeath";
    private int WALK_ID;
    private int IDLE_ID;
    private int FALL_ID;
    private int RUN_ID;
    private int KNOCKBACK_ID;
    private int DIE_ID;
    
    
    private void AnimatorAwake()
    {
        WALK_ID  = Animator.StringToHash(WALK);
        IDLE_ID =  Animator.StringToHash(IDLE);
        FALL_ID =  Animator.StringToHash(FALL);
        RUN_ID = Animator.StringToHash(RUN);
        DIE_ID = Animator.StringToHash(DIE);
        KNOCKBACK_ID = Animator.StringToHash(KNOCKBACK);
    }

    private void AnimatorUpdate()
    {
        animator.SetBool(WALK_ID, IsWalking());
        animator.SetBool(IDLE_ID, IsIdle());
        animator.SetBool(FALL_ID, IsFalling());
        animator.SetBool(RUN_ID, IsRunning());
        animator.SetBool(KNOCKBACK_ID, IsKnockedBack());
        animator.SetBool(DIE_ID, IsDead());
    }
    
    private void PlayDeathAnimation()
    {
        StartCoroutine(DeathAnimationCoroutine());
    }

    private IEnumerator DeathAnimationCoroutine()
    {
        var loadingOperation = SceneManager.LoadSceneAsync(GameScene.DeathScene.ToString());
        loadingOperation.allowSceneActivation = false;
        
        PostProcessingManager.Instance.SetSaturation(-95f);
        CameraManager.Instance.CameraShake(deathAnimationTime / 2, 7f);
        
        float time = 0;
        while (time < deathAnimationTime)
        {
            float eval = time / deathAnimationTime;
            
            // post effects
            PostProcessingManager.Instance.SetLensDistortionIntensity(
                Mathf.Lerp(0, -1, StaticInfoObjects.Instance.LD_DEATH_CURVE.Evaluate(eval)));
            PostProcessingManager.Instance.SetChromaticAberrationIntensity(
                Mathf.Lerp(0, 1, StaticInfoObjects.Instance.CA_DEATH_CURVE.Evaluate(eval)));
            PostProcessingManager.Instance.SetContrast(
                Mathf.Lerp(0, 100, StaticInfoObjects.Instance.LD_DEATH_CURVE.Evaluate(eval)));
            
            time += Time.deltaTime;
            
            yield return null;
        }
        
        //TODO: end death SFX here!    
        loadingOperation.allowSceneActivation = true;
        
    }

    private void PlayDreamStateChangeAnimation()
    {
        TimeManager.Instance.DoSlowMotion(duration:.5f);
        PostProcessingManager.Instance.CAImpulse();
        PostProcessingManager.Instance.LDImpulse();
    }

    private void PlayNormalSlash(bool inverted=false)
    {
        normalSlash.transform.position = transform.position + transform.forward * normalSlashOffset + new Vector3(0, normalSlashOffsetY, 0);
        normalSlash.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y + 180, inverted ? 180 : 0));
        normalSlash.Restart();
    }
    
    private void Swing1AnimationStartedTrigger()
    {
        PlayNormalSlash();
    }
    
    // the reverse swing
    private void Swing2AnimationStartedTrigger()
    {
        PlayNormalSlash(true);
    }
    
    private void ChargedSwingAnimationStartedTrigger()
    {
        particleOutwardSplash.transform.position = transform.position;
        particleOutwardSplash.Restart();
        CameraManager.Instance.CameraShake(.2f, 4f);
        PostProcessingManager.Instance.CAImpulse();
        
        chargedSlash.transform.position = transform.position + transform.forward * chargedSlashOffset + new Vector3(0, chargedSlashOffsetY, 0);
        chargedSlash.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y + 180, 180));
        chargedSlash.Restart();
    }
    
    private void DashAnimationStartedTrigger()
    {
        
        dashSmokePuff.transform.position = transform.position + transform.forward * dashSmokePuffOffset + new Vector3(0, dashSmokePuffOffsetY, 0);
        dashSmokePuff.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y + 143, 0));
        FMODUnity.RuntimeManager.PlayOneShot("event:/Character/Player/PlayerDash", transform.position);
        dashSmokePuff.Play();
    }

    private void RunOneStepTrigger()
    {
        runSmokePuff.transform.position = transform.position + runSmokePuffOffset;
        runSmokePuff.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y + 90, 0));
        runSmokePuff.Play();
        
        // TODO: felix this shouldnt be hardcoded here. Either make the event constant or use Scriptable objects to encode the string.
        FMODUnity.RuntimeManager.PlayOneShot("event:/Character/Player/PlayerFootsteps", transform.position);
    }
}
