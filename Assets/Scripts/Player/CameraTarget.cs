using System.Collections;
using TMPro;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float maxXOffset;
    [SerializeField] private float maxYOffset;
    [SerializeField][Range(0f, 1f)] private float intensity;

    private IEnumerator lookForPlayer;

    private void Awake()
    {
        lookForPlayer = LookForPlayerRecursive();
        StartCoroutine(lookForPlayer);
    }

    IEnumerator LookForPlayerRecursive()
    {
        while (player == null) 
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 mouseOffset = (Vector2)Input.mousePosition - screenCenter;
        mouseOffset = new Vector2(mouseOffset.x / screenCenter.x, mouseOffset.y / screenCenter.y);

        Vector2 desiredPosition = mouseOffset * new Vector2(maxXOffset, maxYOffset) + (Vector2)player.position;
        Vector2 lerpedPosition = Vector2.Lerp(player.position, desiredPosition, intensity);
        transform.position = lerpedPosition;
    }
}
