using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PoliceManager))]
public class GameManager : MonoBehaviour
{
	// Editable parameters
	[Header("General")]
	public Player PlayerCar;
	[SerializeField] private PlayerHUD PlayerCanv;
	[SerializeField] private GameObject PlayerTargetMark;

	public PoliceManager PoliceMang;
	[SerializeField] private GameOverHUD GameOverCanv;
	[SerializeField] private int MainMenuBuildIdx = 0;
	[SerializeField] private List<string> NavMeshLayers;
	[SerializeField] public bool ForceResetPlayerTarget = false;

	// Auxiliar variables
	public RoadManager RoadMang { get; private set; }
	public int NavMeshLayerBite { get; private set; }
	public bool IsGameOver { get; private set; }
	public Vector3 PlayerTarget { get; private set; }
	public float PlayerDistToTarget { get; private set; }
	public float PlayerScore { get; private set; }


	#region Initialization

	private void Awake()
	{
		IsGameOver = false;
		CalculateNavMashLayerBite();

		// Player
		if (PlayerCar == null)
			PlayerCar = FindObjectOfType<Player>();
		PlayerCar.SetGameManager(this);

		// PoliceManager
		if (PoliceMang == null)
			PoliceMang = GetComponent<PoliceManager>();

		// RoadManager
		if (RoadMang == null)
			RoadMang = FindObjectOfType<RoadManager>();

		// Canvas
		PlayerCanv.gameObject.SetActive(true);
		GameOverCanv.gameObject.SetActive(false);

		// Player targets
		PlayerDistToTarget = 0; // Sentinel
		PlayerScore = 0;
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
				NavMeshLayerBite += 1 << NavMesh.GetAreaFromName(Layer);
		}
	}

	#endregion

	#region Update		

	private void Update()
	{
		CheckEndConditions();

		if (!IsGameOver)
			UpdatePlayerTarget();
	}

	#region End conditions control

	private void CheckEndConditions()
	{
		// Show catch state to playerit to player
		PlayerCanv.SetCatch(PoliceMang.CatchCounter, 100);

		// Check for game over
		if (!IsGameOver && (PlayerCar.IsDead || PoliceMang.CatchCounter == 100))
			GameOver();
	}

	#endregion

	#region Player destinations

	private void UpdatePlayerTarget()
	{
		bool noTarget = PlayerDistToTarget == 0;
		bool targetReached = Vector3.Distance(PlayerCar.transform.position, PlayerTarget) < 10;

		// New target is required
		if (noTarget || targetReached || ForceResetPlayerTarget)
		{
			ForceResetPlayerTarget = false;

			// If player reaches the target, add score
			if (targetReached)
				PlayerScore += PlayerDistToTarget;

			// Generate a new accesible target if possible
			PlayerTarget = RoadMang.GetRandomMarker().transform.position;

			// Set distance to target (potential score)
			PlayerDistToTarget = Vector3.Distance(PlayerCar.transform.position, PlayerTarget);

			// Place a mark on target
			PlayerTargetMark.transform.position = PlayerTarget + Vector3.up * PlayerTargetMark.transform.lossyScale.y;
		}
	}

	#endregion

	#endregion

	#region Game over

	private void GameOver()
	{
		// Disable player car and canvas
		PlayerCar.Death();
		PlayerCanv.gameObject.SetActive(false);

		// Show game over canvas
		string gameOverMsg = PoliceMang.CatchCounter == 100 ? "The police caught you" : "The car is broken"; // Priorize catch message
		gameOverMsg += ", you failed.\nScore = " + (int)PlayerScore;
		GameOverCanv.SetMessage(gameOverMsg);
		GameOverCanv.gameObject.SetActive(true);

		IsGameOver = true;
	}

	public void ReturnToMainMenu()
	{
		SceneManager.LoadScene(MainMenuBuildIdx);
	}

	#endregion

	#region Auxiliar
	public bool IsVisibleByPlayer(Vector3 pos)
	{
		pos += Vector3.up; // Height offset for avoiding sidewalks
		float distToPlayer = (pos - PlayerCar.transform.position).magnitude;    // Check if too close
		Vector3 dirToCamera = (PlayerCar.MainCamera.transform.position - pos);
		return distToPlayer < 10 || !Physics.Raycast(pos, dirToCamera.normalized, dirToCamera.magnitude, LayerMask.GetMask(new string[] { "Default", "Player" }));
	}

	/// <summary>
	/// // Search available markers (not colliding with civilians, police or player) and not visible by the player
	/// </summary>
	public List<Marker> GetMarkersForSpawning(out List<GameObject> carsObjs, bool checkInPlayerView = true)
	{
		List<Marker> availableMarkers = new List<Marker>();
		carsObjs = PoliceMang.PoliceCars.Select((x) => x.gameObject).ToList();
		carsObjs.Add(PlayerCar.gameObject); // Consider player's position
		foreach (Marker mkr in RoadMang.AllMarkers)
		{
			if ((!checkInPlayerView || !IsVisibleByPlayer(mkr.Position)) &
				RoadMang.IsMarkerAvailable(mkr, otherObjs: carsObjs))
				availableMarkers.Add(mkr);
		}

		return availableMarkers;
	}

	#endregion
}

