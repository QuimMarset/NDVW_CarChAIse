using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Rigidbody))]
public abstract class CarController : MonoBehaviour
{
	// Editable parameters
	[Header("Car wheels")]  // Assign wheel Colliders and transform through the inspector
	[SerializeField] protected Transform WheelCollidersContainer;
	[SerializeField] protected Transform WheelTransformsContainer;

	[Header("General Parameters")]
	public bool EnableMovement = true;
	public bool EnableSteering = true;
	[SerializeField] protected bool EnableABS = true;
	[SerializeField] protected float ABSThld = 0.9f;
	[SerializeField] protected float MaxSteeringAngle = 60;
	[SerializeField] public float MaxSpeed = 200;
	[SerializeField] protected float BrakeTorque = 25000;
	[SerializeField] protected float MovementTorque = 2000;
	[SerializeField] protected float DragCoefficient = 0.47f;
	[SerializeField] public Transform CarFront;

	[Header("Health")]
	[SerializeField] [Range(0.001f, 1000)] protected float MaxHealth = 100;
	[SerializeField] [Range(0, 100)] protected float DamagePerKph = 0;

	[Header("Sound")]
	[SerializeField] protected float MinPitch = 1;
	[SerializeField] protected float MaxPitch = 4;

	// Constants    
	protected const float MPS_TO_KPH = 3600f / 1000f;
	protected const float RPM_PER_RADIUS_TO_KPH = 1f / 60f * 2f * Mathf.PI; // TODO: Add * MPS_TO_KPH
	protected const float FRICTION_COEF = 1.225f * 1f; // Density of air * Cross section area

	// Auxiliar variables
	public Rigidbody CarRigidBody { get; protected set; }
	public Vector3 CurrentVelocity { get; protected set; }
	protected WheelCollider[] WheelColliders;
	protected Transform[] WheelTransforms;
	protected AudioSource SoundSource;
	public float CurrentHealth { get; protected set; }
	public bool IsDead { get; protected set; }
	public float CurrentWheelsSpeed { get; protected set; }
	public float CurrentForwardSpeed { get; protected set; }
	public bool IsGoingBackwards { get; protected set; }


	#region Intialization

	protected virtual void Start()
	{
		// Set center of mass to zero
		CarRigidBody = GetComponent<Rigidbody>();
		CarRigidBody.centerOfMass = Vector3.zero;

		// Get wheels colliders and transforms
		WheelColliders = WheelCollidersContainer.GetComponentsInChildren<WheelCollider>();
		WheelTransforms = WheelTransformsContainer.gameObject.GetComponentsInDirectChildren<Transform>();
		IsGoingBackwards = false;

		// Health
		CurrentHealth = MaxHealth;

		// Sound
		SoundSource = GetComponent<AudioSource>();
	}

	#endregion

	#region Update

	protected virtual void FixedUpdate()
	{
		// Get speeds
		CurrentVelocity = CarRigidBody.velocity;
		CurrentWheelsSpeed = GetWheelsSpeed();
		CurrentForwardSpeed = GetForwardSpeed();

		// Control
		Steer();
		Move();
	}

	#region Compute wheels and forward speeds

	protected virtual float GetWheelsSpeed()
	{
		float avgWheelsSpeed = 0;
		for (int i = 0; i < WheelColliders.Length; i++)
			avgWheelsSpeed += GetWheelSpeed(WheelColliders[i]) / WheelColliders.Length;
		return avgWheelsSpeed;
	}

	protected virtual float GetWheelSpeed(WheelCollider wheel)
	{
		return wheel.rpm * wheel.radius * RPM_PER_RADIUS_TO_KPH;
	}

	protected virtual float GetForwardSpeed()
	{
		return Vector3.Dot(CarRigidBody.velocity, transform.forward) * MPS_TO_KPH;
	}

	#endregion

	#region Sound

	protected virtual void Update()
	{
		// Sound
		if (SoundSource)
		{
			float speedRatio = Mathf.Abs(CurrentWheelsSpeed) / MaxSpeed;
			SoundSource.pitch = Mathf.LerpUnclamped(MinPitch, MaxPitch, speedRatio);
		}
	}

