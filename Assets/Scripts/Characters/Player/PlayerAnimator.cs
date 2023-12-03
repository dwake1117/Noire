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
    [SerializeField] private Vector3 normalSlashOffset = new(0f, 2.3f, -1.9f);
    [SerializeField] private ParticleSystemBase chargedSlash;
    [SerializeField] private Vector3 chargedSlashOffset = new(0f, 2.3f, -3f);
    [SerializeField] private ParticleSystemBase particleOutwardSplash;
    
    [Header("Dash Effects")]
    [SerializeField] private VisualEffect dashSmokePuff;
    [SerializeField] private Vector3 dashSmokePuffOffset = new(0f, 0.89f, -2.07f);
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
        Instance = this;
        
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
        TimeManager.Instance.DoSlowMotion(.5f);
        PostProcessingManager.Instance.CAImpulse();
        PostProcessingManager.Instance.LDImpulse();
    }

    private void Swing1AnimationStartedTrigger()
    {
        var playerTransform = Player.Instance.transform;
        
        normalSlash.transform.position = playerTransform.position + normalSlashOffset;
        normalSlash.transform.rotation = Quaternion.Euler(new Vector3(0, playerTransform.rotation.eulerAngles.y + 180));
        normalSlash.Restart();
    }
    
    // the reverse swing
    private void Swing2AnimationStartedTrigger()
    {
        var playerTransform = Player.Instance.transform;
        
        normalSlash.transform.position = playerTransform.position + normalSlashOffset;
        normalSlash.transform.rotation = Quaternion.Euler(new Vector3(0, playerTransform.rotation.eulerAngles.y + 180, 180));
        normalSlash.Restart();
    }
    
    private void ChargedSwingAnimationStartedTrigger()
    {
        var playerTransform = Player.Instance.transform;

        particleOutwardSplash.transform.position = playerTransform.position;
        particleOutwardSplash.Restart();
        CameraManager.Instance.CameraShake(.2f, 4f);
        PostProcessingManager.Instance.CAImpulse();
        
        chargedSlash.transform.position = playerTransform.position + chargedSlashOffset;
        chargedSlash.transform.rotation = Quaternion.Euler(new Vector3(0, playerTransform.rotation.eulerAngles.y + 180, 180));
        chargedSlash.Restart();
    }
    
    private void DashAnimationStartedTrigger()
    {
        var playerTransform = Player.Instance.transform;
        
        dashSmokePuff.transform.position = playerTransform.position + dashSmokePuffOffset;
        dashSmokePuff.transform.rotation = Quaternion.Euler(new Vector3(0, playerTransform.rotation.eulerAngles.y + 143, 0));
        
        dashSmokePuff.Play();
    }

    private void RunOneStepTrigger()
    {
        var playerTransform = Player.Instance.transform;
        
        runSmokePuff.transform.position = playerTransform.position + runSmokePuffOffset;
        runSmokePuff.transform.rotation = Quaternion.Euler(new Vector3(0, playerTransform.rotation.eulerAngles.y + 90, 0));
        runSmokePuff.Play();
        FMODUnity.RuntimeManager.PlayOneShot("event:/Character/Player/PlayerFootsteps", transform.position);
    }
}
