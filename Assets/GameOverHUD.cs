using TMPro;
using UnityEngine;

public class GameOverHUD : MonoBehaviour
{
    // Editable parameters
    [SerializeField] private TextMeshProUGUI MessageText;

    public void SetMessage(string text)
	{
        MessageText.text = text;
    }
}
