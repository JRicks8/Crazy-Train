using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OpeningSequenceManager : MonoBehaviour
{
    public static OpeningSequenceManager instance;

    [SerializeField] private GameObject fakePlayer;
    [SerializeField] private Transform cameraController;

    private IEnumerator openingSequenceHandler;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are two OpeningSequenceManagers in this scene.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void DoOpeningSequence()
    {
        if (openingSequenceHandler != null) 
            StopCoroutine(openingSequenceHandler);

        openingSequenceHandler = OpeningSequenceHandler();
        StartCoroutine(openingSequenceHandler);
    }

    IEnumerator OpeningSequenceHandler()
    {
        Animator fakePlayerAnimator = fakePlayer.GetComponent<Animator>();
        Transform fakePlayerTransform = fakePlayer.transform;

        // Walk back to edge of platform and face right
        fakePlayerAnimator.SetTrigger("runLeftTrigger");

        float timer = 0.0f;
        float duration = 1.0f;
        Vector3 destination = new Vector3(-1.125f, 2.0f, 0);
        Vector3 start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fakePlayerTransform.localPosition = Vector3.Lerp(start, destination, timer / duration);
            cameraController.transform.position = fakePlayerTransform.position + new Vector3(0, 1.755f, 0);
            yield return new WaitForEndOfFrame();
        }
        fakePlayerTransform.localPosition = destination;

        fakePlayerAnimator.SetTrigger("idleRightTrigger");

        // Start running to the right end
        fakePlayerAnimator.SetTrigger("runRightTrigger");

        timer = 0.0f;
        duration = 0.5f;
        destination = new Vector3(1.125f, 2.0f, 0);
        start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fakePlayerTransform.localPosition = Vector3.Lerp(start, destination, timer / duration);
            cameraController.transform.position = fakePlayerTransform.position + new Vector3(0, 1.755f, 0);
            yield return new WaitForEndOfFrame();
        }
        fakePlayerTransform.localPosition = destination;

        // Jump at edge, making an arc until the apex
        fakePlayerAnimator.SetTrigger("jumpRightTrigger");

        timer = 0.0f;
        duration = 2.0f;
        destination = new Vector3(8.1875f, 8.7f, 0);
        start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = 1 - Mathf.Pow(1 - (timer / duration), 3);
            fakePlayerTransform.localPosition = Vector3.Lerp(start, destination, t);
            cameraController.transform.position = fakePlayerTransform.position + new Vector3(0, 1.755f, 0);
            yield return new WaitForEndOfFrame();
        }

        // Fall to the train
        fakePlayerAnimator.SetTrigger("jumpRightTrigger");

        timer = 0.0f;
        duration = 2.0f;
        destination = new Vector3(17.5f, 3.7f, 0);
        start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Pow(timer / duration, 3);
            fakePlayerTransform.localPosition = Vector3.Lerp(start, destination, t);
            cameraController.transform.position = fakePlayerTransform.position + new Vector3(0, 1.755f, 0);
            yield return new WaitForEndOfFrame();
        }
        fakePlayerTransform.localPosition = new Vector3(-100, 0, 0);

        // Start the game
        GameController.instance.StartGame();
    }
}
