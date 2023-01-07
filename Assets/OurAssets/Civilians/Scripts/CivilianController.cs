using UnityEngine;


[RequireComponent(typeof(ObstacleAvoidanceBehavior))]
public class CivilianController : CarController
{
    [Header("Civlian Parameters")]
    [SerializeField] public Vector3 targetPosition;
    [SerializeField] protected float turnSpeed;
    [SerializeField] protected float steeringAngle;
    [SerializeField] protected bool movingBackwards;

    protected ObstacleAvoidanceBehavior obstacleAvoidanceBehavior;
    protected float MaxSpeedAtCurve;
    protected float MaxSpeedOriginal;

    protected virtual void Awake()
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

    public virtual void StopCar()
    {
        EnableMovement = false;
    }

    public virtual void SetNormalDriving()
    {
        ResumeMovement();
        DisableObstacleAvoidance();
    }

    public virtual void SetAvoidanceDriving()
    {
        ResumeMovement();
        EnableObstacleAvoidance();
    }

    protected virtual void ResumeMovement()
    {
        EnableMovement = true;
    }

    public virtual void ChangeDestination(Vector3 newDestination)
    {
        targetPosition = newDestination;
    }

    protected virtual void EnableObstacleAvoidance()
    {
        obstacleAvoidanceBehavior.enabled = true;
    }

    protected virtual void DisableObstacleAvoidance()
    {
        obstacleAvoidanceBehavior.enabled = false;
    }

    public virtual void SetBackwardMovement()
    {
        movingBackwards = true;
    }

    public virtual void SetForwardMovement()
    {
        movingBackwards = false;
    }

    public virtual bool IsMovingBackwards()
    {
        return movingBackwards;
    }

    protected override float GetMovementDirection()
    {
        // Adjust maximum speed
        MaxSpeed = (steeringAngle > 15f)? MaxSpeedAtCurve : MaxSpeedOriginal;

        // Return movement direction
        return movingBackwards? -1.0f: 1.0f;
    }

    protected override float GetSteeringAngle()
    {
        float steeringAngle;

        if (obstacleAvoidanceBehavior.enabled && obstacleAvoidanceBehavior.ObstacleDetected(CarFront, targetPosition))
        {
            steeringAngle = MaxSteeringAngle * obstacleAvoidanceBehavior.SteerDirection;
        }
        else
        {
            Vector3 relativeVector = transform.InverseTransformPoint(targetPosition);
            steeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;
            if (movingBackwards)
                steeringAngle = -steeringAngle;
        }

        steeringAngle = LerpToTargetAngle(steeringAngle);

        return steeringAngle;
    }

    protected virtual float LerpToTargetAngle(float steeringAngle)
    {
        steeringAngle = Mathf.Clamp(steeringAngle, -MaxSteeringAngle, MaxSteeringAngle);
        return Mathf.Lerp(WheelColliders[0].steerAngle, steeringAngle, Time.deltaTime * turnSpeed);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, 1f);
    }
}
