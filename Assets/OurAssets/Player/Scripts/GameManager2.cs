using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager2 : MonoBehaviour
{
	// Editable parameters
	[Header("General")]
	public Player PlayerCar;
	[SerializeField] private PlayerHUD PlayerCanv;
	[SerializeField] private GameOverHUD GameOverCanv;
	[SerializeField] private PoliceManager PoliceMang;
	[SerializeField] private int MainMenuBuildIdx = 0;
	[SerializeField] protected List<string> NavMeshLayers;
	[SerializeField] private GameObject PlayerTargetMark;
	[SerializeField] public bool ForceResetPlayerTarget = false;

	// Auxiliar variables
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

		// Get Player (if not assigned)
		if (PlayerCar == null)
			PlayerCar = FindObjectOfType<Player>();
		PlayerCar.SetGameManager(this);

		// Check PoliceManager
		if (PoliceMang == null)
			Debug.LogError("PoliceManager missing in GameManager. Police system will fail!");

		// Canvas
		PlayerCanv.gameObject.SetActive(true);
		GameOverCanv.gameObject.SetActive(false);

		// Player targets
		PlayerDistToTarget = -1; // Sentinel
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
			{
				int i = 1 << NavMesh.GetAreaFromName(Layer);
				NavMeshLayerBite += i;
			}
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
		bool noTarget = PlayerDistToTarget < 0;
		bool targetReached = Vector3.Distance(PlayerCar.transform.position, PlayerTarget) < 5;

		// New target is required
		if (noTarget || targetReached || ForceResetPlayerTarget)
		{
			ForceResetPlayerTarget = false;

			// If player reaches the target, add score
			if (targetReached)
				PlayerScore += PlayerDistToTarget;

			// Generate a new accesible target
			PlayerTarget = new Vector3(Random.Range(-500, 500), 0, Random.Range(-500, 500)); // TODO: Use street waypoints
			NavMeshHit hit;
			while (!NavMesh.SamplePosition(PlayerTarget, out hit, Mathf.Infinity, NavMeshLayerBite))
				PlayerTarget = new Vector3(Random.Range(-500, 500), 0, Random.Range(-500, 500));
			PlayerTarget = new Vector3(hit.position.x, 0, hit.position.z); // Use closest position to navmesh as target

			// Set distance to target (potential score)
			PlayerDistToTarget = Vector3.Distance(PlayerCar.transform.position, PlayerTarget);

			// Place a mark on target
			PlayerTargetMark.transform.position = PlayerTarget;
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
}

