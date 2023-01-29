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
    private Vector2 screenPosition;
    private Vector2 centeredScreenPosition;
    [SerializeField] private Transform centerOfMass;
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
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
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
        currentInputVector = Vector2.SmoothDamp(currentInputVector, centeredScreenPosition, ref smoothVectorVelocity, .1f);
        Debug.Log(currentInputVector);
        Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
        moveVel = transform.forward;
        rb.AddForce(moveVel * 5f - currentPlayerHorizontalVelocity, ForceMode.VelocityChange);
        rb.angularVelocity += -Vector3.up * GetAngularAcceleration() * Time.fixedDeltaTime;
        rb.angularVelocity += transform.right * (currentInputVector.y * 5000 * Mathf.PI / 180) * Time.fixedDeltaTime;
        rb.rotation *= Quaternion.Euler(0f,0f,-transform.localEulerAngles.z);
    }
    float GetAngularAcceleration()
    {
        return -currentInputVector.x * 9000 * Mathf.PI / 180;
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

        Quaternion targetRotation = Quaternion.Euler(smoothedXAngle, 0f, 0f);
        rb.MoveRotation(targetRotation);
    }

    private void ReadMovementInput()
    {
        screenPosition = new Vector2(Mathf.Clamp(Mouse.current.position.ReadValue().x / Screen.height, 0, 1), Mathf.Clamp(Mouse.current.position.ReadValue().y / Screen.height, 0, 1));
        centeredScreenPosition = new Vector2(Mouse.current.delta.ReadValue().x, -Mouse.current.delta.ReadValue().y * .2f).normalized;
    }
}
