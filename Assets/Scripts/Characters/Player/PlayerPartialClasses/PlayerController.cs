using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Player
{
    [Header("---------- Player Controller ---------- ")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float turnSpeed = 30f;

    [SerializeField] private float fallTimerMax = 2f;
    private float fallTimer;

    private PlayerState state;
    private Vector3 moveDir;
    
    [SerializeField] private float gravity = 1f;
    private float currentGravity = 5f;
    
    // constants
    private readonly Quaternion rightRotation = Quaternion.Euler(new Vector3(0, 90, 0));

    private void Turn(Vector3 direction)
    {
        float dist = Vector3.Distance(transform.forward, direction);
        if (dist > 1.3f)
            transform.forward = direction;
        else
            transform.forward = Vector3.Lerp(transform.forward, direction, turnSpeed * Time.deltaTime);
    }
    
    /// move towards the last moved direction with specified speed, for a single frame
    public void Move(float speed, bool turn=true)
    {
        Vector3 velocity = Vector3.zero;
        if(speed != 0)
            velocity = speed * Time.deltaTime * moveDir;
        
        velocity.y = -currentGravity;
        controller.Move(velocity);

        if (turn && moveDir != Vector3.zero)
            Turn(moveDir);
    }

    ///  Move with a specified speed and direction, for a single frame
    public void Move(float speed, Vector3 moveDirection, bool turn=true)
    {
        moveDir = moveDirection;
        Move(speed, turn);
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
    
    private void HandleFall()
    {
        if (controller.isGrounded)
        {
            currentGravity = 1f;
            fallTimer = 0;
            if (IsFalling())
                ResetStateAfterAction();
        }
        else
        {
            currentGravity += gravity * Time.deltaTime;
            fallTimer += Time.deltaTime;
            
            if (!IsFalling())
            {
                state = PlayerState.Falling;
                currentGravity = 0.01f;
                fallTimer = 0;
            }
            else
            {
                if (fallTimer > fallTimerMax)
                {
                    state = PlayerState.Dead;
                    Die();
                }
            }
        }

        Move(0);
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
        // if (!GameInput.Instance.IsShiftModifierOn())
        // {
        //     state = PlayerState.Walking;
        //     Move(walkSpeed);
        // }
        // else
        // {
            state = PlayerState.Running;
            Move(runSpeed);
        // }
    }
}