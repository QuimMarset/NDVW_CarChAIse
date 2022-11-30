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
	[SerializeField] protected Image CatchBar;
	[SerializeField] protected TextMeshProUGUI BeingChasedMsg;
	[SerializeField] protected TextMeshProUGUI EscapingMsg;


	public void SetSpeed(float speed, float maxSpeed)
	{
		SpeedBar.fillAmount = Mathf.Abs(speed) / maxSpeed;
		SpeedText.text = (int)speed + " Km/h";
	}

	public void SetHealth(float health, float maxHealth)
	{
		HealthBar.fillAmount = health / maxHealth;
		HealthText.text = (int)health + "%";
	}

	public void SetCatch(float catchCount, float maxCatchCount)
	{
		float newFillAmount = catchCount / maxCatchCount;

		// Message depending if incrementing or decrementing
		BeingChasedMsg.enabled = newFillAmount > CatchBar.fillAmount;
		EscapingMsg.enabled = newFillAmount < CatchBar.fillAmount;

		// Update chase bar
		CatchBar.fillAmount = newFillAmount;

		// Show only if CatchCount > 0
		CatchBarContainer.SetActive(newFillAmount > 0);
	}

}
