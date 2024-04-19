using System.Collections;
using UnityEngine;

public class UIUtility : MonoBehaviour
{
    public static IEnumerator LerpPositionHandler(RectTransform rectToMove, Vector2 start, Vector2 end, float duration)
    {
        rectToMove.localPosition = start;
        float timer = 0.0f;

        while (timer < duration)
        {
            float t = Mathf.Pow(timer / duration, 3);
            rectToMove.localPosition = Vector2.Lerp(start, end, t);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        rectToMove.localPosition = end;
    }
}
