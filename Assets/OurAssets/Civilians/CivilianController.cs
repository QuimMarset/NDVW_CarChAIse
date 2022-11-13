using UnityEngine;

/*
 * 
< 0.85 actiu ABS
max steering 60 (-60, 60)
movement drection -> -1 brake 1
Canviar stifness (10 o 20 grip gran)
 * 
 * */

public class CivilianController : MonoBehaviour
{
    [Header("Car Wheels (Wheel Collider)")]
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider backLeft;
    public WheelCollider backRight;

    [Header("Car Wheels (Transform)")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelBL;
    public Transform wheelBR;

    public int MaxSteeringAngle = 45;
    public int MaxRPM = 150;
    private float LocalMaxSpeed;
    private float MovementTorque = 1;

    [SerializeField]
    private Vector3 targetPosition;
    
    [SerializeField]
    private bool stopForCollision = false;
    [SerializeField]
    private bool stopForTrafficLight = false;
    [SerializeField]
    private Transform forward;
    [SerializeField]
    private float raycastLength;

    public bool Stop
    {
        get { return stopForCollision || stopForTrafficLight; }
    }

    public void GetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    public void GetStopForTrafficLight(bool stop)
    {
        stopForTrafficLight = stop;
    }

    private void Update()
    {
        CheckForCollisions();
    }

    private void FixedUpdate()
    {
        UpdateWheels();
        ApplySteering();
        Movement();
    }

    private void CheckForCollisions()
    {
        stopForCollision = Physics.Raycast(forward.transform.position, transform.forward, raycastLength, 1 << gameObject.layer);
    }

    private void ApplyRotationAndPostion(WheelCollider targetWheel, Transform wheel)
    {
        targetWheel.ConfigureVehicleSubsteps(5, 12, 15);
        Vector3 pos;
        Quaternion rot;
        targetWheel.GetWorldPose(out pos, out rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }

    private void UpdateWheels()
    {
        ApplyRotationAndPostion(frontLeft, wheelFL);
        ApplyRotationAndPostion(frontRight, wheelFR);
        ApplyRotationAndPostion(backLeft, wheelBL);
        ApplyRotationAndPostion(backRight, wheelBR);
    }

    private void ApplyBrakes()
    {
        frontLeft.brakeTorque = 5000;
        frontRight.brakeTorque = 5000;
        backLeft.brakeTorque = 5000;
        backRight.brakeTorque = 5000;
    }

    private void RemoveBrakes()
    {
        frontLeft.brakeTorque = 0;
        frontRight.brakeTorque = 0;
        backLeft.brakeTorque = 0;
        backRight.brakeTorque = 0;
    }

    private int GetSpeedOfWheels()
    {
        return (int)((frontLeft.rpm + frontRight.rpm + backLeft.rpm + backRight.rpm) / 4);
    }

    private void ApplyMotorTorque()
    {
        backRight.motorTorque = 400 * MovementTorque;
        backLeft.motorTorque = 400 * MovementTorque;
        frontRight.motorTorque = 400 * MovementTorque;
        frontLeft.motorTorque = 400 * MovementTorque;
    }

    private void RemoveMotorTorque()
    {
        backRight.motorTorque = 0;
        backLeft.motorTorque = 0;
        frontRight.motorTorque = 0;
        frontLeft.motorTorque = 0;
    }

    void ApplySteering()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(targetPosition);
        float SteeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;
        if (SteeringAngle > 15) LocalMaxSpeed = 100;
        else LocalMaxSpeed = MaxRPM;

        frontLeft.steerAngle = SteeringAngle;
        frontRight.steerAngle = SteeringAngle;
    }

    void Movement()
    {
        if (Stop)
        {
            RemoveMotorTorque();
            ApplyBrakes();
        }
        else
        {
            RemoveBrakes();
            int speedOfWheels = GetSpeedOfWheels();
            if (speedOfWheels < LocalMaxSpeed)
            {
                ApplyMotorTorque();
            }
            else if (speedOfWheels < LocalMaxSpeed + (LocalMaxSpeed * 1 / 4))
            {
                RemoveMotorTorque();
            }
            else
            {
                ApplyBrakes();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(forward.position, forward.position + transform.forward * raycastLength);
    }
}
