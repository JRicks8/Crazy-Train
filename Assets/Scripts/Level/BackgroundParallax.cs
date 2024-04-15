using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [SerializeField] private Transform tracks;
    [SerializeField] private Transform skyGradient;
    [SerializeField] private List<Transform> layers;
    [SerializeField] private List<Transform> duplicates;
    [SerializeField] private List<float> multipliers;
    [SerializeField] private Transform cameraController;

    [SerializeField] private float xParallax;
    [SerializeField] private float imageWidth;

    private Vector2 lastFrameCameraPosition = Vector2.zero;

    // Called via the Unity Event in Camera Controller, after the camera is done positioning.
    public void Parallax()
    {
        Vector2 cameraDelta = (Vector2)cameraController.position - lastFrameCameraPosition;

        tracks.position += new Vector3(xParallax * Time.deltaTime, 0, 0);
        float tracksXDist = cameraController.position.x - tracks.position.x;
        if (tracksXDist > imageWidth)
            tracks.position += new Vector3(imageWidth, 0, 0);

        skyGradient.position += new Vector3(cameraController.position.x - skyGradient.position.x, cameraDelta.y, 0) * 0.99f;

        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].position += (Vector3)cameraDelta * multipliers[i] + new Vector3(xParallax * Time.deltaTime, 0, 0) * (1 - multipliers[i]);

            // Reposition main layers if needed
            float xDist = cameraController.position.x - layers[i].position.x;
            if (xDist < -imageWidth)
                layers[i].position += new Vector3(-imageWidth, 0, 0);
            else if (xDist > imageWidth)
                layers[i].position += new Vector3(imageWidth, 0, 0);

                // Position Duplicates
            if (xDist <= 0)
                duplicates[i].position = layers[i].position + new Vector3(-imageWidth, 0, 0);
            else
                duplicates[i].position = layers[i].position + new Vector3(imageWidth, 0, 0);
        }

        lastFrameCameraPosition = cameraController.position;
    }
}
