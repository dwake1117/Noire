using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The main partial class for Player
/// </summary>

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public partial class Player : Character, IPlayer, IDataPersistence
{
    [Header("Fields")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    public static Player Instance { get; private set; }
    private Animator animator;

    [Header("Player Health/Stamina")]
    [SerializeField] private PlayerHealthSO playerHealthSO;
    [SerializeField] private PlayerStaminaSO playerStaminaSO;
    
    [Header("Player Stats")] 
    [SerializeField] private PlayerStatisticsSO dreamShardsSO;
    [SerializeField] private PlayerStatisticsSO dreamThreadsSO;
    
    [Header("Player Dream State")]
    public readonly int LucidThreshold = 2;
    public readonly int DeepThreshold = 5;
    public DreamState DreamState { get; private set; }

    [Header("Player Items")] 
    [SerializeField] private InventorySO playerInventory;
    
    #region IPlayer
    public bool IsWalking() => state == PlayerState.Walking;
    public bool IsIdle() => state == PlayerState.Idle;
    public bool IsCasting() => state == PlayerState.Casting;
    public bool IsDead() => state == PlayerState.Dead;
    public bool IsFalling() => state == PlayerState.Falling;
    public bool IsRunning() => state == PlayerState.Running;
    public float GetPlayerHitBoxHeight() => playerHitBoxHeight;
    public Transform GetRangedTargeter() => rangedTargeter;
    public bool AddItem(CollectableItemSO item) => playerInventory.Add(item);
    public bool RemoveItem(CollectableItemSO item) => playerInventory.Remove(item);
    public void SetMaxHP(int x) => playerHealthSO.SetMaxHP(x);
    public void SetMaxStamina(float x) => playerStaminaSO.SetMaxStamina(x);
    
    #endregion
    
    #region EVENT FUNCTIONS
    
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
        DreamState = DreamState.Neutral;
        
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        
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
        InitializeOnHitRenderers();
        UpdateAbilities();
        
        Shader.SetGlobalColor("_FullScreenVoronoiColor", StaticInfoObjects.Instance.VORONOI_INDICATOR[DreamState]);
        
        GameInput.Instance.OnInteract += OnInteract;
        GameInput.Instance.OnAbilityCast += OnAbilityCast;
        
        GameEventsManager.Instance.PlayerEvents.OnTakeDamage += OnTakingDamage;
        GameEventsManager.Instance.PlayerEvents.OnHealthRegen += OnRegenDrowsiness;
        GameEventsManager.Instance.PlayerEvents.OnDreamShardsChange += OnDreamShardsChange;
        GameEventsManager.Instance.PlayerEvents.OnDreamThreadsChange += OnDreamThreadsChange;
    }

    private void OnDestroy()
    {
        GameInput.Instance.OnInteract -= OnInteract;
        GameInput.Instance.OnAbilityCast -= OnAbilityCast;
        
        GameEventsManager.Instance.PlayerEvents.OnTakeDamage -= OnTakingDamage;
        GameEventsManager.Instance.PlayerEvents.OnHealthRegen -= OnRegenDrowsiness;
        GameEventsManager.Instance.PlayerEvents.OnDreamShardsChange -= OnDreamShardsChange;
        GameEventsManager.Instance.PlayerEvents.OnDreamThreadsChange -= OnDreamThreadsChange;
    }
    
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
            return;
        if (IsDead())
            return;
        
        HandleStamina();
        HandleAbilityCooldowns();
        HandleComboCooldowns();
        
        if (!IsCasting())
        {
            HandleFall();
            HandleMovement();
        }
    }

    #endregion

    #region TRIGGER FUNCTIONS
    private void OnInteract()
    {
        Interact();
    }
    
    // called when restoring drowsiness (hp)
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
    #endregion
    
    #region HELPER SUBROUTINES
    
    // called after ability for state transition
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

    public IEnumerator WaitForAndReset(float time)
    {
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
    
    #endregion
    
    #region HANDLE FUNCTIONS

    // called on every frame for buffer regen
    private void HandleStamina()
    {
        if(invulnerableTimer > 0)
            invulnerableTimer -= Time.deltaTime;

        if (playerStaminaSO.RegenStamina())
            GameEventsManager.Instance.PlayerEvents.UpdateStaminaBar();
    }
    
    // called upon changing HP
    private void HandleDreamState()
    {
        DreamState prevDreamState = DreamState;
        
        int currDrowsiness = playerHealthSO.CurrentDrowsiness;
        if (currDrowsiness < LucidThreshold)
            DreamState = DreamState.Lucid;
        else if (currDrowsiness > DeepThreshold)
            DreamState = DreamState.Deep;
        else
            DreamState = DreamState.Neutral;

        if (prevDreamState != DreamState)
            HandleDreamStateTransition(prevDreamState);
    }
    
    // called when transitioning between dream states
    private void HandleDreamStateTransition(DreamState prevDreamState)
    {
        UpdateAbilities();
        Shader.SetGlobalColor("_FullScreenVoronoiColor", StaticInfoObjects.Instance.VORONOI_INDICATOR[DreamState]);
    }
    
    // called when drowsiness == 0
    private void HandleDeath()
    {
        dreamShardsSO.OnDeath();
        dreamThreadsSO.OnDeath();
        GameEventsManager.Instance.PlayerEvents.DreamShardsChangeFinished();
        GameEventsManager.Instance.PlayerEvents.DreamThreadsChangeFinished();
        
        state = PlayerState.Dead;
        Loader.Load(GameScene.DeathScene);
    }
    
    #endregion
    
    #region IDataPersistence
    
    /** IMPORTANT DEBUGGING INFORMATION:
     * if you get an error saying loading error or something in main scene,
     * please DISABLE DataPersistenceManager in scene/Globals or toggle on "Disable Data Persistence"
    */
    
    public void LoadData(GameData data)
    {
        if (Instance && Instance == this)
        {
            UpdateAbilities();
            
            dreamShardsSO.SetCurrencyCount(data.DreamShards);
            dreamThreadsSO.SetCurrencyCount(data.DreamThreads);
            transform.position = data.Position;
            playerInventory.FromSerializedInventory(data.Inventory);
        }
    }

    public void SaveData(GameData data)
    {
        // IMPORTANT: here we need to save the current scene, 
        // which was the last `targetScene` the loader had loaded
        data.CurrentScene = SceneManager.GetActiveScene().name;
        
        data.DreamShards = dreamShardsSO.GetCurrencyCount();
        data.DreamThreads = dreamThreadsSO.GetCurrencyCount();
        data.Position = transform.position;
        data.Inventory = playerInventory.ToSerializableInventory();
    }
    
    #endregion
}