using UnityEngine;
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
    
    private Animator animator;
    private const string WALK = "PlayerWalk";
    private const string IDLE = "PlayerIdle";
    private const string FALL = "PlayerFall";
    private const string RUN = "PlayerRun";
    private int WALK_ID;
    private int IDLE_ID;
    private int FALL_ID;
    private int RUN_ID;
    
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        WALK_ID  = Animator.StringToHash(WALK);
        IDLE_ID =  Animator.StringToHash(IDLE);
        FALL_ID =  Animator.StringToHash(FALL);
        RUN_ID = Animator.StringToHash(RUN);
    }

    private void Update()
    {
        animator.SetBool(WALK_ID, Player.Instance.IsWalking());
        animator.SetBool(IDLE_ID, Player.Instance.IsIdle());
        animator.SetBool(FALL_ID, Player.Instance.IsFalling());
        animator.SetBool(RUN_ID, Player.Instance.IsRunning());
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

    private void Swing1AnimationStartedTrigger()
    {
        var playerTransform = Player.Instance.transform;
        
        normalSlash.transform.position = playerTransform.position + normalSlashOffset;
        normalSlash.transform.rotation = Quaternion.Euler(new Vector3(0, playerTransform.rotation.eulerAngles.y + 180, 180));
        normalSlash.Restart();
    }
    
    // the reverse swing
    private void Swing2AnimationStartedTrigger()
    {
        var playerTransform = Player.Instance.transform;
        
        normalSlash.transform.position = playerTransform.position + normalSlashOffset;
        normalSlash.transform.rotation = Quaternion.Euler(new Vector3(0, playerTransform.rotation.eulerAngles.y + 180, 0));
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
        
        // TODO: play sound of one step
    }
}
