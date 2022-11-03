using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    [Header("Car wheels")]  // Assign wheel Colliders and transform through the inspector
    public WheelCollider frontLeft;
    public Transform wheelFL;
    public WheelCollider frontRight;
    public Transform wheelFR;
    public WheelCollider backLeft;
    public Transform wheelBL;
    public WheelCollider backRight;
    public Transform wheelBR;    

    [Header("Car body")]    // Assign a Gameobject representing the front of the car
    public Rigidbody CarRigidBody;
    public Transform carFront;

    [Header("General Parameters")]
    public float MaxSteeringAngle = 45;
    public float MaxSpeed = 150;
    public float CurveAngleThld = 15;
    public float MaxSpeedAtCurve = 100;
    public float BrakeForce = 5000;
    public float MovementTorque = 400;
    public bool EnableMovement = true;

    // Auxiliar local variables
    private float LocalMaxSpeed;

    protected void Start()
    {
        // Set center of mass to zero
        CarRigidBody.centerOfMass = Vector3.zero;
    }

    protected void FixedUpdate()
    {       
        UpdateWheels();
        Steer();
        Move();
    }

    /// <summary>
    ///  Updates the wheel's postion and rotation
    /// </summary>
    protected void UpdateWheels()
    {
        ApplyRotationAndPostion(frontLeft, wheelFL);
        ApplyRotationAndPostion(frontRight, wheelFR);
        ApplyRotationAndPostion(backLeft, wheelBL);
        ApplyRotationAndPostion(backRight, wheelBR);
    }

    /// <summary>
    /// Updates the wheel's postion and rotation
    /// </summary>
    /// <param name="targetWheel"></param>
    /// <param name="wheel"></param>
    protected void ApplyRotationAndPostion(WheelCollider targetWheel, Transform wheel)
    {
        targetWheel.ConfigureVehicleSubsteps(5, 12, 15);

        Vector3 pos;
        Quaternion rot;
        targetWheel.GetWorldPose(out pos, out rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }

    protected float GetSteeringAngle()
	{
        float steeringValue = Input.GetAxis("Horizontal");
        float steeringAngle = steeringValue * MaxSteeringAngle;
        return steeringAngle;
    }

    /// <summary>
    /// Applies steering to the Current waypoint
    /// </summary>
    protected void Steer()
    {
        // Get steering angle
        float steeringAngle = GetSteeringAngle();

        // Set direction wheels angle
        frontLeft.steerAngle = steeringAngle;
        frontRight.steerAngle = steeringAngle;

        // Adapt local maximum speed depending of the steering angle
        LocalMaxSpeed = (steeringAngle <= CurveAngleThld) ? MaxSpeed : MaxSpeedAtCurve;        
    }

    /// <summary>
    /// Apply brake torque 
    /// </summary>
    protected void Brake(float brakeRatio=1)
    {
        frontLeft.brakeTorque = brakeRatio * BrakeForce;
        frontRight.brakeTorque = brakeRatio * BrakeForce;
        backLeft.brakeTorque = brakeRatio * BrakeForce;
        backRight.brakeTorque = brakeRatio * BrakeForce;
    }

    protected void Accelerate(float accelRatio=1)
	{
        backRight.motorTorque = accelRatio * MovementTorque;
        backLeft.motorTorque = accelRatio * MovementTorque;
        frontRight.motorTorque = accelRatio * MovementTorque;
        frontLeft.motorTorque = accelRatio * MovementTorque;
    }

    protected float GetMovementMagnitude()
	{
        return Input.GetAxis("Vertical");
    }

    /// <summary>
    /// Moves the car forward and backward depending on the input
    /// </summary>
    protected void Move()
    {
        // If movement allowed
        if (EnableMovement)
        {
            // Reset brake and acceleration torque
            Brake(0);
            Accelerate(0);

            // Get movement magnitude
            float movementMagnitude = GetMovementMagnitude();

            // Compute average speed of wheels
            int wheelsSpeed = (int)((frontLeft.rpm + frontRight.rpm + backLeft.rpm + backRight.rpm) / 4);
            int absWheelsSpeed = Mathf.Abs(wheelsSpeed);

            // If movement direction is different from wheels speed, brake
            if ((movementMagnitude > 0 && wheelsSpeed < 0) || (movementMagnitude < 0 && wheelsSpeed > 0))
			{
                Brake(Mathf.Abs(movementMagnitude));    // Absolute braking
            }
            // Otherwise, normal acceleration
			else
			{
                // If speed below local maximum, accelerate
                if (absWheelsSpeed < LocalMaxSpeed)
                    Accelerate(movementMagnitude);
                // If speed is too high (with a 1/4 marging), brake
                else if (absWheelsSpeed > LocalMaxSpeed + (LocalMaxSpeed * 1 / 4))
                    Brake();
                // Otherwise, do nothing
            }            
        }
        // If movement disabled, brake
        else
            Brake();
    }


}
