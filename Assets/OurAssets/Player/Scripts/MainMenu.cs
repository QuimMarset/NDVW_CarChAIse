using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Editable parameters
    [SerializeField] protected int PlaySceneBuildIndex;

    public void Play()
	{
		SceneManager.LoadScene(PlaySceneBuildIndex);
	}

    public void Exit()
	{
		Debug.Log("Exit game");
		Application.Quit();
	}
}
