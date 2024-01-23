using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicDoor : MonoBehaviour
{
    [Header("Basic Door")]
    private SpriteRenderer sRenderer;

    [SerializeField] private Color closedColor = Color.white;
    [SerializeField] private Color openColor = Color.gray;

    [Header("Lerp Position and Rotation")]
    [SerializeField] private bool useLerp = false;
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private Vector3 closedRotation;

    [SerializeField] private Vector2 openPosition;
    [SerializeField] private Vector3 openRotation;

    [SerializeField] private float transitionTime;

    private Vector2 startLerpPos;
    private Quaternion startLerpRot;

    private Vector2 endLerpPos;
    private Quaternion endLerpRot;

    private float lerpTimer;

    [Space]
    [Header("Door Variables")]
    [SerializeField] private bool isOpen = false;
    private LayerMask openLayer;
    private LayerMask closedLayer;

    private void Awake()
    {
        sRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        lerpTimer = transitionTime;

        closedLayer = LayerMask.NameToLayer("Door");
        openLayer = LayerMask.NameToLayer("NoCollision");

        isOpen = false;
        gameObject.layer = closedLayer;
        sRenderer.color = closedColor;
    }

    private void Update()
    {
        if (lerpTimer >= transitionTime) return;

        lerpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(lerpTimer / transitionTime);

        transform.position = Vector2.Lerp(startLerpPos, endLerpPos, t);
        transform.rotation = Quaternion.Lerp(startLerpRot, endLerpRot, t);
    }

    public void OnDoorInteract()
    {
        if (useLerp)
        {
            lerpTimer = 0.0f;
            startLerpPos = transform.position;
            startLerpRot = transform.rotation;

            if (isOpen) // close the door
            {
                endLerpPos = closedPosition;
                endLerpRot = Quaternion.Euler(closedRotation);
                isOpen = false;
            }
            else // open the door
            {
                endLerpPos = openPosition;
                endLerpRot = Quaternion.Euler(openRotation);
                isOpen = true;
            }
        }
        else
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                gameObject.layer = openLayer;
                sRenderer.color = openColor;
            }
            else
            {
                gameObject.layer = closedLayer;
                sRenderer.color = closedColor;
            }
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
