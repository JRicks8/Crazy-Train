using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private Vector2 viewingPosition;
    [SerializeField] private Vector2 hidingPosition;
    [SerializeField] private GameObject content;
    private RectTransform rectTransform;

    private bool transitioning = false;
    protected bool showing = false;

    private IEnumerator hideContentCoroutine;
    private IEnumerator showContentCoroutine;
    private IEnumerator transitionSwitchCoroutine;

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
        showing = false;
    }

    private IEnumerator BoolSwitchTimer(float duration)
    {
        transitioning = true;
        yield return new WaitForSecondsRealtime(duration);
        transitioning = false;
    }
}