	#endregion

	#endregion

	#region Steering

	/// <summary>
	/// Applies steering to the Current waypoint
	/// </summary>
	protected virtual void Steer()
	{
		if (EnableSteering)
		{
			// Get steering angle
			float steeringAngle = GetSteeringAngle();

			// Check maximum steering values
			steeringAngle = Mathf.Clamp(steeringAngle, -MaxSteeringAngle, MaxSteeringAngle);

			// Set direction wheels angle
			for (int i = 0; i < WheelColliders.Length / 2; i++)
				WheelColliders[i].steerAngle = steeringAngle;

			// Update wheels rotations
			UpdateWheels();
		}
	}

	/// <summary>
	/// Obtaing the angle for rotating the steering wheels.
	/// Positive values for turning right, negatives for turning left.
	/// Maximum absolute values shoudt be lower than the MaxSteeringAngle
	/// </summary>
	/// <returns>Desired steering wheels rotation</returns>
	protected abstract float GetSteeringAngle();

	/// <summary>
	///  Updates the wheel's postion and rotation
	/// </summary>
	protected virtual void UpdateWheels()
	{
		for (int i = 0; i < WheelColliders.Length; i++)
			ApplyRotationAndPostion(WheelColliders[i], WheelTransforms[i]);
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

	#endregion

	#region Movement

	/// <summary>
	/// Apply acceleration torque depending on the ratio
	/// </summary>
	/// <param name="accelRatio">Must be in the range [-1, 1]. Otherwise it would be clamped.</param>
	protected virtual void Accelerate(float accelRatio = 1)
	{
		accelRatio = Mathf.Clamp(accelRatio, -1, 1);
		for (int i = 0; i < WheelColliders.Length; i++)
			WheelColliders[i].motorTorque = accelRatio * MovementTorque;
	}


	/// <summary>
	/// Apply brake torque depending on the ratio
	/// </summary>
	/// <param name="brakeRatio">Must be in the range [0, 1]. Otherwise it would be clamped.</param>
	protected virtual void Brake(float brakeRatio = 1)
	{
		WheelCollider wheel;
		float wheelBreakRatio;
		float absWheelSpeed;
		float absForwardSpeed;

		brakeRatio = Mathf.Clamp(brakeRatio, 0, 1);

		// For each wheel
		for (int i = 0; i < WheelColliders.Length; i++)
		{
			wheel = WheelColliders[i];
			wheelBreakRatio = brakeRatio;

			// If ABS, check difference between forward speed and wheels speed
			if (EnableABS && brakeRatio > 0)
			{
				absWheelSpeed = Mathf.Abs(GetWheelSpeed(wheel));
				absForwardSpeed = Mathf.Abs(CurrentForwardSpeed / 3.6f);    // TODO: Remove /3.6f when speed constant is adjusted
				if (absWheelSpeed > 1f && absWheelSpeed < absForwardSpeed * ABSThld)
					wheelBreakRatio = 0;
			}

			// Apply break
			WheelColliders[i].brakeTorque = wheelBreakRatio * BrakeTorque;
		}
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
			IsGoingBackwards = movementDirection < 0;

			// If movement direction is different from wheels speed or it's -Inf, brake
			if ((movementDirection > 0 && CurrentWheelsSpeed < -0.1f) || (movementDirection < 0 && CurrentWheelsSpeed > 0.1f) ||
				float.IsNegativeInfinity(movementDirection))
			{
				// When want to stop (movementDir = -Inf), brake as much as possible (1)
				if (float.IsNegativeInfinity(movementDirection))
					movementDirection = 1;

				Brake(Mathf.Abs(movementDirection));    // Absolute braking (independently of the direction sign)
			}
			// If there is a movement direction
			else if (Mathf.Abs(movementDirection) > 0)
			{
				// Compute speed of wheels
				float absWheelsSpeed = Mathf.Abs(CurrentWheelsSpeed);

				// If speed below maximum, accelerate
				if (absWheelsSpeed < MaxSpeed)
					Accelerate(movementDirection);
				// If speed is too high (with a 1/4 marging), brake
				else if (absWheelsSpeed > MaxSpeed + (MaxSpeed * 1 / 4))
					Brake();
				// Otherwise, do nothing
			}
			// If movementDirection == 0, apply air resistance/drag
			else
			{
				float currentSpeed = CarRigidBody.velocity.magnitude;
				float forceAmount = (FRICTION_COEF * DragCoefficient * Mathf.Pow(currentSpeed, 2)) / 2;
				Vector3 resistanceDir = -CarRigidBody.velocity.normalized;
				CarRigidBody.AddForce(resistanceDir * forceAmount);
			}
		}
		// If movement disabled, brake
		else
			Brake();
	}

