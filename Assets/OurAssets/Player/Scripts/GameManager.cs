using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Editable parameters
    [SerializeField] private Player2 PlayerCar;
    [SerializeField] private Canvas PlayerCanvas;
    [SerializeField] private Canvas GameOverCanvas;
    [SerializeField] private Image ChaseBar;
    [SerializeField] private GameObject ChaseBarContainer;
    [SerializeField] private int MainMenuBuildIdx = 0;
    [SerializeField] private float MinSpeedToCatch = 10;
    [SerializeField] private float MinDistanceToCatch = 10;
    [SerializeField] private int CatchPointPerPolice = 5;
    [SerializeField] private int EscapePointPerSecond = 15;

    private float CatchCounter = 0;
    private Police[] PoliceObjects;

    private void Start()
    {
        PlayerCar.enabled = true;
        PlayerCanvas.enabled = true;
        GameOverCanvas.enabled = false;

        PoliceObjects = FindObjectsOfType<Police>();
        UpdateCatchCounter(0);
    }

    private void Update()
    {
        // Check for game over
		if (PlayerCar.IsDead)
            GameOver();

        CheckChaseCondition();
    }

    private void GameOver()
	{
        PlayerCanvas.enabled = false;
        GameOverCanvas.enabled = true;
        PlayerCar.enabled = false;
    }

    public void ReturnToMainMenu()
	{
        SceneManager.LoadScene(MainMenuBuildIdx);
	}

    private void CheckChaseCondition()
    {
        if (PlayerCar.CurrentForwardSpeed < MinSpeedToCatch)
        {
            int PoliceCount = 0;
            foreach(Police police in PoliceObjects)
            {
                if (Vector3.Distance(PlayerCar.transform.position, police.transform.position) < MinDistanceToCatch)
                {
                    PoliceCount++;
                }
            }
            UpdateCatchCounter(PoliceCount * CatchPointPerPolice * Time.deltaTime);
        }
        else 
        {
            UpdateCatchCounter(-Time.deltaTime * EscapePointPerSecond);
        }
    }

    private void UpdateCatchCounter(float increment){
        CatchCounter += increment;
        CatchCounter = Mathf.Clamp(CatchCounter, 0, 100);
         
        ChaseBarContainer.SetActive(CatchCounter != 0);
        ChaseBar.fillAmount = CatchCounter / 100;

        if (CatchCounter == 100) GameOver();
    }
}

