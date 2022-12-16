using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Police2 : CarController
{
	// Editable parameters
	[Header("Police Parameters")]
	[SerializeField] protected List<string> NavMeshLayers;
	[SerializeField] private float AIFOV = 180;
	[SerializeField] protected bool Patrol = false;
	[SerializeField] protected float ReactionTime = 0.5f;
	[SerializeField] protected float CurveSpeedFactor = 1000;
	[SerializeField] protected bool CustomDebugger = false;
	[SerializeField] protected bool ShowGizmos = true;

	// Auxiliar parameters
	protected PoliceManager PoliceMang;
	protected int NavMeshLayerBite;
	protected List<Vector3> Waypoints = new List<Vector3>();
	protected int CurrentWayPoint;
	protected Vector3 PositionToFollow;
	protected Vector3 CurrentDirection;
	protected float NextActionTime = 0.0f;
	protected float PatrolCheckTime = -Mathf.Infinity;
	protected float BackwardStartTime = Mathf.Infinity;
	protected float BlockCheckStartTime = Mathf.Infinity;
	protected float SecondsForBlocked = 0.5f;
	protected float BackwardTime = 1f;
	public Vector3 LastPlayerKnownPos { get; protected set; }
	public float LastPlayerPosKnownTime { get; protected set; }


	#region Initialization

	protected override void Start()
	{
		base.Start();

		// Initialize catch settings with an already reached position and invalid/minimum frame index
		NotifyPlayerPos(transform.position, -1); // Any new notification will be accepted

		// Initialization of path settings
		CurrentWayPoint = 0;
		CalculateNavMashLayerBite();
	}

	private void CalculateNavMashLayerBite()
	{
		if (NavMeshLayers == null || NavMeshLayers[0] == "AllAreas")
			NavMeshLayerBite = NavMesh.AllAreas;
		else if (NavMeshLayers.Count == 1)
			NavMeshLayerBite += 1 << NavMesh.GetAreaFromName(NavMeshLayers[0]);
		else
		{
			foreach (string Layer in NavMeshLayers)
			{
				int I = 1 << NavMesh.GetAreaFromName(Layer);
				NavMeshLayerBite += I;
			}
		}
	}

	public void SetPoliceManager(PoliceManager policeMang)
	{
		PoliceMang = policeMang;
	}

	#endregion

	#region Player visual

	protected override void Update()
	{
		base.Update();
		CheckPlayerVisual();
	}

	protected virtual void CheckPlayerVisual()
	{
		Vector3 iniPos = CarFront.position;
		Vector3 direction = (PoliceMang.PlayerCar.transform.position - iniPos).normalized;
		if (Physics.Raycast(iniPos, direction, out RaycastHit hit, Mathf.Infinity, 0xFFFF) &&
			hit.transform.gameObject == PoliceMang.PlayerCar.gameObject)    // TODO: Mask Police colliders
		{
			//Debug.DrawRay(iniPos, direction * hit.distance, Color.red);
			LastPlayerKnownPos = PoliceMang.PlayerCar.transform.position;
			LastPlayerPosKnownTime = Time.realtimeSinceStartup;
		}
	}

	/// <summary>
	/// Called by the PoliceManager.
	/// </summary>
	public void NotifyPlayerPos(Vector3 lastPlayerKnownPos, float lastPlayerPosTime)
	{
		// If notification is more recent, update the parameters
		if (lastPlayerPosTime > LastPlayerPosKnownTime)
		{
			LastPlayerKnownPos = lastPlayerKnownPos;
			LastPlayerPosKnownTime = lastPlayerPosTime;
		}
	}

	#endregion

	#region Driving and path

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		ProgessPath();
	}

	#region Path

	/// <summary>
	/// Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
	/// </summary>
	protected virtual void ProgessPath()
	{
		// Update path if necessary
		if (Patrol == true)
		{
			// TODO: Follow the civilian routes
		}
		else
		{
			PatrolCheckTime = -Mathf.Infinity;
			CreatePathToTarget(LastPlayerKnownPos);
		}

		// If possible, define PositionToFollow
		if (CurrentWayPoint > 0 && CurrentWayPoint < Waypoints.Count)
		{
			PositionToFollow = Waypoints[CurrentWayPoint];

			// While very close to current waypoint, go to the next one
			while (Vector3.Distance(CarFront.position, PositionToFollow) < 1 && CurrentWayPoint < Waypoints.Count)
			{
				CurrentWayPoint++;
				if (CurrentWayPoint < Waypoints.Count)
					PositionToFollow = Waypoints[CurrentWayPoint];
			}
		}

		// If not PositionToFollow possible, go to the target
		if (CurrentWayPoint <= 0 || CurrentWayPoint >= Waypoints.Count)
		{
			CurrentWayPoint = Waypoints.Count; // Set to an impossible value, as sentinel
			PositionToFollow = LastPlayerKnownPos;
		}

		// Reset movement direction and steering (already defined in base.FixedUpdate)
		Steer();
		Move();
	}


	/// <summary>
	/// Creates a path to the Custom destination
	/// </summary>
	public virtual void CreatePathToTarget(Vector3 destination)
	{
		// If next action time, update path
		if (Time.realtimeSinceStartup > NextActionTime)
		{
			NavMeshPath path = new NavMeshPath();
			Vector3 sourcePosition;

			NextActionTime += ReactionTime;
			Waypoints.Clear();
			CurrentWayPoint = 1;

			sourcePosition = CarFront.position;
			if (CurrentDirection == null)
				CurrentDirection = CarFront.forward;

			Calculate(destination, sourcePosition, CurrentDirection, NavMeshLayerBite);
			void Calculate(Vector3 destination, Vector3 sourcePosition, Vector3 direction, int NavMeshAreaBite)
			{
				if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 1000000, NavMeshAreaBite) &&
					NavMesh.CalculatePath(sourcePosition, hit.position, NavMeshAreaBite, path))
				{
					if (path.corners.ToList().Count() > 1 && CheckForAngle(path.corners[1], sourcePosition, direction))
					{
						Waypoints.AddRange(path.corners.ToList());
						CustomDebug("Custom Path generated successfully", false);
					}
					else
					{
						if (path.corners.Length > 2 && CheckForAngle(path.corners[2], sourcePosition, direction))
						{
							Waypoints.AddRange(path.corners.ToList());
							CustomDebug("Custom Path generated successfully", false);
						}
						else
						{
							CustomDebug("Failed to generate a Custom path. Waypoints are outside the AIFOV. Generating a new one", false);
						}
					}
				}
				else
				{
					CustomDebug("Failed to generate a Custom path. Invalid Path. Generating a new one", false);
				}
			}
		}
	}

	/// <summary>
	/// Calculates the angle between the car and the waypoint
	/// </summary>
	protected virtual bool CheckForAngle(Vector3 pos, Vector3 source, Vector3 direction)
	{
		Vector3 distance = (pos - source).normalized;
		float CosAngle = Vector3.Dot(distance, direction);
		float Angle = Mathf.Acos(CosAngle) * Mathf.Rad2Deg;

		if (Angle < AIFOV)
			return true;
		else
			return false;
	}

	#endregion

	#region Steering

	protected override float GetSteeringAngle()
	{
		// Steer to PositionToFollow
		Vector3 relativeVector = transform.InverseTransformPoint(PositionToFollow);
		float steeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;

		// If going backwards, inverse steer
		if (BackwardStartTime != Mathf.Infinity & Time.time < BackwardStartTime + BackwardTime)
			steeringAngle = -steeringAngle;

		return steeringAngle;
	}

	#endregion

	#region Movement

	protected override float GetMovementDirection()
	{
		float movementDirection = 1;    // By default, accelerate

		// Check if blocked. If true go backwards.
		if (BlockCheckStartTime != Mathf.Infinity)
		{
			// If too many time stopped (i.e., blocked) and no backward time started
			if (Time.time >= (BlockCheckStartTime + SecondsForBlocked) && BackwardStartTime == Mathf.Infinity)
				BackwardStartTime = Time.time;

			// If backward time started
			if (BackwardStartTime != Mathf.Infinity)
			{
				if (Time.time < (BackwardStartTime + BackwardTime))
					movementDirection = -1.0f;
				else
				{
					BlockCheckStartTime = Mathf.Infinity;
					BackwardStartTime = Mathf.Infinity;
				}
			}
		}
		// If not blocked
		else
		{
			// If car don't move and target is far, start the blocked timer (consider the car blocked)
			if (Mathf.Abs(CurrentForwardSpeed) < 0.1f && Vector3.Distance(CarFront.position, LastPlayerKnownPos) > PoliceMang.MinDistanceToCatch/2)
				BlockCheckStartTime = Time.time;
			else
				BlockCheckStartTime = Mathf.Infinity;

			// Define the movement direction (accelerating or braking) depending on the curves
			Vector3 prev, post, diff;
			float curveAngle, requiredSpeed, brakeDistance, distanceToWaypoint;
			// For each next waypoints in the path, get angle between them. End if already breaking
			for (int i = CurrentWayPoint - 1; i < Waypoints.Count - 1 && movementDirection == 1; i++)
			{
				// First check is the trajectory to target
				prev = (i < CurrentWayPoint) ? CarFront.position : Waypoints[i];
				post = (i < CurrentWayPoint) ? PositionToFollow : Waypoints[i + 1];
				diff = post - prev; // TODO: Consider the distance btw the waypoints and the distance to that path section

				// Get the angle of the curve
				curveAngle = Vector3.Angle(CarRigidBody.velocity.normalized, diff);

				// TODO: Check this
				//if (i < Waypoints.Count - 2)    // If no last point
				//else // If last point
				//	curveAngle = 180; // Force brake

				// Get distance required to achieve the required speed				
				requiredSpeed = CurveSpeedFactor / curveAngle;  // Example with 50 degrees: 1000/50 = 20 Km/h
				brakeDistance = EstimateBrakeDistance(requiredSpeed);
				distanceToWaypoint = Vector3.Distance(CarFront.position, post);

				// If distance is close or lower to the brake distance
				if ((distanceToWaypoint - brakeDistance) < 5)
					movementDirection = -1;
			}

		}

		return movementDirection;
	}

	#endregion

	#endregion

	#region Debug and Gizmos

	protected virtual void CustomDebug(string text, bool isCritical = false)
	{
		if (CustomDebugger)
		{
			if (isCritical)
				Debug.LogError(text);
			else
				Debug.Log(text);
		}
	}

	/// <summary>
	/// Shows a Gizmos representing the waypoints and AI FOV
	/// </summary>
	protected virtual void OnDrawGizmos()
	{
		if (ShowGizmos == true)
		{
			for (int i = 0; i < Waypoints.Count; i++)
			{
				if (i == CurrentWayPoint)
					Gizmos.color = Color.blue;
				else
				{
					if (i > CurrentWayPoint)
						Gizmos.color = Color.red;
					else
						Gizmos.color = Color.green;
				}
				Gizmos.DrawWireSphere(Waypoints[i], 2f);
			}
			CalculateFOV();
		}

		void CalculateFOV()
		{
			Gizmos.color = Color.white;
			float totalFOV = AIFOV * 2;
			float rayRange = 10.0f;
			float halfFOV = totalFOV / 2.0f;
			Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
			Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
			Vector3 leftRayDirection = leftRayRotation * transform.forward;
			Vector3 rightRayDirection = rightRayRotation * transform.forward;
			Gizmos.DrawRay(CarFront.position, leftRayDirection * rayRange);
			Gizmos.DrawRay(CarFront.position, rightRayDirection * rayRange);
		}
	}

	#endregion
}
