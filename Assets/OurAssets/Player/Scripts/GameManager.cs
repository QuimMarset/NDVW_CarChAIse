using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	// Editable parameters
	[Header("General")]
	[SerializeField] private Player PlayerCar;
	[SerializeField] private PlayerHUD PlayerCanv;
	[SerializeField] private GameOverHUD GameOverCanv;
	[SerializeField] private int MainMenuBuildIdx = 0;

	[Header("Chasing")]
	[SerializeField] private float MinSpeedToCatch = 10;
	[SerializeField] private float MinDistanceToCatch = 10;
	[SerializeField] private int CatchPointPerPolice = 5;
	[SerializeField] private int EscapePointPerSecond = 15;


	// Auxiliar variables
	private float CatchCounter = 0;
	public List<Police> PoliceObjects { get; private set; }
	public bool IsGameOver { get; private set; }


	#region Initialization

	private void Awake()
	{
		IsGameOver = false;

		// Get Player if not assigned
		if (PlayerCar == null)
			PlayerCar = FindObjectOfType<Player>();

		// Create empty list of Police cars to be filled by police
		PoliceObjects = new List<Police>();

		// Starting state
		PlayerCar.enabled = true;
		PlayerCanv.gameObject.SetActive(true);
		GameOverCanv.gameObject.SetActive(false);
		UpdateCatchCounter(0);
	}

	#endregion

	#region Police

	public void AddPolice(Police police)
	{
		PoliceObjects.Add(police);
	}

	public void DelPolice(Police police)
	{
		PoliceObjects.Remove(police);
	}

	#endregion

	#region End conditions control

	private void Update()
	{
		// Update chasing state
		CheckCatchCondition();

		// Check for game over
		if (!IsGameOver && (PlayerCar.IsDead || CatchCounter == 100))
			GameOver();
	}

	#region Catch

	private void CheckCatchCondition()
	{
		// Get police cars close
		int nClosePoliceCars = 0;
		foreach (Police police in PoliceObjects)
			if (Vector3.Distance(PlayerCar.transform.position, police.transform.position) < MinDistanceToCatch)
				nClosePoliceCars++;

		// If car going slow and police cars close, increment catch counter
		if (Mathf.Abs(PlayerCar.CurrentForwardSpeed) < MinSpeedToCatch && nClosePoliceCars > 0)
			UpdateCatchCounter(nClosePoliceCars * CatchPointPerPolice * Time.deltaTime);
		// Otherwise, decrement it
		else
			UpdateCatchCounter(-Time.deltaTime * EscapePointPerSecond);

		// TODO: If cops have no visual, catch counter is decremented faster
	}

	private void UpdateCatchCounter(float increment)
	{
		CatchCounter += increment;
		CatchCounter = Mathf.Clamp(CatchCounter, 0, 100);

		// Show it to player
		PlayerCanv.SetCatch(CatchCounter, 100);
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
		string gameOverMsg = CatchCounter == 100 ? "The police caught you" : "The car is broken"; // Priorize catch message
		GameOverCanv.SetMessage(gameOverMsg + ", you failed.");
		GameOverCanv.gameObject.SetActive(true);
	}

	public void ReturnToMainMenu()
	{
		SceneManager.LoadScene(MainMenuBuildIdx);
	}

	#endregion
}

