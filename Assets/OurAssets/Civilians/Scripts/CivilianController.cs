using UnityEngine;


[RequireComponent(typeof(ObstacleAvoidanceBehavior))]
public class CivilianController : CarController
{
    private ObstacleAvoidanceBehavior obstacleAvoidanceBehavior;

    [SerializeField]
    protected Vector3 targetPosition;

    private float MaxSpeedAtCurve;
    private float MaxSpeedOriginal;

    [SerializeField]
    private float turnSpeed;
    [SerializeField]
    private float steeringAngle;

    [SerializeField]
    private bool movingBackwards;

    private void Awake()
    {
        obstacleAvoidanceBehavior = GetComponent<ObstacleAvoidanceBehavior>();
    }

    protected override void Start()
    {
        base.Start();
        movingBackwards = false;
        MaxSpeedOriginal = MaxSpeed;
        MaxSpeedAtCurve = MaxSpeed / 2.0f;
    }

    public void StopCar()
    {
        EnableMovement = false;
    }

    public void SetNormalDriving()
    {
        ResumeMovement();
        DisableObstacleAvoidance();
    }

    public void SetAvoidanceDriving()
    {
        ResumeMovement();
        EnableObstacleAvoidance();
    }

    private void ResumeMovement()
    {
        EnableMovement = true;
    }

    public void ChangeDestination(Vector3 newDestination)
    {
        targetPosition = newDestination;
    }

    private void EnableObstacleAvoidance()
    {
        obstacleAvoidanceBehavior.enabled = true;
    }

    private void DisableObstacleAvoidance()
    {
        obstacleAvoidanceBehavior.enabled = false;
    }

    public void SetBackwardMovement()
    {
        movingBackwards = true;
    }

    public void SetForwardMovement()
    {
        movingBackwards = false;
    }

    public bool IsMovingBackwards()
    {
        return movingBackwards;
    }

    protected override float GetMovementDirection()
    {
        if (movingBackwards)
        {
            return -1.0f;
        }
        return 1.0f;
    }

    protected override float GetSteeringAngle()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(targetPosition);
        float steeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;
        if (movingBackwards)
        {
            steeringAngle = -steeringAngle;
        }
        return steeringAngle;
    }

    private float GetAvoidSteeringAngle()
    {
        return MaxSteeringAngle * obstacleAvoidanceBehavior.SteerDirection;
    }

    protected override void Steer()
    {
        if (obstacleAvoidanceBehavior.enabled && obstacleAvoidanceBehavior.ObstacleDetected(CarFront, targetPosition))
        {
            steeringAngle = GetAvoidSteeringAngle();
        }
        else
        {
            steeringAngle = GetSteeringAngle();
        }
        LerpToTargetAngle(steeringAngle);
        UpdateWheels();
    }

    protected override void Move()
    {
        if (steeringAngle > 15f)
        {
            MaxSpeed = MaxSpeedAtCurve;
        }
        else
        {
            MaxSpeed = MaxSpeedOriginal;
        }
        base.Move();
    }


    private void LerpToTargetAngle(float targetSteerAngle)
    {
        targetSteerAngle = Mathf.Clamp(targetSteerAngle, -MaxSteeringAngle, MaxSteeringAngle);
        WheelColliders[0].steerAngle = Mathf.Lerp(WheelColliders[0].steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        WheelColliders[1].steerAngle = Mathf.Lerp(WheelColliders[1].steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}
