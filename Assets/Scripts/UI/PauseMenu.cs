using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MenuPanel
{
    public static PauseMenu instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are two credits menu instances.");
            Destroy(gameObject);
        }

        instance = this;
    }

    public void OnResumeButtonClicked()
    {
        if (!showing) return;
        ForceHideContent();
        OptionsMenu.instance.ForceHideContent();
        GameController.instance.TogglePause();
    }

    public void OnOptionsButtonClicked()
    {
        if (!showing) return;
        OptionsMenu.instance.ShowContent();
    }

    public void OnQuitButtonClicked()
    {
        if (!showing) return;
        PlayerController.instance.GetHealthScript().SetHealth(0); // Kill the player to send them back to the main menu
        ForceHideContent();
        OptionsMenu.instance.ForceHideContent();
        GameController.instance.TogglePause();
    }
}