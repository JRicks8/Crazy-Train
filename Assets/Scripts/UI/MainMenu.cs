using System.Collections;
using UnityEngine;

public class MainMenu : MenuPanel
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

        showing = true;
    }

    public void OnPlayButtonClicked()
    {
        if (!showing) return;
        OptionsMenu.instance.ForceHideContent();
        CreditsMenu.instance.ForceHideContent();
        ForceHideContent();
        GameController.instance.PlayOpeningSequence();
    }

    public void OnOptionsButtonClicked()
    {
        if (!showing) return;
        CreditsMenu.instance.HideContent();
        OptionsMenu.instance.ShowContent();
    }

    public void OnCreditsButtonClicked()
    {
        if (!showing) return;
        OptionsMenu.instance.HideContent();
        CreditsMenu.instance.ShowContent();
    }

    public void OnQuitButtonClicked()
    {
        if (!showing) return;
        Application.Quit();
    }
}
