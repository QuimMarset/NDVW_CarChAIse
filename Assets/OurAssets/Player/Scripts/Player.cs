using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Player : CarController
{
    [Header("Handbrake")]
    [SerializeField] protected bool EnableHandBrake = true;
    [SerializeField] protected float HardBrakeStifnessMultiplier = 0.5f;

    [Header("Camera")]
    [SerializeField] protected Camera MainCamera;
    [SerializeField] protected float CameraSecondsToRotate = 0.5f;

    // Auxiliar variables
    protected GameManager2 GameMang;
    protected PlayerHUD PlayerCanv;
    protected float BackWheelsOriginalStiffness;
    protected WheelFrictionCurve BackWheelsFrictionCurve;

	#region Initialization

	protected override void Start()
	{
		base.Start();

        // Get info from backwheels for hand brake        
        BackWheelsFrictionCurve = WheelColliders[2].sidewaysFriction;   // The 2 first wheels are the directional/steering ones
        BackWheelsOriginalStiffness = BackWheelsFrictionCurve.stiffness;

        // Try to get camera if not available
        if (MainCamera == null)
            MainCamera = GetComponentInChildren<Camera>();

        // Get player canvas
        PlayerCanv = FindObjectOfType<PlayerHUD>();

        // Show initial health
        UpdateHealth(0);
    }

	public virtual void SetGameManager(GameManager2 gameMang)
	{
        GameMang = gameMang;
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

	protected override void Update()
    {
        base.Update();

        UpdateCamera();

        // Update speed and score
        if (PlayerCanv)
		{
			PlayerCanv.SetSpeed(CurrentWheelsSpeed, MaxSpeed);
			PlayerCanv.SetScore(GameMang.PlayerScore);
			UpdateTargetArrow();
		}
	}

	private void UpdateTargetArrow()
	{
        Vector3 origDir = MainCamera.transform.forward;
        Vector2 origDir2D = new Vector2(origDir.x, origDir.z);
        Vector3 targetDir = GameMang.PlayerTarget - transform.position;
        Vector2 targetDir2D = new Vector2(targetDir.x, targetDir.z);
        PlayerCanv.SetTargetArrow(Vector2.SignedAngle(origDir2D, targetDir2D));
	}

	protected virtual void UpdateCamera()
	{
        // Move camera according to car's velocity
        if (MainCamera && CarRigidBody.velocity.magnitude > 1f)
        {
            float angleDiff = Vector3.SignedAngle(MainCamera.transform.forward, CarRigidBody.velocity.normalized, axis: Vector3.up);

            // If angle difference is not too low
            if (Mathf.Abs(angleDiff) > 1f)
			{
                MainCamera.transform.RotateAround(transform.position, Vector3.up, angleDiff * Time.deltaTime / CameraSecondsToRotate);
            }                
        }
    }

	protected override void UpdateHealth(float healthDecrement)
	{
		base.UpdateHealth(healthDecrement);

        // Show new health
        PlayerCanv.SetHealth(CurrentHealth, MaxHealth);
    }

    #endregion
}
