using System.Collections;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{
    public bool DEBUG = false;
    [Header("Target Settings")]
    [Tooltip("The Transform of the player.")]
    public Transform TargetPlayer;

    [Header("Cover and Patrol")]
    [SerializeField] [Tooltip("Patrol points for enemy movement.")]
    private Transform[] PatrolPoints;
    [SerializeField] [Tooltip("Time to wait at each patrol point.")]
    private float patrolWaitTime = 3f;
    private int currentPatrolIndex = 0;
    private bool switchingPatrol = false;

    [Header("Attack Parameters")]
    [Tooltip("Attack range of the enemy.")]
    public float AttackRange = 10f;
    [Tooltip("Damage inflicted by the enemy.")]
    public float Damage = 10.0f;
    [Tooltip("Time between enemy attacks.")]
    public float TimeBetweenAttacks = 1.0f;
    [SerializeField] [Tooltip("Minimum distance from the player.")]
    private float MinPlayerDistance = 5f;
    [SerializeField] [Tooltip("Minimum height of obstacles for cover.")]
    private float MinObstacleHeight = 1.25f;
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
    public float WaitAttackRange = 2f;
    [Header("Attack Cooldown")]
    [SerializeField] [Tooltip("Range of random cooldowns between attacks.")]
    private Range<float, float> AttackCooldownRange = new Range<float, float>(1.0f, 2.0f);
    [SerializeField] private float CurrentAttackCooldown = 0f;
    [SerializeField]private float AttackCooldownTimer = 0f;
    [SerializeField]private bool CanAttack = false;
    [SerializeField] private float noReturnThreshold = 3f;
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
    [Tooltip("Rotation speed of the laser.")]
    public float rotationSpeed = 2f;

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

    [SerializeField ] private bool isAttacking;
    [SerializeField ] private bool AttackStarted = false;
    [SerializeField ]  private float lastAttackTime = -1;
    [SerializeField ] private float lastSpottedTime = 0f;
    [SerializeField ]  private float LoseInterestTimer = 8f;
    [SerializeField ] private bool moveToNextPatrolPoint = true;
    public float maxAttackTime;
    public float ddTest = 2f;
    // Nested class for Range
    public struct Range<T, U>
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

        Debug.Log("Point Reached");
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

    public void TransitionToAttack()
    {
        if (IsPlayerInSight() && IsPlayerInRadius(lookRadiusHiding))
        {
            Invoke("AttackCooldown", CurrentAttackCooldown);
            currentState = EnemyState.Attack;
            Agent.isStopped = false;
        }
    }
    public bool IsPlayerInRadius(float radius)
    {
        return Vector3.Distance(transform.position, TargetPlayer.position) < radius;
    }
    bool IsPlayerInSight()
    {
        // check if there is LOS to the player
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (TargetPlayer.position - transform.position).normalized, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.layer == 7)
            {
                return true;
            }
        }

        return false;
    }
    private void AttackBehavior()
    {
        if (isAttacking) return;
        Agent.speed = attackRunSpeed;
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
            Agent.isStopped = false;
            currentState = EnemyState.Idle;
        }
        // go back to idle if the player manages to get too far away
        if (Vector3.Distance(transform.position, TargetPlayer.position) > lookRadiusHiding)
        {
            currentState = EnemyState.Idle;
            Agent.isStopped = false;
            return;
        }

        Debug.Log(Vector3.Distance(TargetPlayer.position, transform.position) < WaitAttackRange);
        Debug.Log(IsPlayerInSight());
        if(Vector3.Distance(TargetPlayer.position, transform.position) < WaitAttackRange && IsPlayerInSight())
        {
            Debug.Log("God dammit ima say a slur");
            Agent.SetDestination(transform.position);
            FaceTarget();
            Agent.isStopped = true;
            
        }
        else
        {
            
            Agent.SetDestination(TargetPlayer.position);
            Agent.isStopped = false;
            return;
        }
        Agent.speed = attackRunSpeed;
        if(CanAttack)
        {
            Debug.Log("AttackBehavior");
            StartCoroutine(PeekAndAttack());
        }
    }


    void FaceTarget()
    {
        Vector3 direction = (TargetPlayer.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
        private IEnumerator PeekAndAttack()
        {
            isAttacking = true;
            FaceTarget();
            AttackStarted = true;
            RaycastHit hit;
            float timer = 0;
            while (timer < 0.75f)
            {
                FaceTarget();
                timer += Time.deltaTime;
                yield return null;
            }
            //CameraManager.Instance.CameraShake(WarningTime, 5f);
            bool isReturning = false;
            Agent.isStopped = false;
            Vector3 initialPos = transform.position;
            Vector3 initialPlayerPos = TargetPlayer.position;
            float initialAgentSpeed = Agent.speed;
            Agent.speed = 20f;
            Agent.angularSpeed = 0f;
            Agent.SetDestination(initialPlayerPos);
            float attackTimer = 0f;
            while(Vector3.Distance(transform.position, initialPos) > 0.5 /*Vector3.Distance*/ || !isReturning)
            {
                if (Vector3.Distance(initialPlayerPos, TargetPlayer.position) > noReturnThreshold)
                {
                    Debug.Log("Not Returning");
                    break;
                }
                Debug.Log("returnState " + isReturning);
                attackTimer += 0.2f;
                
                FaceTarget();
                if (!isReturning && Vector3.Distance(transform.position, initialPlayerPos) < ddTest)
                {
                    if(DEBUG) Debug.Log("u are about to experience a whooping like no other, stores will sell out of the auotbiography of the experience, ur body will never recover to it's original state, you will forever show grace to the alpha");
                    isReturning = true;
                }
                if(isReturning)
                {
                    FaceTarget();
                    
                    if(DEBUG) Debug.Log(
                        "return");
                    Agent.SetDestination(initialPos);
                }
                if(attackTimer > maxAttackTime) break;
                yield return new WaitForSeconds(0.2f);
            }
    
            // reset attack
            AttackStarted = false;
            isAttacking = false;
            CanAttack = false;
            Agent.angularSpeed = 120f;
            CurrentAttackCooldown = Random.Range(AttackCooldownRange.Lower, AttackCooldownRange.Upper);
            Invoke("AttackCooldown", CurrentAttackCooldown);
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == 7 && isAttacking)
            {
                GameEventsManager.Instance.PlayerEvents.TakeDamage((int)damage, transform.position);
            }
        }
        private void JumpAttack(Vector3 initalDirection)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, TargetPlayer.position);

            // Check if the player is within attack range
            if (distanceToPlayer < 6)
            {
                // Move towards the player quickly
                Vector3 directionToPlayer = (TargetPlayer.position - transform.position).normalized;
                Agent.Move(directionToPlayer * 2 * Time.deltaTime);

                // Optional: Add code here for the attack (e.g., animation, damage application)

                // Retreat
                Vector3 retreatDirection = (transform.position - TargetPlayer.position).normalized;
                Vector3 retreatPosition = transform.position + retreatDirection * 2;
                Agent.SetDestination(retreatPosition);
            }
        }
    public void AttackCooldown()
    {
        CanAttack = true;
    }
    
    private void Peek()
    {
        Agent.SetDestination(TargetPlayer.position);
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