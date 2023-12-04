using System.Collections;
using UnityEngine;
/// <summary>
/// Handles all movements of the player
/// </summary>
/// 
public partial class Player
{
    [Header("Player Controller")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float gravity = 1f;
    
    [SerializeField] private float turnSpeed = 30f;
    [SerializeField] private LayerMask raycastHit;

    private PlayerState state;
    private CharacterController controller;
    private Vector3 moveDir;
    private readonly Vector3 raycastOffset = new(0, 1f, 0);
    private const float FALLING_THRESHOLD = 2f;
    private const float MAX_FALL_RAYCAST = 200f;
    
    private readonly Quaternion rightRotation = Quaternion.Euler(new Vector3(0, 90, 0));

    private void Turn(Vector3 direction)
    {
        float dist = Vector3.Distance(transform.forward, direction);
        if(dist > 1.3f)
            transform.forward = Vector3.Lerp(transform.forward, direction, 2f * turnSpeed * Time.deltaTime);
        else
            transform.forward = Vector3.Lerp(transform.forward, direction, turnSpeed * Time.deltaTime);
    }
    
    /// move towards the last moved direction with specified speed, for a single frame
    public void Move(float speed, bool turn=true)
    {
        Vector3 velocity = Vector3.zero;
        if(speed != 0)
            velocity = speed * Time.deltaTime * moveDir;
        
        velocity.y = -gravity;
        controller.Move(velocity);

        if (turn)
            Turn(moveDir);
    }

    ///  Move with a specified speed and direction, for a single frame
    public void Move(float speed, Vector3 moveDirection, bool turn=true)
    {
        Vector3 velocity = Vector3.zero;
        if(speed != 0)
            velocity = speed * Time.deltaTime * moveDirection;
            
        velocity.y = -gravity;
        controller.Move(velocity);
        
        if (turn)
            Turn(moveDirection);
    }
    
    // Coroutine for move for a certain period of time with `speed`
    private IEnumerator MoveForCoroutine(float speed, float time, bool turn)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            Move(speed, turn);
            yield return null;
        }
    }

    // overload of MoveFor, specifying a direction `dir`
    private IEnumerator MoveForCoroutine(float speed, float time, Vector3 dir, bool turn)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            Move(speed, dir, turn);
            yield return null;
        }
    }
    
    /// Move the player for a certain time period and speed towards the last moved direction
    public void MoveFor(float speed, float time, bool turn=true)
    {
        StartCoroutine(MoveForCoroutine(speed, time, turn));
    }

    /// Move the player for a certain time period and speed towards `dir`
    public void MoveFor(float speed, float time, Vector3 dir, bool turn=true)
    {
        StartCoroutine(MoveForCoroutine(speed, time, dir, turn));
    }

    private IEnumerator DieDelayedCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Die();
    }
    
    private void HandleFall()
    {
        // if nothing down beneath -> die!
        // TODO: this may not be the best way to implement death on falling for too long -- should keep counter for falling time instead
        if (!Physics.Raycast(transform.position + raycastOffset, Vector3.down, out RaycastHit hit, MAX_FALL_RAYCAST,
                raycastHit))
        {
            state = PlayerState.Dead;
            StartCoroutine(DieDelayedCoroutine());
            return;
        }

        Move(0);
        
        if (hit.distance > FALLING_THRESHOLD)
        { 
            state = PlayerState.Falling;
        }
        else
        {
            if (IsFalling())
            {
                ResetStateAfterAction();
            }
        }
    }
    
    // called when player is either moving or idle
    private void HandleMovement()
    {
        Vector3 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        if (inputVector == Vector3.zero)
        {
            if(!IsFalling())
                state = PlayerState.Idle;
            return;
        }
        
        // calculates orthographic camera angle
        Vector3 forward = virtualCamera.transform.forward;
        forward.y = 0;
        Vector3 right = rightRotation * forward;
        forward *= inputVector.z;
        right *= inputVector.x;
        moveDir = (forward + right).normalized;
        
        // move
        if (!GameInput.Instance.IsShiftModifierOn())
        {
            state = PlayerState.Walking;
            Move(walkSpeed);
        }
        else
        {
            state = PlayerState.Running;
            Move(runSpeed);
        }
    }
}