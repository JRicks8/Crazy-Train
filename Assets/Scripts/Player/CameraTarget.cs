using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;
    [SerializeField] private float maxXOffset;
    [SerializeField] private float maxYOffset;
    [SerializeField][Range(0f, 1f)] private float intensity;

    private void Awake()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (player == null) return;
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 mouseOffset = (Vector2)Input.mousePosition - screenCenter;
        mouseOffset = new Vector2(mouseOffset.x / screenCenter.x, mouseOffset.y / screenCenter.y);

        Vector2 desiredPosition = mouseOffset * new Vector2(maxXOffset, maxYOffset) + (Vector2)player.position;
        transform.position = Vector2.Lerp(player.position, desiredPosition, intensity);
    }
}
