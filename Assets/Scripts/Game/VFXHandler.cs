using UnityEngine;

public class VFXHandler : MonoBehaviour
{
    private Sprite[] sheet;
    private SpriteRenderer sRenderer;

    private readonly float frameLength = 0.06f;
    private float replayTimer = 0.0f;
    private float lifeTimer = 0.0f;
    private float duration;
    private bool shouldLoop;

    private void Update()
    {
        replayTimer += Time.deltaTime;
        lifeTimer += Time.deltaTime;

        int frameIndex = Mathf.FloorToInt(replayTimer / frameLength);
        if (frameIndex >= sheet.Length)
        {
            if (shouldLoop)
            {
                replayTimer -= frameLength * sheet.Length;
                frameIndex = 0;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        sRenderer.sprite = sheet[frameIndex];

        if (shouldLoop && lifeTimer >= duration)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Sprite[] sheet, bool shouldLoop = false, float duration = 0.0f)
    {
        this.sheet = sheet;
        sRenderer = gameObject.AddComponent<SpriteRenderer>();
        sRenderer.sortingLayerName = "VFX";
        sRenderer.sprite = sheet[0];

        this.shouldLoop = shouldLoop;
        this.duration = duration;
    }
}
