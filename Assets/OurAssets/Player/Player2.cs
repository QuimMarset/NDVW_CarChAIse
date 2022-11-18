using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Player2 : CarController2
{
    [Header("Handbrake")]
    [SerializeField] protected bool EnableHandBrake = true;
    [SerializeField] protected float HardBrakeStifnessMultiplier = 0.5f;

    [Header("Canvas")]
    [SerializeField] protected TextMeshProUGUI SpeedText;
    [SerializeField] protected Image SpeedBar;
    [SerializeField] protected Image HealthBar;


    // Auxiliar variables
    protected float BackWheelsOriginalStiffness;
    protected WheelFrictionCurve BackWheelsFrictionCurve;

	#region Initialization

	protected override void Start()
	{
		base.Start();

        // Get info from backwheels for hand brake        
        BackWheelsFrictionCurve = WheelColliders[2].sidewaysFriction;   // The 2 first wheels are the directional/steering ones
        BackWheelsOriginalStiffness = BackWheelsFrictionCurve.stiffness;
    }

	#endregion

	#region Controls

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

	#endregion

	#region Handbrake

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
            for (int i = 2; i < WheelColliders.Length; i++) // The 2 first wheels are the directional/steering ones
                WheelColliders[i].sidewaysFriction = BackWheelsFrictionCurve;

            // Apply hand brake
            HandBrake(brakeRatio);
        }
    }
    
    protected virtual void HandBrake(float brakeRatio)
	{
        for (int i = WheelColliders.Length / 2; i < WheelColliders.Length; i++)
            WheelColliders[i].brakeTorque = brakeRatio * BrakeTorque;
    }

	#endregion

	#region Visual

	protected virtual void Update()
    {
        if (SpeedText)
            SpeedText.text = (int)CurrentWheelsSpeed + "km/h ";

        if (SpeedBar)
            SpeedBar.fillAmount = Mathf.Abs(CurrentWheelsSpeed) / MaxSpeed;
    }

	protected override void UpdateHealth(float healthModification)
	{
		base.UpdateHealth(healthModification);

        // Update health bar if available
        if (HealthBar)
            HealthBar.fillAmount = CurrentHealth / MaxHealth;
    }

	#endregion
}
