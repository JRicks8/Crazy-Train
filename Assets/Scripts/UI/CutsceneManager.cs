using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance;

    [SerializeField] private GameObject fakePlayer;
    [SerializeField] private Transform cameraController;

    private IEnumerator openingSequenceHandler;
    private IEnumerator deathSequenceHandler;

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

    public void DoDeathSequence(Vector2 deathPosition)
    {
        if (deathSequenceHandler != null)
            StopCoroutine(deathSequenceHandler);

        deathSequenceHandler = DeathSequenceHandler(deathPosition);
        StartCoroutine(deathSequenceHandler);
    }

    IEnumerator OpeningSequenceHandler()
    {
        Animator fakePlayerAnimator = fakePlayer.GetComponent<Animator>();
        Transform fakePlayerTransform = fakePlayer.transform;

        // Walk back to edge of platform and face right
        fakePlayerAnimator.SetTrigger("runTrigger");
        fakePlayerTransform.localScale = new Vector3(-1, 1, 1);

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

        fakePlayerAnimator.SetTrigger("idleTrigger");
        fakePlayerTransform.localScale = Vector3.one;

        yield return new WaitForSeconds(1.0f);

        // Start running to the right end
        fakePlayerAnimator.SetTrigger("runTrigger");
        
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
        fakePlayerAnimator.SetTrigger("jumpTrigger");

        timer = 0.0f;
        duration = 1.0f;
        destination = new Vector3(8.1875f, 8.7f, 0);
        start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = 1 - Mathf.Pow(1 - (timer / duration), 3);
            float x = Mathf.Lerp(start.x, destination.x, timer / duration);
            float y = Mathf.Lerp(start.y, destination.y, t);
            fakePlayerTransform.localPosition = new Vector2(x, y);
            cameraController.transform.position = fakePlayerTransform.position + new Vector3(0, 1.755f, 0);
            yield return new WaitForEndOfFrame();
        }

        // Fall to the train
        fakePlayerAnimator.SetTrigger("fallTrigger");

        timer = 0.0f;
        duration = 1.0f;
        destination = new Vector3(17.5f, 3.7f, 0);
        start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Pow(timer / duration, 3);
            float x = Mathf.Lerp(start.x, destination.x, timer / duration);
            float y = Mathf.Lerp(start.y, destination.y, t);
            fakePlayerTransform.localPosition = new Vector2(x, y);
            cameraController.transform.position = fakePlayerTransform.position + new Vector3(0, 1.755f, 0);
            yield return new WaitForEndOfFrame();
        }
        fakePlayerTransform.localPosition = new Vector3(-100, 0, 0);

        // Start the game
        GameController.instance.StartGame();
    }

    IEnumerator DeathSequenceHandler(Vector2 deathPosition)
    {
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_EnemiesClosingIn, 0.0f, 2.0f);
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Sound_TrainNoise, 0.0f, 2.0f);
        MusicPlayer.instance.ChangeVolumeGradual(MusicPlayer.Sound.Song_BossLayer, 0.0f, 2.0f);

        Animator fakePlayerAnimator = fakePlayer.GetComponent<Animator>();
        Transform fakePlayerTransform = fakePlayer.transform;

        // Walk back to edge of platform and face right
        fakePlayerAnimator.SetTrigger("ouchTrigger");
        fakePlayerTransform.position = deathPosition + new Vector2(0, 0.5f);
        cameraController.transform.position = new Vector2(fakePlayerTransform.position.x, -1.87f);

        yield return new WaitForSeconds(1.0f);

        MusicPlayer.instance.PlaySoundOneShot(MusicPlayer.Sound.Sound_DeathFalling);

        float timer = 0.0f;
        float duration = 1.0f;
        Vector3 destination = new Vector3(fakePlayerTransform.localPosition.x / 2, 9.0f, 0);
        Vector3 start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = 1 - Mathf.Pow(1 - (timer / duration), 3);
            float x = Mathf.Lerp(start.x, destination.x, timer / duration);
            float y = Mathf.Lerp(start.y, destination.y, t);
            fakePlayerTransform.localPosition = new Vector2(x, y);

            fakePlayerTransform.Rotate(new Vector3(0, 0, 1.5f));

            cameraController.transform.position = new Vector2(fakePlayerTransform.position.x, -1.87f);
            yield return new WaitForEndOfFrame();
        }
        fakePlayerTransform.localPosition = destination;

        timer = 0.0f;
        duration = 1.0f;
        destination = new Vector3(0, 2.0f, 0);
        start = fakePlayerTransform.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Pow(timer / duration, 3);
            float x = Mathf.Lerp(start.x, destination.x, timer / duration);
            float y = Mathf.Lerp(start.y, destination.y, t);
            fakePlayerTransform.localPosition = new Vector2(x, y);

            fakePlayerTransform.Rotate(new Vector3(0, 0, 1.5f));

            cameraController.transform.position = new Vector2(fakePlayerTransform.position.x, -1.87f);
            yield return new WaitForEndOfFrame();
        }
        fakePlayerTransform.localPosition = destination;
        fakePlayerTransform.rotation = Quaternion.identity;

        fakePlayerAnimator.SetTrigger("idleTrigger");

        GameController.instance.ResetGame();
    }
}
