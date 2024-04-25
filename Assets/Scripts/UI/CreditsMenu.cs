using System.Collections;
using UnityEngine;

public class CreditsMenu : MenuPanel
{
    public static CreditsMenu instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are two credits menu instances.");
            Destroy(gameObject);
        }

        instance = this;
    }
}