using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Player : CarController
{
    [SerializeField] protected bool EnableHandBrake = true;
    [SerializeField] protected float HardBrakeStifnessMultiplier = 0.1f;

    [Header("Canvas")]
    [SerializeField] protected TextMeshProUGUI SpeedText;
    

    // Auxiliar variables
    protected float BackWheelsOriginalStiffness;
    protected WheelFrictionCurve BackWheelsFrictionCurve;

	protected override void Start()
	{
		base.Start();

        // Get info from backwheels for hand brake
        BackWheelsOriginalStiffness = BackLeftC.sidewaysFriction.stiffness;
        BackWheelsFrictionCurve = BackLeftC.sidewaysFriction;
    }

	protected override void FixedUpdate()
	{
        base.FixedUpdate();
        CheckHandBrake();
    }

    protected virtual void CheckHandBrake()
	{
        if (EnableHandBrake)
		{
            float brakeRatio = Mathf.Abs(Input.GetAxis("Jump"));

            // Change stiffness        
            float newStiffness = Mathf.Lerp(BackWheelsOriginalStiffness,
                BackWheelsOriginalStiffness * HardBrakeStifnessMultiplier,
                brakeRatio);
            BackWheelsFrictionCurve.stiffness = newStiffness;
            BackLeftC.sidewaysFriction = BackWheelsFrictionCurve;
            BackRightC.sidewaysFriction = BackWheelsFrictionCurve;

            // Apply hand brake
            HandBrake(brakeRatio);
        }
    }
        

    protected virtual void HandBrake(float brakeRatio)
	{
        BackLeftC.brakeTorque = brakeRatio * BrakeTorque;
        BackRightC.brakeTorque = brakeRatio * BrakeTorque;
    }

    protected virtual void Update()
	{
        if (SpeedText)
            SpeedText.text = (int)CurrentWheelsSpeed + "km/h ";
        else
            Debug.LogWarning("Canvas speed text missing");
	}

    protected override float GetSteeringAngle()
	{
        float steeringValue = Input.GetAxis("Horizontal");
        float steeringAngle = steeringValue * MaxSteeringAngle;
        return steeringAngle;
    }

    protected override float GetMovementDirection()
	{
        return Input.GetAxis("Vertical");
    }
}
