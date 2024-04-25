using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MenuPanel
{
    public static OptionsMenu instance;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle vSyncToggle;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are two option menu instances.");
            Destroy(gameObject);
        }

        instance = this;
    }

    public void OnChangeVolumeSlider()
    {
        if (!showing) return;
        AudioListener.volume = volumeSlider.value;
    }

    public void OnToggleVSync()
    {
        if (!showing) return;
        if (vSyncToggle.isOn)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
    }
}
