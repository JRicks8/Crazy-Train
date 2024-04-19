using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [SerializeField] private Vector2 viewingPosition;
    [SerializeField] private Vector2 hidingPosition;
    [SerializeField] private GameObject content;
    private RectTransform rectTransform;

    private bool transitioning = false;
    private bool showing = true;

    private IEnumerator hideContentCoroutine;
    private IEnumerator showContentCoroutine;
    private IEnumerator transitionSwitchCoroutine;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are two main menu instances.");
            Destroy(gameObject);
        }

        instance = this;
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
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

    public void ShowContent()
    {
        if (transitioning) return;
        if (showing)
        {
            HideContent();
            return;
        }

        showContentCoroutine = UIUtility.LerpPositionHandler(rectTransform, hidingPosition, viewingPosition, 0.25f);
        StartCoroutine(showContentCoroutine);
        transitionSwitchCoroutine = BoolSwitchTimer(0.25f);
        StartCoroutine(transitionSwitchCoroutine);
        showing = true;
    }

    public void HideContent()
    {
        if (!showing || transitioning) return;

        hideContentCoroutine = UIUtility.LerpPositionHandler(rectTransform, viewingPosition, hidingPosition, 0.25f);
        StartCoroutine(hideContentCoroutine);
        transitionSwitchCoroutine = BoolSwitchTimer(0.25f);
        StartCoroutine(transitionSwitchCoroutine);
        showing = false;
    }

    public void ForceHideContent()
    {
        if (!showing) return;
        if (showContentCoroutine != null)
            StopCoroutine(showContentCoroutine);
        if (hideContentCoroutine != null)
            StopCoroutine(hideContentCoroutine);
        if (transitionSwitchCoroutine != null)
            StopCoroutine(transitionSwitchCoroutine);

        hideContentCoroutine = UIUtility.LerpPositionHandler(rectTransform, viewingPosition, hidingPosition, 0.25f);
        StartCoroutine(hideContentCoroutine);
        transitionSwitchCoroutine = BoolSwitchTimer(0.25f);
        StartCoroutine(transitionSwitchCoroutine);
    }

    private IEnumerator BoolSwitchTimer(float duration)
    {
        transitioning = true;
        yield return new WaitForSeconds(duration);
        transitioning = false;
    }
}