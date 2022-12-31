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
        Debug.Log(moveVelClampDown);
        //transform.LookAt(target.transform);
    }

    private void FixedUpdate()
    {
        RotatePlayer();
    }

    private void RotatePlayer()
    {
        Vector3 movementDirection = new Vector3(MovementInput.x, 0f, Mathf.Abs(MovementInput.y)).normalized;

        Rotate(movementDirection);
        RotateTowardsTargetRotation();
        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
        moveVel = transform.forward;
        //moveVel += transform.up * Mathf.Clamp(MovementInput.y, moveVelClampDown, moveVelClampUp) * .5f;
        rb.AddForce(moveVel * 5f - currentPlayerHorizontalVelocity, ForceMode.VelocityChange);
    }
    protected Vector3 GetPlayerHorizontalVelocity()
    {
        Vector3 playerHorizontalVelocity = rb.velocity;

        playerHorizontalVelocity.y = 0f;

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
    private void UpdateTargetRotationData(float targetAngle)
    {
        CurrentTargetRotation.y = targetAngle;
        DampedTargetRotationPassedTime.y = 0f;
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
    private float AddCameraRotationToAngle(float angle)
    {
        angle += MainCameraTransform.eulerAngles.y;
        if (angle > 360f)
        {
            angle -= 360f;
        }
        return angle;
    }
    private void RotateTowardsTargetRotation()
    {
        float currentYAngle = rb.rotation.eulerAngles.y;

        if (currentYAngle == CurrentTargetRotation.y)
        {
            return;
        }

        float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, CurrentTargetRotation.y, ref DampedTargetRotationCurrentVelocity.y, timeToReachTargetRotation.y - DampedTargetRotationPassedTime.y);
        DampedTargetRotationPassedTime.y += Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);
        rb.MoveRotation(targetRotation);
    }

    private void ReadMovementInput()
    {
        MovementInput = new Vector2(Input.PlayerActions.Look.ReadValue<Vector2>().x, Input.PlayerActions.Look.ReadValue<Vector2>().y).normalized;
    }
}
