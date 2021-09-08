using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScripts : MonoBehaviour
{
    public Toggle arMode;
    void Start()
    {
        arMode.isOn = GameManager.instance.GetArMode();

        arMode.onValueChanged.AddListener(delegate {
            ToggleValueChanged(arMode);
        });
    }

    void ToggleValueChanged(Toggle change)
    {
        Debug.Log("arMode is " + arMode.isOn);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void HomeButton(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    public void PlayAIMode(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    public void PlayMultiplayerMode(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
