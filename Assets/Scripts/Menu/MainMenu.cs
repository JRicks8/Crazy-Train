using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are two main menu instances.");
            Destroy(gameObject);
        }

        instance = this;
    }

    public void OnPlayButtonClicked()
    {
        Hide();
        GameController.instance.StartGame();
    }

    public void OnOptionsButtonClicked()
    {

    }

    public void OnCreditsButtonClicked()
    {

    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
