using System.Buffers.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class RangedEnemy : Enemy
{
    [Header("Target Settings")]
    [Tooltip("The Transform of the player.")]
    public Transform TargetPlayer;

    [Header("Cover and Patrol")]
    [SerializeField] [Tooltip("Possible cover points for the enemy.")]
    private CoverPoint[] coverPoints;
    [SerializeField] [Tooltip("Patrol points for enemy movement.")]
    private Transform[] PatrolPoints;
    [SerializeField] [Tooltip("Time to wait at each patrol point.")]
    private float patrolWaitTime = 3f;
    private int currentPatrolIndex = 0;
    private bool switchingPatrol = false;

    [Header("Attack Parameters")]
    [Tooltip("Attack range of the enemy.")]
    public float AttackRange = 10f;
    [SerializeField] [Tooltip("Damage inflicted by the enemy.")] private int laserDamage = 1;
    // [Tooltip("Time between enemy attacks.")]
    // public float TimeBetweenAttacks = 1.0f;
    [SerializeField] private float maximumLOS = 20f; 
    // [SerializeField] [Tooltip("Minimum distance from the player.")]
    // private float MinPlayerDistance = 5f;
    // [SerializeField] [Tooltip("Minimum height of obstacles for cover.")]
    // private float MinObstacleHeight = 1.25f;
    [SerializeField] [Tooltip("How far the enemy can see to decide on attacking.")]
    private float lookRadiusHiding = 50f;

    [Header("AI Settings")]
    [SerializeField] [Tooltip("The NavMeshAgent for the enemy.")]
    private NavMeshAgent Agent;
    [SerializeField] [Tooltip("Enemy layers to consider for AI decisions.")]
    private LayerMask EnemyLayers;
    [SerializeField] [Tooltip("Speed when patrolling.")]
    private float patrolWalkSpeed = 3.5f;
    [SerializeField] [Tooltip("Speed when attacking.")]
    private float attackRunSpeed = 4.5f;
    [SerializeField] [Tooltip("How often to update the slow update loop.")]
    private float slowUpdateTime = 0.5f;

    [Header("Attack Cooldown")]
    [SerializeField] [Tooltip("Range of random cooldowns between attacks.")]
    private Range<float, float> AttackCooldownRange = new Range<float, float>(1.0f, 2.0f);
    private float CurrentAttackCooldown = 0f;
    private float AttackCooldownTimer = 0f;
    private bool CanAttack = false;

    [Header("Visual Effects")]
    [SerializeField] [Tooltip("Line renderer for showing the warning line.")]
    private LineRenderer WarningLineRenderer;
    [SerializeField] [Tooltip("Particle system for laser attacks.")]
    private ParticleSystem LaserParticles;
    [SerializeField] [Tooltip("Particle system for impact effect.")]
    private ParticleSystem impactParticleSystem;
    [Tooltip("Time for the warning effect before an attack.")]
    public float WarningTime = 0.5f;

    [Header("Laser Direction and Speed")] 
    [Tooltip("If the laser does not hit anything, how far should it go?")]
    public float noHitLaserDistance = 100f;
    [Tooltip("Rotation speed of the laser.")]
    public float rotationSpeed = 2f;
    [SerializeField] [Tooltip("Initial direction of the laser.")]
    private Vector3 initialLaserDirection;
    [SerializeField] [Tooltip("Current direction of the laser.")]
    private Vector3 currentLaserDirection;
    [SerializeField] [Tooltip("Flag to check if initial laser direction is set.")]
    private bool initialDirectionSet = false;
    [Tooltip("Speed of lerp for laser aiming.")]
    public float lerpSpeed = 0.1f;

    [Header("Animation")]
    [Tooltip("Animator for the player.")]
    public Animator PlayerAnimator;

    [Header("State Management")]
    [Tooltip("Current state of the enemy.")]
    public EnemyState currentState;

    [Header("Additional Components")]
    [SerializeField] [Tooltip("Line renderer for the laser attack.")]
    private LineRenderer LaserLineRenderer;
    [SerializeField] [Tooltip("Animator for the enemy.")]
    private Animator anim;
    [SerializeField] [Tooltip("Layers considered as player.")]
    private LayerMask PlayerLayers;
    [SerializeField] [Tooltip("Point from which the laser fires.")]
    private Transform LaserFirePoint;

    private bool isAttacking;
    private bool AttackStarted = false;
    private float lastAttackTime = -1;
    private float lastSpottedTime = 0f;
    private float LoseInterestTimer = 35f;
    private bool moveToNextPatrolPoint = true;

    // Nested class for Range
    private struct Range<T, U>
    {
        public T Lower;
        public U Upper;
        public Range(T first, U second)
        {
            Lower = first;
            Upper = second;
        }
    }

    // Enumeration for Enemy State
    public enum EnemyState { Idle, Attack }
    public override void Start()
    {
        base.Start();
        
        currentState = EnemyState.Idle;
        Invoke("SlowUpdate", slowUpdateTime);
        CurrentAttackCooldown = Random.Range(AttackCooldownRange.Lower, AttackCooldownRange.Upper);
        LaserParticles.Stop();
        LaserLineRenderer.enabled = false;
        TargetPlayer = Player.Instance.GetRangedTargeter();
    }
    public override void Update()
    {
        base.Update();
        Debug.DrawRay(transform.position, (TargetPlayer.position - transform.position), Color.red);
    }

    private void SlowUpdate()
    {
        if (gameObject.activeSelf == false) return;
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleBehavior();
                break;
            case EnemyState.Attack:
                AttackBehavior();
                break;
        }
        Invoke("SlowUpdate", slowUpdateTime);
    }

    protected override void HandleDeath()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Character/Enemy/EyeballDie", transform.position);
    }

    private void IdleBehavior()
    {
        TransitionToAttack();

        Agent.speed = patrolWalkSpeed;

        if (PatrolPoints.Length == 0) return;  // No patrol points defined

        if (moveToNextPatrolPoint)
        {
            Agent.SetDestination(PatrolPoints[currentPatrolIndex].position);
            moveToNextPatrolPoint = false;
            return; 
        }

        // Debug.Log("Point Reached");
        // If the enemy is close to the current patrol point
        if (!switchingPatrol && Agent.velocity == Vector3.zero)
        {
            switchingPatrol = true;
            Invoke("SwitchPatrolPoint", patrolWaitTime);
        }
    }

    void SwitchPatrolPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % PatrolPoints.Length;
        moveToNextPatrolPoint = true; // Set the flag to true here
        switchingPatrol = false;
    }

    private void TransitionToAttack()
    {
        if (IsPlayerInSight() && IsPlayerInRadius(lookRadiusHiding))
        {
            Invoke("AttackCooldown", CurrentAttackCooldown);
            currentState = EnemyState.Attack;
            Agent.isStopped = false;
        }
    }
    
    private bool IsPlayerInRadius(float radius)
    {
        return Vector3.Distance(transform.position, TargetPlayer.position) < radius;
    }
    
    // check if there is LOS to the player
    bool IsPlayerInSight()
    {
        return Physics.Raycast(transform.position, (TargetPlayer.position - transform.position).normalized,
                   out RaycastHit hit, maximumLOS)
               && hit.collider.gameObject.CompareTag("Player");
    }
    private void AttackBehavior()
    {
        if (IsPlayerInSight())
        {
            lastSpottedTime = 0f;
        }
        else
        {
            lastSpottedTime += slowUpdateTime;
        }
        if(lastSpottedTime > LoseInterestTimer)
        {
            currentState = EnemyState.Idle;
            return;
        }
        
        // go back to idle if the player manages to get too far away
        if (!IsPlayerInRadius(lookRadiusHiding))
        {
            currentState = EnemyState.Idle;
            Agent.isStopped = true;
            return;
        }
        
        Agent.speed = attackRunSpeed;
        CanAttack = IsPlayerInRadius(AttackRange);
        
        if (!isAttacking)
        {
            Agent.isStopped = false;
            FindCover(TargetPlayer.position + 0.5f * Vector3.up);
        }
        else if(!AttackStarted && CanAttack)
        {
            StartCoroutine(PeekAndAttack());
        }
    }

    
    void FaceTarget()
    {
        Quaternion lookRotation = Quaternion.LookRotation(TargetPlayer.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    
    private IEnumerator PeekAndAttack()
    {
        FaceTarget();
        AttackStarted = true;
        Agent.isStopped = true;
        anim.SetTrigger("Attack");
        
        // Physics.Raycast(transform.position, TargetPlayer.position - transform.position, out RaycastHit hit,
        //     noHitLaserDistance);
        
        Debug.DrawRay(transform.position, TargetPlayer.position - transform.position, Color.red);
        
        // while (!hit.collider.gameObject.CompareTag("Player"))
        // {
        //     FaceTarget();
        //     Physics.Raycast(transform.position, TargetPlayer.position - transform.position, out hit,
        //         noHitLaserDistance);
        //     Peek();
        //     
        //     yield return null;
        // }
        
        float timer = 0;
        while (timer < 0.75f)
        {
            FaceTarget();
            timer += Time.deltaTime;
            yield return null;
        }
        
        Vector3 updatedDirection = (TargetPlayer.position - transform.position).normalized;
        timer = 0;
        while (timer < 0.25f)
        {
            FaceTarget();
            timer += Time.deltaTime;
            yield return null;
        }
        impactParticleSystem.Play();
        
        // Attack Audio Play
        FMODUnity.RuntimeManager.PlayOneShot("event:/Character/Enemy/EyeballAttack", transform.position);
        CameraManager.Instance.CameraShake(WarningTime, 5f);
        
        // Enable the main attack laser and perform the attack
        LaserLineRenderer.enabled = true;
        
        timer = 0;
        while(timer < WarningTime)
        {
            FaceTarget();
            timer += Time.deltaTime;
            LaserAttack(updatedDirection); // Pass the updated direction to LaserAttack
            yield return null;
        }

        // reset attack
        LaserLineRenderer.enabled = false;
        AttackStarted = false;
        isAttacking = false;
        initialDirectionSet = false;
        CurrentAttackCooldown = Random.Range(AttackCooldownRange.Lower, AttackCooldownRange.Upper);
        impactParticleSystem.Stop();
        Invoke("AttackCooldown", CurrentAttackCooldown);
    }

    // private void Peek()
    // {
    //     Agent.SetDestination(TargetPlayer.position);
    // }
    
    private void LaserAttack(Vector3 initalDirection)
    {
        Vector3 targetDirection = (TargetPlayer.position - transform.position).normalized;

        // Set initial direction the first time the attack is initiated
        if(!initialDirectionSet)
        {
            initialLaserDirection = initalDirection;
            currentLaserDirection = initialLaserDirection;
            initialDirectionSet = true;
        }
        else
        {
            // Lerp the direction based on the initial direction
            currentLaserDirection = Vector3.Lerp(currentLaserDirection, targetDirection, lerpSpeed);
            currentLaserDirection = new Vector3(currentLaserDirection.x, 0, currentLaserDirection.z);
        }

        RaycastHit hit;
        // Debug.Log(PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dash"));
        if (Player.Instance.invulnerableTimer < 0.5f)
        {
            if (Physics.Raycast(transform.position, currentLaserDirection, out hit, Mathf.Infinity) )
            {
                impactParticleSystem.transform.position = hit.point;
                impactParticleSystem.transform.forward = -currentLaserDirection;
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    GameEventsManager.Instance.PlayerEvents.TakeDamage(laserDamage, transform.position);
                }
            
                LaserLineRenderer.SetPosition(0, LaserFirePoint.position);
                LaserLineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                impactParticleSystem.transform.position = transform.position + currentLaserDirection * noHitLaserDistance;
                LaserLineRenderer.SetPosition(0, LaserFirePoint.position);
                LaserLineRenderer.SetPosition(1, transform.position + currentLaserDirection * noHitLaserDistance);
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, currentLaserDirection, out hit, Mathf.Infinity, ~PlayerLayers) )
            {
                impactParticleSystem.transform.position = hit.point;
                impactParticleSystem.transform.forward = -currentLaserDirection.normalized;
                LaserLineRenderer.SetPosition(0, LaserFirePoint.position);
                LaserLineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                impactParticleSystem.transform.position = transform.position + currentLaserDirection * noHitLaserDistance;
                LaserLineRenderer.SetPosition(0, LaserFirePoint.position);
                LaserLineRenderer.SetPosition(1, transform.position + currentLaserDirection * noHitLaserDistance);
            }
        }
        
        
    }
    public void AttackCooldown()
    {
        isAttacking = true;
    }
    
    
    private void FindCover(Vector3 threatPosition)
    {
        CoverPoint bestCover = null;
        float closestDistance = float.MaxValue;

        foreach (var cover in coverPoints)
        {
            if (cover.IsCoverSafe(threatPosition, EnemyLayers, TargetPlayer.gameObject))
            {
                float distance = Vector3.Distance(transform.position, cover.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestCover = cover;
                }
            }
        }
        
        if (bestCover != null)
        {
            Agent.SetDestination(bestCover.transform.position);
        }
    }
    // draw gizmos for radii
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lookRadiusHiding);
    }
}