	#endregion

	#region Health and crashing
	protected void OnCollisionEnter(Collision collision)
	{
		// Get velocity difference
		Vector3 otherVelocity = Vector3.zero;   // By default, other object is static
		CarController otherCar = collision.gameObject.GetComponent<CarController>();
		if (otherCar)   // Try with CarController
			otherVelocity = otherCar.CurrentVelocity;
		else // Otherwise, try rigidbody velocity, which is already updated by collision (not ideal)
		{
			Rigidbody otherRigidbody = collision.gameObject.GetComponent<Rigidbody>();
			if (otherRigidbody)
				otherVelocity = otherRigidbody.velocity;
		}
		Vector3 velocityDiff = CurrentVelocity - otherVelocity; // Using CurrentVelocity instead of CarRigidBody.velocity since it is already updated due to the collision
		Vector2 velocityDiff2D = new Vector2(Mathf.Abs(velocityDiff.x), Mathf.Abs(velocityDiff.z)); // Transform to 2D

		// Get contact direction
		Vector3 collisionDir = Vector3.zero;
		for (int i = 0; i < collision.contactCount; i++)
			collisionDir += (collision.GetContact(i).point - transform.position).normalized;
		collisionDir = collisionDir.normalized; // Normalize to get the average direction

		// Discretize direction
		Vector3[] possibleDirs = new Vector3[] { transform.forward, -transform.forward, transform.right, -transform.right };
		Vector3 bestDir = Vector3.zero;
		float dist;
		float minDistance = float.PositiveInfinity;
		foreach (Vector3 dir in possibleDirs)
		{
			dist = Vector3.Distance(collisionDir, dir);
			if (dist < minDistance)
			{
				minDistance = dist;
				bestDir = dir;
			}
		}

		// Transform to 2D
		Vector2 collisionDir2D = new Vector2(bestDir.x, bestDir.z);

		// Compute damage
		float collisionSpeed = Mathf.Abs(Vector2.Dot(velocityDiff2D, collisionDir2D));
		float damage = DamagePerKph * collisionSpeed;

		// Take damage
		UpdateHealth(damage);
	}

	protected virtual void UpdateHealth(float healthDecrement)
	{
		// Update value with limits
		CurrentHealth = Mathf.Clamp(CurrentHealth - healthDecrement, 0, MaxHealth);

		// Check death
		if (CurrentHealth == 0)
			Death();
	}

	public virtual void Death()
	{
		Debug.Log(this + " has dead");
		IsDead = true;
		EnableMovement = false; // Disable movement
		EnableSteering = false; // Disable steering
		if (SoundSource)
			SoundSource.pitch = MinPitch;   // Stopped engine sound
	}

	#endregion

	#region Auxiliar

	public virtual float EstimateBrakeDistance(float finalSpeed, float? startSpeed = null)
	{
		float currentSpeed = (startSpeed == null) ? CarRigidBody.velocity.magnitude : (float)startSpeed;

		// Estimate decceleration. Function approximated from tests of 100km/h to 0km/h, and the web https://mathcracker.com/es/calculadora-funcion-logaritmica
		float esimatedBrakeAcceleration = -3.4516f * Mathf.Log(0.0465f * BrakeTorque);

		// Compute brake distance using UARM equation
		float speedToLoss = currentSpeed - finalSpeed;
		float brakeTime = -speedToLoss / esimatedBrakeAcceleration;
		float brakeDistance = currentSpeed * brakeTime + 0.5f * esimatedBrakeAcceleration * Mathf.Pow(brakeTime, 2);

		return brakeDistance;
	}

	#endregion
}
