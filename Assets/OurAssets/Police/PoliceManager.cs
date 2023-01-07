using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class PoliceManager : MonoBehaviour
{
	// Editable parameters
	[Header("Spawning")]
	[SerializeField] private uint PoliceCarsToSpawn = 3;
	[SerializeField] private Police PolicePrefab;
	[SerializeField] private bool StartChasing = true;
	[SerializeField] private bool ForceIgnorePlayer = false;

	[Header("Chasing")]
	[SerializeField] private float MinSpeedToCatch = 10;
	public float MinDistanceToCatch = 10;
	[SerializeField] private float CatchPointPerPolice = 5;
	[SerializeField] private float EscapePointPerSecond = 5;
	[SerializeField] private float EscapePointLostPlayerPerSecond = 20;
	[SerializeField] private float TimeForLosingPlayer = 10;
	[SerializeField] private float TimeForRelocatePolice = 30;

	// Auxiliar parameters
	private GameManager GameMang;
	public Player PlayerCar { get => GameMang.PlayerCar; }
	public RoadManager RoadMang { get => GameMang.RoadMang; }
	public List<Police> PoliceCars { get; protected set; }
	public float CatchCounter { get; protected set; }
	public bool PlayerIsLost { get => (Time.time - LastPlayerPosTime) > TimeForLosingPlayer; }
	private Vector3 LastPlayerKnownPos;
	private float LastPlayerPosTime;
	private float LastRelocateTime;

	#region Initialization

	private void Start()
	{
		// Initialize main variables		
		GameMang = GetComponent<GameManager>();
		PoliceCars = new List<Police>();

		CatchCounter = 0;
		LastRelocateTime = Time.time;

		// Initialize catch parameters
		if (StartChasing && !ForceIgnorePlayer)
		{
			LastPlayerKnownPos = PlayerCar.transform.position;
			LastPlayerPosTime = Time.time;
		}
		else
		{
			LastPlayerKnownPos = Vector3.zero;
			LastPlayerPosTime = Mathf.NegativeInfinity;
		}
		UpdateCatchCounter(0);
	}

	#endregion

	#region Spawning


	#endregion

	#region Update

	private void Update()
	{
		ManagePlayerVisual();
		CheckCatchState();
		CheckSpawnPolice(); // This should be done after ManagePlayerVisual
		CheckRelocatePolice();
	}

	#region Spawning and relocating

	private void CheckSpawnPolice()
	{
		// If too few police cars
		if (PoliceCars.Count < PoliceCarsToSpawn)
		{
			// Spawn remaining police cars
			for (int i = PoliceCars.Count; i < PoliceCarsToSpawn; i++)
				PlacePolice();
		}
		// If too much police cars
		else if (PoliceCars.Count > PoliceCarsToSpawn)
		{
			// Spawn remaining police cars
			for (int i = PoliceCars.Count - 1; i > PoliceCarsToSpawn - 1; i--)
			{
				Destroy(PoliceCars[i].gameObject);
				PoliceCars.RemoveAt(i);
			}
		}
	}

	private void PlacePolice(Marker mkr = null, GameObject policeObj = null)
	{
		// Get marker
		if (mkr == null)
		{
			// Search available markers
			List<Marker> availableMarkers = GameMang.GetMarkersForSpawning();

			// Get a random marker
			if (availableMarkers.Count > 0)
				mkr = availableMarkers[Random.Range(0, availableMarkers.Count)];
		}

		if (mkr == null)
			Debug.LogWarning("No marker found for spawning a police car.");
		else
		{
			// Spawn a new car
			if (policeObj == null)
			{
				policeObj = Instantiate(PolicePrefab.gameObject, mkr.transform.position, mkr.transform.rotation);
				policeObj.name = "Police_" + (PoliceCars.Count + 1);

				// Initialize police behaviour
				Police police = policeObj.GetComponent<Police>();
				police.SetPoliceManager(this);
				PoliceCars.Add(police);
			}
			// Replace car
			else
			{
				// Resume movement if was at traffic light
				TrafficLightBehavior trafficLightBehavior = policeObj.GetComponent<TrafficLightBehavior>();
				trafficLightBehavior.ResumeMovement();

				policeObj.transform.position = mkr.transform.position;
				policeObj.transform.rotation = mkr.transform.rotation;
			}

			// Set current waypoint
			MoveToWaypointBehavior moveToWaypointBehavior = policeObj.GetComponent<MoveToWaypointBehavior>();
			moveToWaypointBehavior.SetTargetMarker(mkr.GetNextAdjacentMarker());
		}
	}

	private void CheckRelocatePolice()
	{
		// If too many time since player is lost and since last relocate
		if (PlayerIsLost && (Time.time - LastPlayerPosTime) > TimeForRelocatePolice &&
			(Time.time - LastRelocateTime) > TimeForRelocatePolice)
		{
			// Search available markers
			List<GameObject> carsObjs = PoliceCars.Select((x) => x.gameObject).ToList();
			List<Marker> availableMarkers = GameMang.GetMarkersForSpawning();

			// Sort by distance to target	//TODO: Add noise to distances for randomization
			availableMarkers = availableMarkers.OrderBy(mkr => (mkr.transform.position - GameMang.PlayerTarget).magnitude).ToList();

			// Move police cars to available markers. Maybe not all can be relocated
			int policeIdx = 0;
			for (int mkrIdx = 0; mkrIdx < availableMarkers.Count && policeIdx < PoliceCars.Count; mkrIdx++)
			{
				// If police car is visible by player
				if (GameMang.IsVisibleByPlayer(PoliceCars[policeIdx].transform.position))
					policeIdx++;
				// If marker NOT occupied by a previously relocated car
				else if (RoadMang.IsMarkerAvailable(availableMarkers[mkrIdx], otherObjs: carsObjs))
				{
					PlacePolice(availableMarkers[mkrIdx], PoliceCars[policeIdx].gameObject);
					policeIdx++;
				}
			}

			// Reset relocate timer
			LastRelocateTime = Time.time;

			// Debug
			Debug.Log("Police relocated");
		}
	}

	public Marker GetClosestMarker(Vector3 pos)
	{
		return RoadMang.GetClosestMarker(pos);
	}

	#endregion

	#region Player visual

	public void InformPlayerPos(Vector3 lastPlayerKnownPos, float lastPlayerPosTime)
	{
		// If update is more recent or the player is lost (some car at last player pos but without visual or contact) and not ignoring, update
		if ((lastPlayerPosTime > LastPlayerPosTime || lastPlayerPosTime == Mathf.NegativeInfinity) && !ForceIgnorePlayer)
		{
			LastPlayerKnownPos = lastPlayerKnownPos;
			LastPlayerPosTime = lastPlayerPosTime;
		}
	}

	private void ManagePlayerVisual()
	{
		// If player is lost or to ignore, use Time == -Infinity; the sentinel for patrolling
		if (PlayerIsLost || ForceIgnorePlayer)
			LastPlayerPosTime = Mathf.NegativeInfinity;

		// Set multiple targets at different sides of the player
		Vector3[] targets = { PlayerCar.transform.forward, -PlayerCar.transform.forward, PlayerCar.transform.right, -PlayerCar.transform.right };

		// Communicate information to police cars, with a different target depending on the index
		for (int i = 0; i < PoliceCars.Count; i++)
			PoliceCars[i].ReceivePlayerPos(LastPlayerKnownPos + 2 * targets[i%targets.Length], LastPlayerPosTime);
	}

	#endregion

	#region Catch

	private void CheckCatchState()
	{
		// Get police cars close
		int nClosePoliceCars = 0;
		foreach (Police police in PoliceCars)
			if (Vector3.Distance(PlayerCar.transform.position, police.transform.position) < MinDistanceToCatch)
				nClosePoliceCars++;

		// If car going slow and police cars close, increment catch counter
		if (Mathf.Abs(PlayerCar.CurrentForwardSpeed) < MinSpeedToCatch && nClosePoliceCars > 0)
			UpdateCatchCounter(nClosePoliceCars * CatchPointPerPolice * Time.deltaTime);
		// Otherwise, decrement it
		else
		{
			if (PlayerIsLost)
				UpdateCatchCounter(-Time.deltaTime * EscapePointLostPlayerPerSecond);
			else
				UpdateCatchCounter(-Time.deltaTime * EscapePointPerSecond);
		}
	}

	private void UpdateCatchCounter(float increment)
	{
		CatchCounter += increment;
		CatchCounter = Mathf.Clamp(CatchCounter, 0, 100);
	}

	#endregion

	#endregion
}
