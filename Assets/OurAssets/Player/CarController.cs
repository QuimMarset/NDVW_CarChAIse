using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System;

public abstract class CarController : MonoBehaviour
{
    [Header("Car wheels")]  // Assign wheel Colliders and transform through the inspector
    [SerializeField] protected WheelCollider FrontLeftC;
    [SerializeField] protected Transform FrontLeftT;
    [SerializeField] protected WheelCollider FrontRightC;
    [SerializeField] protected Transform FrontRightT;
    [SerializeField] protected WheelCollider BackLeftC;
    [SerializeField] protected Transform BackLeftT;
    [SerializeField] protected WheelCollider BackRightC;
    [SerializeField] protected Transform BackRightT;

    [Header("Car body")]    // Assign a Gameobject representing the front of the car
    [SerializeField] protected Rigidbody CarRigidBody;
    [SerializeField] protected Transform CarFront;

    [Header("General Parameters")]
    [SerializeField] protected bool EnableMovement = true;
    [SerializeField] protected bool EnableABS = true;
    [SerializeField] protected float ABSThld = 0.75f;
    [SerializeField] protected float MaxSteeringAngle = 45;
    [SerializeField] public float MaxSpeed = 200;
    [SerializeField] protected float BrakeTorque = 5000;
    [SerializeField] protected float MovementTorque = 1000;
    [SerializeField] protected bool LimitRpmAtCurve = true;
    [SerializeField] protected float CurveSteerAngleThld = 30;
    [SerializeField] protected float MaxSpeedAtCurve = 100;

    // Constants    
    protected const float MPS_TO_KPH = 3600f / 1000f;
    protected const float RPM_PER_RADIUS_TO_KPH = 1f / 60f * 2f * Mathf.PI;

    // Auxiliar local variables
    public float CurrentWheelsSpeed { get; protected set; }
    public float CurrentForwardSpeed { get; protected set; }
    protected float LocalMaxSpeed;
    

    protected virtual void Start()
    {
        // Set center of mass to zero
        CarRigidBody.centerOfMass = Vector3.zero;
    }

    protected virtual void FixedUpdate()
    {
        // Get speeds
        CurrentWheelsSpeed = GetWheelsSpeed();
        CurrentForwardSpeed = GetForwardSpeed();

        // Control
        Steer();
        Move();
    }

    /// <summary>
    ///  Updates the wheel's postion and rotation
    /// </summary>
    protected virtual void UpdateWheels()
    {
        ApplyRotationAndPostion(FrontLeftC, FrontLeftT);
        ApplyRotationAndPostion(FrontRightC, FrontRightT);
        ApplyRotationAndPostion(BackLeftC, BackLeftT);
        ApplyRotationAndPostion(BackRightC, BackRightT);
    }

    /// <summary>
    /// Updates the wheel's postion and rotation
    /// </summary>
    /// <param name="targetWheel"></param>
    /// <param name="wheel"></param>
    protected virtual void ApplyRotationAndPostion(WheelCollider targetWheel, Transform wheel)
    {
        targetWheel.ConfigureVehicleSubsteps(5, 12, 15);

        Vector3 pos;
        Quaternion rot;
        targetWheel.GetWorldPose(out pos, out rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }

    protected abstract float GetSteeringAngle();

    /// <summary>
    /// Applies steering to the Current waypoint
    /// </summary>
    protected virtual void Steer()
    {
        // Get steering angle
        float steeringAngle = GetSteeringAngle();

        // Set direction wheels angle
        FrontLeftC.steerAngle = steeringAngle;
        FrontRightC.steerAngle = steeringAngle;

        // If desired, adapt local maximum speed depending of the steering angle
        if (LimitRpmAtCurve)
            LocalMaxSpeed = (Mathf.Abs(steeringAngle) <= CurveSteerAngleThld) ? MaxSpeed : MaxSpeedAtCurve;
        else
            LocalMaxSpeed = MaxSpeed;

        UpdateWheels();
    }

    protected virtual float GetWheelsSpeed()
    {
        // Average wheels speed
        return (GetWheelSpeed(FrontLeftC) + GetWheelSpeed(FrontRightC) + GetWheelSpeed(BackLeftC) + GetWheelSpeed(BackRightC)) / 4;
    }

    protected virtual float GetWheelSpeed(WheelCollider wheel)
    {
        return wheel.rpm * wheel.radius * RPM_PER_RADIUS_TO_KPH;
    }

    protected virtual float GetForwardSpeed()
	{
        return Vector3.Dot(CarRigidBody.velocity, transform.forward) * MPS_TO_KPH;
    }

    /// <summary>
    /// Apply brake torque 
    /// </summary>
    protected virtual void Brake(float brakeRatio = 1)
    {
        // Check difference between forward speed and wheels speed
        if (EnableABS && brakeRatio > 0)
		{
            float absWheelsSpeed = Mathf.Abs(CurrentWheelsSpeed);
            float forwardSpeed = Vector3.Dot(CarRigidBody.velocity, transform.forward); // TODO: Check why this is 3.6 times greater than expected
            float slipRatio = absWheelsSpeed / forwardSpeed;
            if (slipRatio > 0.1 && slipRatio <= ABSThld)
                brakeRatio = 0;
        }

        FrontLeftC.brakeTorque = brakeRatio * BrakeTorque;
        FrontRightC.brakeTorque = brakeRatio * BrakeTorque;
        BackLeftC.brakeTorque = brakeRatio * BrakeTorque;
        BackRightC.brakeTorque = brakeRatio * BrakeTorque;
    }

    protected virtual void Accelerate(float accelRatio = 1)
    {
        BackRightC.motorTorque = accelRatio * MovementTorque;
        BackLeftC.motorTorque = accelRatio * MovementTorque;
        FrontRightC.motorTorque = accelRatio * MovementTorque;
        FrontLeftC.motorTorque = accelRatio * MovementTorque;
    }



    protected abstract float GetMovementDirection();

    /// <summary>
    /// Moves the car forward and backward depending on the input
    /// </summary>
    protected virtual void Move()
    {
        // If movement allowed
        if (EnableMovement)
        {
            // Reset brake and acceleration torque
            Brake(0);
            Accelerate(0);

            // Get movement magnitude
            float movementDirection = GetMovementDirection();

            // Compute speed of wheels
            float absWheelsSpeed = Mathf.Abs(CurrentWheelsSpeed);

            // If movement direction is different from wheels speed or it's -Inf, brake
            if (float.IsNegativeInfinity(movementDirection) || (movementDirection > 0 && CurrentWheelsSpeed < 0) || (movementDirection < 0 && CurrentWheelsSpeed > 0))
            {
                // When want to stop, brake as much as possible (1)
                if (float.IsNegativeInfinity(movementDirection))
                    movementDirection = 1;

               Brake(Mathf.Abs(movementDirection));    // Absolute braking (independently of the direction sign)
            }
            // Otherwise, normal acceleration
            else
            {
                // If speed below local maximum, accelerate
                if (absWheelsSpeed < LocalMaxSpeed)
                    Accelerate(movementDirection);
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
