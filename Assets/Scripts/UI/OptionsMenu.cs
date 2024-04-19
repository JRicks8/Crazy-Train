using System.Collections;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu instance;

    [SerializeField] private Vector2 viewingPosition;
    [SerializeField] private Vector2 hidingPosition;
    [SerializeField] private GameObject content;
    private RectTransform rectTransform;

    private bool transitioning = false;
    private bool showing = false;

    private IEnumerator hideContentCoroutine;
    private IEnumerator showContentCoroutine;
    private IEnumerator transitionSwitchCoroutine;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are two option menu instances.");
            Destroy(gameObject);
        }

        instance = this;
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
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
