using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The Player monolith* class. The following are partial class declarations:
/// <code>PlayerAnimator</code> - handles most of VFX and animations
/// <code>PlayerAudio</code> - handles audio and SFXs
/// <code>PlayerCombat</code> - handles combat, and combat related VFX/Anims
/// <code>PlayerController</code> - handles movement and controls
/// <code>PlayerInteract</code> - handles interaction
///
/// The Player class implements the IPlayer interface, a public API for all player-related functions
/// </summary>

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public partial class Player : Damagable, IPlayer, IDataPersistence
{
    public static Player Instance { get; private set; }
    
    [Header("---------- Player Fields and Components ---------- ")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CharacterController controller;
    private Animator animator;
    private CapsuleCollider capsuleCollider;

    [Header("Shader Properties")]
    private const string PLAYER_POSITION = "_PlayerPosition";
    private int PLAYER_POSITION_ID;
    private const string DREAMSTATE_INDICATOR = "_FullScreenVoronoiColor";
    private int DREAMSTATE_INDICATOR_ID;

    [Header("Player Health/Stamina")]
    [SerializeField] private PlayerHealthSO playerHealthSO;
    [SerializeField] private PlayerStaminaSO playerStaminaSO;
    
    [Header("Player Stats")] 
    [SerializeField] private PlayerStatisticsSO dreamShardsSO;
    [SerializeField] private PlayerStatisticsSO dreamThreadsSO;
    
    [Header("Player Dream State")]
    public readonly int LucidThreshold = 2;
    public readonly int DeepThreshold = 5;
    private DreamState dreamState;

    [Header("Player Items")] 
    [SerializeField] private InventorySO playerInventory;
    
    #region IPlayer
    public bool IsWalking() => state == PlayerState.Walking;
    public bool IsIdle() => state == PlayerState.Idle;
    public bool IsCasting() => state == PlayerState.Casting;
    public bool IsDead() => state == PlayerState.Dead;
    public bool IsFalling() => state == PlayerState.Falling;
    public bool IsRunning() => state == PlayerState.Running;
    public bool IsKnockedBack() => state == PlayerState.KnockedBack;
    public float GetPlayerHitBoxHeight() => playerHitBoxHeight;
    public Transform GetTargeter() => rangedTargeter;
    public bool AddItem(CollectableItemSO item) => playerInventory.Add(item);
    public bool RemoveItem(CollectableItemSO item) => playerInventory.Remove(item);
    public void SetMaxHP(int x) => playerHealthSO.SetMaxHP(x);
    public void SetMaxStamina(float x) => playerStaminaSO.SetMaxStamina(x);
    
    #endregion
    
    private void Awake()
    {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        state = PlayerState.Idle;
        dreamState = DreamState.Neutral;
        
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        
        AnimatorAwake();
        
        playerHealthSO.ResetHealth();
        playerStaminaSO.ResetStamina();
        
        dreamShardsSO.SetCurrencyCount(0);
        dreamThreadsSO.SetCurrencyCount(0);
        
        playerInventory.Init();
        
        InitializeAbilitiesAndCombos();
        
        // set initial material for weapon
        weaponFabricRenderer.material = originalWeaponFabricMaterial;
        CacheOnHitAnimationVariables();
    }

    private void Start()
    {
        PLAYER_POSITION_ID = Shader.PropertyToID(PLAYER_POSITION);
        DREAMSTATE_INDICATOR_ID = Shader.PropertyToID(DREAMSTATE_INDICATOR);            
        Shader.SetGlobalColor(DREAMSTATE_INDICATOR_ID, StaticInfoObjects.Instance.VORONOI_INDICATOR[dreamState]);

        InitializeOnHitRenderers();
        
        UpdateAbilities();
        
        GameInput.Instance.OnInteract += OnInteract;
        GameInput.Instance.OnAbilityCast += OnAbilityCast;
        
        GameEventsManager.Instance.PlayerEvents.OnTakeDamage += OnHit;
        GameEventsManager.Instance.PlayerEvents.OnHealthRegen += OnRegenDrowsiness;
        GameEventsManager.Instance.PlayerEvents.OnDreamShardsChange += OnDreamShardsChange;
        GameEventsManager.Instance.PlayerEvents.OnDreamThreadsChange += OnDreamThreadsChange;
    }

    private void OnDestroy()
    {
        GameInput.Instance.OnInteract -= OnInteract;
        GameInput.Instance.OnAbilityCast -= OnAbilityCast;
        
        GameEventsManager.Instance.PlayerEvents.OnTakeDamage -= OnHit;
        GameEventsManager.Instance.PlayerEvents.OnHealthRegen -= OnRegenDrowsiness;
        GameEventsManager.Instance.PlayerEvents.OnDreamShardsChange -= OnDreamShardsChange;
        GameEventsManager.Instance.PlayerEvents.OnDreamThreadsChange -= OnDreamThreadsChange;
    }
    
    private void Update()
    {
        AnimatorUpdate();
        
        if (SceneManager.GetActiveScene().name == "LoadingScene"
            || IsDead()
            || IsKnockedBack())
            return;
        
        Shader.SetGlobalVector(PLAYER_POSITION_ID, transform.position + Vector3.up * capsuleCollider.radius);
        
        HandleStamina();
        HandleAbilityCooldowns();
        HandleComboCooldowns();
        HandleFall();
        
        if (!IsCasting() && !IsFalling() && !IsDead())
        {
            HandleMovement();
        }
    }
    
    private void OnInteract()
    {
        Interact();
    }
    
    /// called when restoring drowsiness (hp)
    private void OnRegenDrowsiness(int value)
    {
        if(playerHealthSO.RegenHealth(value))
        {
            HandleDreamState();
            GameEventsManager.Instance.PlayerEvents.UpdateHealthBar();
        }
        else
        {
            // should not decrease potion
        }
    }
    
    // handle when currency change occurs
    private void OnDreamShardsChange(float val)
    {
        dreamShardsSO.Change(val);
        GameEventsManager.Instance.PlayerEvents.DreamShardsChangeFinished();
    }
    
    private void OnDreamThreadsChange(float val)
    {
        dreamThreadsSO.Change(val);
        GameEventsManager.Instance.PlayerEvents.DreamThreadsChangeFinished();
    }
    
    /// resets the player state to either Idle, Running, or Walking
    public void ResetStateAfterAction()
    {
        currentAbility = null;
        
        bool isMoving = GameInput.Instance.GetMovementVectorNormalized() != Vector3.zero;
        if (!isMoving)
        {
            state = PlayerState.Idle;
            return;
        }

        bool isRunning = GameInput.Instance.IsShiftModifierOn();
        if (isRunning)
            state = PlayerState.Running;
        else
            state = PlayerState.Walking;
    }
    
    /// Waits for `time`, if realTime=True, then real waits for `time`. Then resets the state. 
    public IEnumerator WaitForAndReset(float time, bool realTime=false)
    {
        if (realTime)
            yield return new WaitForSecondsRealtime(time);
        else
            yield return new WaitForSeconds(time);
        
        ResetStateAfterAction();
    }

    public void SetAnimatorTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void ResetAnimationTrigger(string triggerName)
    {
        animator.ResetTrigger(triggerName);
    }
    
    /// Called on every frame for stamina regen
    private void HandleStamina()
    {
        if(invulnerableTimer > 0)
            invulnerableTimer -= Time.deltaTime;

        if (playerStaminaSO.RegenStamina())
            GameEventsManager.Instance.PlayerEvents.UpdateStaminaBar();
    }
    
    /// Handles dream state changes
    private void HandleDreamState()
    {
        DreamState prevDreamState = dreamState;
        
        int currDrowsiness = playerHealthSO.CurrentDrowsiness;
        if (currDrowsiness < LucidThreshold)
            dreamState = DreamState.Lucid;
        else if (currDrowsiness > DeepThreshold)
            dreamState = DreamState.Deep;
        else
            dreamState = DreamState.Neutral;

        if (prevDreamState != dreamState)
            DreamStateTransition(prevDreamState);
    }
    
    /// Handles dream state transitions, sets fullscreen pass colors and plays animation
    private void DreamStateTransition(DreamState prevDreamState)
    {
        UpdateAbilities();
        PlayDreamStateChangeAnimation();
        Shader.SetGlobalColor(DREAMSTATE_INDICATOR_ID, StaticInfoObjects.Instance.VORONOI_INDICATOR[dreamState]);
    }
    
    /// Handles player death
    private void Die()
    {
        dreamShardsSO.OnDeath();
        dreamThreadsSO.OnDeath();
        GameEventsManager.Instance.PlayerEvents.DreamShardsChangeFinished();
        GameEventsManager.Instance.PlayerEvents.DreamThreadsChangeFinished();
        DataPersistenceManager.Instance.OnDeath();
        
        state = PlayerState.Dead;
        
        PlayDeathSound();
        PlayDeathAnimation();
    }

    // TODO: play anims
    public void Respawn()
    {
        // state = PlayerState.Idle;
    }
    
    #region IDataPersistence

    public void SaveCurrencyAndInventory(GameData data)
    {
        data.DreamShards = dreamShardsSO.GetCurrencyCount();
        data.DreamThreads = dreamThreadsSO.GetCurrencyCount();
        data.Inventory = playerInventory.ToSerializableInventory();
    }
    
    public void LoadData(GameData data)
    {
        if (Instance && Instance == this)
        {
            UpdateAbilities();
            
            dreamShardsSO.SetCurrencyCount(data.DreamShards);
            dreamThreadsSO.SetCurrencyCount(data.DreamThreads);
            transform.position = data.LastCheckPointPosition;
            playerInventory.FromSerializedInventory(data.Inventory);
        }
    }

    // TODO: dont update position unless its a checkpoint
    public void SaveData(GameData data)
    {
        // IMPORTANT: here we need to save the current scene, 
        // which was the last `targetScene` the loader had loaded
        SaveCurrencyAndInventory(data);
        data.LastCheckPointScene = Loader.TargetScene;
        data.LastCheckPointPosition = transform.position;
    }
    
    #endregion
}
