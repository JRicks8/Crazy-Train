using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;
    [SerializeField] private float Thresholdx;
    [SerializeField] private float Thresholdy;

    // Start is called before the first frame update
    void Update()
    {
        AimLogic();
    }

    private void AimLogic()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 midPos = (player.position + mousePos) / 2;

        midPos.x = Mathf.Clamp(midPos.x, -Thresholdx + player.position.x, Thresholdx + player.position.x);
        midPos.y = Mathf.Clamp(midPos.y, -Thresholdy + player.position.y, Thresholdy + player.position.y);

        this.transform.position = midPos;
    }
}
