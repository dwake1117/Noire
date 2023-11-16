using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public partial class Player
{
    [Header("Player Controller")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float gravity = .1f;
    [SerializeField] private float turnSpeed = 30f;
    [SerializeField] private LayerMask raycastHit;

    private PlayerState state;
    private CharacterController controller;
    private Vector3 moveDir;
    private readonly Vector3 raycastOffset = new(0, 1f, 0);
    private readonly float fallingThreshold = 1.3f;
    
    private readonly Quaternion rightRotation = Quaternion.Euler(new Vector3(0, 90, 0));

    private void Turn(Vector3 direction)
    {
        float dist = Vector3.Distance(transform.forward, direction);
        if(dist > 1.3f)
            transform.forward = Vector3.Lerp(transform.forward, direction, 1.6f * turnSpeed * Time.deltaTime);
        else
            transform.forward = Vector3.Lerp(transform.forward, direction, turnSpeed * Time.deltaTime);
    }
    
    // move towards `moveDir` with speed
    public void Move(float speed)
    {
        Vector3 velocity = speed * Time.deltaTime * moveDir;
        velocity.y = -gravity;
        controller.Move(velocity);

        Turn(moveDir);
    }

    // Coroutine for move for a certain period of time with `speed`
    private IEnumerator MoveForCoroutine(float speed, float time)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            Move(speed);
            yield return null;
        }
    }

    // overload of MoveFor, specifying a direction `dir`
    private IEnumerator MoveForCoroutine(float speed, float time, Vector3 dir)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            Move(speed, dir);
            yield return null;
        }
    }
    
    // the actual public function to move the player for a certain time period
    public void MoveFor(float speed, float time)
    {
        StartCoroutine(MoveForCoroutine(speed, time));
    }

    // overload for `MoveFor`, specifying a direction `dir`
    public void MoveFor(float speed, float time, Vector3 dir)
    {
        StartCoroutine(MoveForCoroutine(speed, time, dir));
    }
    
    
    // overload of Move with moveDirection
    public void Move(float speed, Vector3 moveDirection)
    {
        Vector3 velocity = speed * Time.deltaTime * moveDirection;
        velocity.y = -gravity;
        controller.Move(velocity);

        Turn(moveDirection);
    }

    private void HandleFall()
    {
        if (Physics.Raycast(transform.position + raycastOffset, Vector3.down, out RaycastHit hit, Mathf.Infinity, raycastHit)) 
        {
            if (hit.distance > fallingThreshold)
            {
                state = PlayerState.Falling;
            }
            else
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
            Move(0); // handle fall
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