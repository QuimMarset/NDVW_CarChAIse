using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class PoliceManager : MonoBehaviour
{
	// Editable parameters
	[Header("Spawning")]
	[SerializeField] private uint MaxPoliceCars = 3;
	[SerializeField] private Police2 PolicePrefab;
	[SerializeField] private GameObject SpawnersContainer;

	[Header("Chasing")]
	[SerializeField] private float MinSpeedToCatch = 10;
	public float MinDistanceToCatch = 10;
	[SerializeField] private float CatchPointPerPolice = 5;
	[SerializeField] private float EscapePointPerSecond = 5;
	[SerializeField] private float EscapePointLostPlayerPerSecond = 20;
	[SerializeField] private float TimeForLosingPlayer = 3;
	[SerializeField] private float TimeForRelocatePolice = 10;

	// Auxiliar parameters
	private GameManager2 GameMang;
	private List<Police2> PoliceCars;
	private Transform[] Spawners;
	public float CatchCounter { get; protected set; }
	public Player PlayerCar { get => GameMang.PlayerCar; }
	private Vector3 LastPlayerKnownPos;
	private float LastPlayerPosTime;
	private float LastRelocateTime;



	#region Initialization

	private void Start()
	{
		// Initialize main variables		
		GameMang = GetComponent<GameManager2>();
		PoliceCars = new List<Police2>();
		Spawners = SpawnersContainer.GetComponentsInDirectChildren<Transform>();
		CatchCounter = 0;
		LastRelocateTime = Time.realtimeSinceStartup;

		// Initialize catch parameters
		LastPlayerKnownPos = PlayerCar.transform.position;
		LastPlayerPosTime = -1;
		UpdateCatchCounter(0);

		// First spawn of police cars		
		IniPoliceSpawn();
	}

	private void IniPoliceSpawn()
	{
		// Check if spawners are available
		if (Spawners.Length < MaxPoliceCars)
			throw new System.Exception("PoliceManager has less spawners than maximum number of police cars.");

		// Spawn all the police cars
		for (int spawnerIdx = 0; spawnerIdx < MaxPoliceCars; spawnerIdx++)
			SpawnPolice(spawnerIdx);
	}

	#endregion

	#region Spawning

	private void SpawnPolice(int spawnerIdx = -1)
	{
		// If spawner is negative, select a random one
		if (spawnerIdx < 0)
			spawnerIdx = (int)Random.Range(0, Spawners.Length - 0.9f);

		// Get spawner
		Vector3 spawnerPos = Spawners[spawnerIdx].position;

		// Spawn a new car
		GameObject newPoliceObj = Instantiate(PolicePrefab.gameObject, spawnerPos, Quaternion.identity, this.transform);
		Police2 police = newPoliceObj.GetComponent<Police2>();
		police.SetPoliceManager(this);
		police.NotifyPlayerPos(PlayerCar.transform.position, Time.realtimeSinceStartup); // TODO: Don't say player position at the start, start with patrol

		// Store police car
		PoliceCars.Add(police);
	}

	#endregion

	#region Update

	private void Update()
	{
		CheckPlayerVisual();
		CheckCatchState();
		RelocatePolice();
	}

	#region Player visual

	private void CheckPlayerVisual()
	{
		bool posUpdated = false;
		foreach (Police2 police in PoliceCars)
		{
			// If update is more recent, use it for update
			if (police.LastPlayerPosKnownTime > LastPlayerPosTime)
			{
				LastPlayerKnownPos = police.LastPlayerKnownPos;
				LastPlayerPosTime = police.LastPlayerPosKnownTime;
				posUpdated = true;
			}
		}

		// If new pos, communicate to police cars the new player position
		if (posUpdated)
		{
			foreach (Police2 police in PoliceCars)
				police.NotifyPlayerPos(LastPlayerKnownPos, LastPlayerPosTime);
		}
	}

	#endregion

	#region Catch

	private void CheckCatchState()
	{
		// Get police cars close
		int nClosePoliceCars = 0;
		foreach (Police2 police in PoliceCars)
			if (Vector3.Distance(PlayerCar.transform.position, police.transform.position) < MinDistanceToCatch)
				nClosePoliceCars++;

		// If car going slow and police cars close, increment catch counter
		if (Mathf.Abs(PlayerCar.CurrentForwardSpeed) < MinSpeedToCatch && nClosePoliceCars > 0)
			UpdateCatchCounter(nClosePoliceCars * CatchPointPerPolice * Time.deltaTime);
		// Otherwise, decrement it
		else
		{
			if (PlayerIsLost())
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

	private bool PlayerIsLost()
	{
		return (Time.realtimeSinceStartup - LastPlayerPosTime) > TimeForLosingPlayer;
	}

	#endregion

	#region Police relocating

	private void RelocatePolice()
	{
		// If too many time since player is lost and since last relocate
		if ((Time.realtimeSinceStartup - LastPlayerPosTime) > TimeForRelocatePolice &&
			(Time.realtimeSinceStartup - LastRelocateTime) > TimeForRelocatePolice)
		{
			// Search spawners close to player but not visible from them
			List<Transform> AvailableSpawners = new List<Transform>();
			foreach (Transform spawner in Spawners)
			{
				Vector3 iniPos = spawner.position;
				Vector3 direction = (PlayerCar.transform.position - iniPos).normalized;
				// If no hit or the hit is not with the player, spawner is available
				if (!Physics.Raycast(iniPos, direction, out RaycastHit hit, Mathf.Infinity, 0xFFFF) ||
					hit.transform.gameObject != PlayerCar.gameObject)
					AvailableSpawners.Add(spawner);
			}

			// Move police cars to available spawners
			for (int i = 0; i < AvailableSpawners.Count; i++)
			{
				PoliceCars[i].gameObject.SetActive(false);
				PoliceCars[i].transform.position = AvailableSpawners[i].position;
				PoliceCars[i].gameObject.SetActive(true);
			}

			LastRelocateTime = Time.realtimeSinceStartup;
		}
	}

	#endregion

	#endregion
}
