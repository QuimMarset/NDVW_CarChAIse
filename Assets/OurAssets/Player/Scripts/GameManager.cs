using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Editable parameters
    [SerializeField] private Player2 PlayerCar;
    [SerializeField] private Canvas PlayerCanvas;
    [SerializeField] private Canvas GameOverCanvas;
    [SerializeField] private int MainMenuBuildIdx = 0;

    private void Start()
    {
        PlayerCar.enabled = true;
        PlayerCanvas.enabled = true;
        GameOverCanvas.enabled = false;
    }

    private void Update()
    {
        // Check for game over
		if (PlayerCar.IsDead)
            GameOver();
    }

    private void GameOver()
	{
        PlayerCanvas.enabled = false;
        GameOverCanvas.enabled = true;
    }

    public void ReturnToMainMenu()
	{
        SceneManager.LoadScene(MainMenuBuildIdx);
	}
}
