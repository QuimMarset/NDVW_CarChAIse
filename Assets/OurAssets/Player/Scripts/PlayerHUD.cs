using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
	// Editable parameters
	[Header("Speed")]
	[SerializeField] protected Image SpeedBar;
	[SerializeField] protected TextMeshProUGUI SpeedText;

	[Header("Health")]
	[SerializeField] protected Image HealthBar;
	[SerializeField] protected TextMeshProUGUI HealthText;

	[Header("Catch bar")]
	[SerializeField] protected GameObject CatchBarContainer;
	[SerializeField] protected Image[] CatchBars;
	[SerializeField] protected Color MinColorCatchBar = Color.blue;
	[SerializeField] protected Color MaxColorCatchBar = Color.red;
	[SerializeField] protected Image PoliceLights;
	[SerializeField] protected float LightsUpdPerCatchCount = 0.01f;


	public void SetSpeed(float speed, float maxSpeed)
	{
		SpeedBar.fillAmount = Mathf.Abs(speed) / maxSpeed;
		SpeedText.text = (int)speed + " Km/h";
	}

	public void SetHealth(float health, float maxHealth)
	{
		HealthBar.fillAmount = health / maxHealth;
		HealthText.text = (int)health + " HP";
	}

	public void SetCatch(float catchCount, float maxCatchCount)
	{
		foreach (Image catchBar in CatchBars)
		{
			catchBar.fillAmount = catchCount / maxCatchCount;
			catchBar.color = Color.Lerp(MinColorCatchBar, MaxColorCatchBar, catchCount / maxCatchCount);
		}		

		// Disable catch bar if no catch count
		CatchBarContainer.SetActive(catchCount != 0);

		// Make police lights blink
		//PoliceLights.fillAmount = ((catchCount * Time.realtimeSinceStartup * LightsUpdPerCatchCount) % 100) / 100;
	}

}
