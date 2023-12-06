using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

/// <summary>
/// The Player Animator class. Attached to a Player instance.
/// Controls animation and VFXs. 
/// </summary>

[RequireComponent(typeof(Player))]
public class PlayerAnimator : MonoBehaviour
{
    public static PlayerAnimator Instance { get; private set; }
    
    [Header("Splash Effects")]
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
    
    [Header("Enemy OnHit")]
    [SerializeField] private ParticleSystemBase enemiesOnHitParticles;
        
    private Animator animator;
    private const string WALK = "PlayerWalk";
    private const string IDLE = "PlayerIdle";
    private const string FALL = "PlayerFall";
    private const string RUN = "PlayerRun";
    private const string KNOCKBACK = "PlayerKnockback";
    private const string DIE = "Death";
    private int WALK_ID;
    private int IDLE_ID;
    private int FALL_ID;
    private int RUN_ID;
    private int KNOCKBACK_ID;
    
    
    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        animator = GetComponent<Animator>();
        WALK_ID  = Animator.StringToHash(WALK);
        IDLE_ID =  Animator.StringToHash(IDLE);
        FALL_ID =  Animator.StringToHash(FALL);
        RUN_ID = Animator.StringToHash(RUN);
        KNOCKBACK_ID = Animator.StringToHash(KNOCKBACK);
    }

    private void Update()
    {
        animator.SetBool(WALK_ID, Player.Instance.IsWalking());
        animator.SetBool(IDLE_ID, Player.Instance.IsIdle());
        animator.SetBool(FALL_ID, Player.Instance.IsFalling());
        animator.SetBool(RUN_ID, Player.Instance.IsRunning());
        animator.SetBool(KNOCKBACK_ID, Player.Instance.IsKnockedBack());
    }

    // public bool AnimatorIsPlaying(int layer)
    // {
    //     return animator.GetCurrentAnimatorStateInfo(layer).length > animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
    // }
    //
    // public bool AnimatorIsPlaying(int layer, string stateName)
    // {
    //     return AnimatorIsPlaying(layer) && animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
    // }

    public void PlayDeathAnimation()
    {
        animator.SetTrigger(DIE);
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
        
        loadingOperation.allowSceneActivation = true;
    }

    public void PlayDreamStateChangeAnimation()
    {
        TimeManager.Instance.DoSlowMotion(duration:.5f);
        PostProcessingManager.Instance.CAImpulse();
        PostProcessingManager.Instance.LDImpulse();
    }

    private void PlayNormalSlash(bool inverted=false)
    {
        var t = Player.Instance.transform;
        
        normalSlash.transform.position = t.position + t.forward * normalSlashOffset + new Vector3(0, normalSlashOffsetY, 0);
        normalSlash.transform.rotation = Quaternion.Euler(new Vector3(0, t.rotation.eulerAngles.y + 180, inverted ? 180 : 0));
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
        var t = Player.Instance.transform;

        particleOutwardSplash.transform.position = t.position;
        particleOutwardSplash.Restart();
        CameraManager.Instance.CameraShake(.2f, 4f);
        PostProcessingManager.Instance.CAImpulse();
        
        chargedSlash.transform.position = t.position + t.forward * chargedSlashOffset + new Vector3(0, chargedSlashOffsetY, 0);
        chargedSlash.transform.rotation = Quaternion.Euler(new Vector3(0, t.rotation.eulerAngles.y + 180, 180));
        chargedSlash.Restart();
    }
    
    private void DashAnimationStartedTrigger()
    {
        var t = Player.Instance.transform;
        
        dashSmokePuff.transform.position = t.position + t.forward * dashSmokePuffOffset + new Vector3(0, dashSmokePuffOffsetY, 0);
        dashSmokePuff.transform.rotation = Quaternion.Euler(new Vector3(0, t.rotation.eulerAngles.y + 143, 0));
        
        dashSmokePuff.Play();
    }

    private void RunOneStepTrigger()
    {
        var t = Player.Instance.transform;
        
        runSmokePuff.transform.position = t.position + runSmokePuffOffset;
        runSmokePuff.transform.rotation = Quaternion.Euler(new Vector3(0, t.rotation.eulerAngles.y + 90, 0));
        runSmokePuff.Play();
        FMODUnity.RuntimeManager.PlayOneShot("event:/Character/Player/PlayerFootsteps", transform.position);
    }
}
