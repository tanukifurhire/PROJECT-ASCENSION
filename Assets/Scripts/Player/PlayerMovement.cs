using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    public PlayerInput Input { get; private set; }
    public Vector2 MovementInput { get; private set; }
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform target;
    private Vector2 currentInputVector;
    private Vector2 smoothVectorVelocity;
    public Transform MainCameraTransform { get; private set; }
    private Vector3 currentTargetRotation;
    private Vector3 timeToReachTargetRotation;
    private Vector3 dampedTargetRotationPassedTime;
    private Vector3 dampedTargetRotationCurrentVelocity;
    private Vector3 moveVel;
    private int moveVelClampDown = -1;
    private int moveVelClampUp = 1;

    public ref Vector3 CurrentTargetRotation
    {
        get
        {
            return ref currentTargetRotation;
        }
    }
    public ref Vector3 TimeToReachTargetRotation
    {
        get
        {
            return ref timeToReachTargetRotation;
        }
    }
    public ref Vector3 DampedTargetRotationPassedTime
    {
        get
        {
            return ref dampedTargetRotationPassedTime;
        }
    }
    public ref Vector3 DampedTargetRotationCurrentVelocity
    {
        get
        {
            return ref dampedTargetRotationCurrentVelocity;
        }
    }

    private void Awake()
    {
        MainCameraTransform = Camera.main.transform;
        Input = GetComponent<PlayerInput>();
    }
    // Start is called before the first frame update
    void Start()
    {
        timeToReachTargetRotation.y = 0.14f;
        timeToReachTargetRotation.x = 0.3f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        ReadMovementInput();
        float dist = Vector3.Distance(target.position, transform.position);
        if (Mathf.Abs(target.position.y - transform.position.y) >= dist/1.5f)
        {
            if (target.position.y < transform.position.y)
            {
                moveVelClampUp = 0;
            }

            else
            {
                moveVelClampDown = 0;
            }
        }
        else
        {
            moveVelClampDown = -1;
            moveVelClampUp = 1;
        }
    }

    private void FixedUpdate()
    {
        RotatePlayer();
    }

    private void RotatePlayer()
    {
        currentInputVector = Vector2.SmoothDamp(currentInputVector, MovementInput, ref smoothVectorVelocity, 0.3f);
        Vector3 facingDirection = new Vector3(currentInputVector.x, 0f, 0.002f);
        Vector3 movementDirection = new Vector3(0.006f, currentInputVector.y, 0f);
        Rotate(facingDirection);
        RotateX(movementDirection);
        RotateTowardsTargetRotation();
        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
        moveVel = transform.forward;
        //moveVel += transform.up * Mathf.Clamp(MovementInput.y, moveVelClampDown, moveVelClampUp) * 2f;
        rb.AddForce(moveVel * 5f - currentPlayerHorizontalVelocity, ForceMode.Impulse);
    }
    protected Vector3 GetPlayerHorizontalVelocity()
    {
        Vector3 playerHorizontalVelocity = rb.velocity;

        return playerHorizontalVelocity;
    }
    private Vector3 GetTargetRotationDirection(float targetAngle)
    {
        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }

    private float Rotate(Vector3 direction)
    {
        float directionAngle = UpdateTargetRotation(direction);
        return directionAngle;
    }
    private float RotateX(Vector3 direction)
    {
        float directionAngle = UpdateTargetXRotation(direction);
        return directionAngle;
    }
    private float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
    {
        float directionAngle = GetDirectionAngle(direction);
        if (shouldConsiderCameraRotation)
        {
            directionAngle = AddCameraRotationToAngle(directionAngle);
        }
        if (directionAngle != CurrentTargetRotation.y)
        {
            UpdateTargetRotationData(directionAngle);
        }
         
        return directionAngle;
    }
    private float UpdateTargetXRotation(Vector3 direction, bool shouldConsiderCameraRotation = false)
    {
        float directionAngle = GetDirectionXAngle(direction);
        if (shouldConsiderCameraRotation)
        {
            directionAngle = AddCameraXRotationToAngle(directionAngle);
        }
        if (directionAngle != CurrentTargetRotation.x)
        {
            UpdateTargetXRotationData(directionAngle);
        }

        return directionAngle;
    }
    private void UpdateTargetRotationData(float targetAngle)
    {
        CurrentTargetRotation.y = targetAngle;
        DampedTargetRotationPassedTime.y = 0f;
    }
    private void UpdateTargetXRotationData(float targetAngle)
    {
        CurrentTargetRotation.x = targetAngle;
        DampedTargetRotationPassedTime.x = 0f;
    }
    private float GetDirectionAngle(Vector3 direction)
    {
        float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        if (directionAngle < 0f)
        {
            directionAngle += 360f;
        }
        return directionAngle;
    }
    private float GetDirectionXAngle(Vector3 direction)
    {
        float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return directionAngle;
    }
    private float AddCameraRotationToAngle(float angle)
    {
        angle += MainCameraTransform.eulerAngles.y;
        if (angle > 360f)
        {
            angle -= 360f;
        }
        return angle;
    }
    private float AddCameraXRotationToAngle(float angle)
    {
        angle -= MainCameraTransform.eulerAngles.x;
        
        return angle;
    }
    private void RotateTowardsTargetRotation()
    {
        float currentYAngle = rb.rotation.eulerAngles.y;
        float currentXAngle = rb.rotation.eulerAngles.x;

        if (currentYAngle == CurrentTargetRotation.y)
        {
            return;
        }

        float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, CurrentTargetRotation.y, ref DampedTargetRotationCurrentVelocity.y, timeToReachTargetRotation.y - DampedTargetRotationPassedTime.y);
        DampedTargetRotationPassedTime.y += Time.deltaTime;
        float smoothedXAngle = Mathf.SmoothDampAngle(currentXAngle, CurrentTargetRotation.x, ref DampedTargetRotationCurrentVelocity.x, timeToReachTargetRotation.x - DampedTargetRotationPassedTime.x);
        DampedTargetRotationPassedTime.x += Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(smoothedXAngle, smoothedYAngle, 0f);
        rb.MoveRotation(targetRotation);
    }

    private void ReadMovementInput()
    {
        MovementInput = new Vector2(Input.PlayerActions.Look.ReadValue<Vector2>().x, -Input.PlayerActions.Look.ReadValue<Vector2>().y).normalized * Time.deltaTime;

        Debug.Log(MovementInput.y);
    }
}
