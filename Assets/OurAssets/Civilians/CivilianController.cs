using UnityEngine;
using UnityEngine.AI;

/*
 * 
< 0.85 actiu ABS
max steering 60 (-60, 60)
movement drection -> -1 brake 1
Canviar stifness (10 o 20 grip gran)
 * 
 * */

public class CivilianController : CarController2
{

    [SerializeField]
    protected Vector3 targetPosition;

    [SerializeField]
    private float turnRadius = 6f;

    private float MaxSpeedAtCurve;
    private float MaxSpeedOriginal;

    protected override void Start()
    {
        base.Start();
        MaxSpeedOriginal = MaxSpeed;
        MaxSpeedAtCurve = MaxSpeed / 2.0f;
    }


    public void SetStopFlag(bool stop)
    {
        EnableMovement = !stop;
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    protected override float GetMovementDirection()
    {
        return 1.0f;
    }

    protected override float GetSteeringAngle()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(targetPosition);
        relativeVector /= relativeVector.magnitude;
        float steeringAngle = (relativeVector.x / relativeVector.magnitude) * 2;
        return steeringAngle;
    }

    protected override void Steer()
    {
        float steeringAngle = GetSteeringAngle();

        if (steeringAngle > 0)
        {
            WheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (turnRadius + (1.5f / 2))) * steeringAngle;
            WheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (turnRadius - (1.5f / 2))) * steeringAngle;
        }
        else if (steeringAngle < 0)
        {
            WheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (turnRadius - (1.5f / 2))) * steeringAngle;
            WheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (turnRadius + (1.5f / 2))) * steeringAngle;
        }
        else
        {
            WheelColliders[0].steerAngle = 0;
            WheelColliders[1].steerAngle = 0;
        }

        UpdateWheels();
    }

    protected override void Move()
    {
        float steeringAngle = GetSteeringAngle();
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
}
