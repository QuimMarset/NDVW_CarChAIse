using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager2 : MonoBehaviour
{
	// Editable parameters
	[Header("General")]
	public Player PlayerCar;
	[SerializeField] private PlayerHUD PlayerCanv;
	[SerializeField] private GameOverHUD GameOverCanv;
	[SerializeField] private PoliceManager PoliceMang;
	[SerializeField] private int MainMenuBuildIdx = 0;


	// Auxiliar variables	
	public bool IsGameOver { get; private set; }


	#region Initialization

	private void Awake()
	{
		IsGameOver = false;

		// Get Player if not assigned
		if (PlayerCar == null)
			PlayerCar = FindObjectOfType<Player>();

		if (PoliceMang == null)
			Debug.LogError("PoliceManager missing in GameManager. Police system will fail!");

		// Starting state
		PlayerCar.enabled = true;
		PlayerCanv.gameObject.SetActive(true);
		GameOverCanv.gameObject.SetActive(false);
	}

	#endregion

	#region End conditions control

	private void Update()
	{
		// Show catch state to playerit to player
		PlayerCanv.SetCatch(PoliceMang.CatchCounter, 100);

		// Check for game over
		if (!IsGameOver && (PlayerCar.IsDead || PoliceMang.CatchCounter == 100))
			GameOver();
	}

	#endregion

	#region Game over

	private void GameOver()
	{
		// Disable player car and canvas
		PlayerCar.Death();
		PlayerCanv.gameObject.SetActive(false);

		// Show game over canvas
		string gameOverMsg = PoliceMang.CatchCounter == 100 ? "The police caught you" : "The car is broken"; // Priorize catch message
		GameOverCanv.SetMessage(gameOverMsg + ", you failed.");
		GameOverCanv.gameObject.SetActive(true);
	}

	public void ReturnToMainMenu()
	{
		SceneManager.LoadScene(MainMenuBuildIdx);
	}

	#endregion
}

