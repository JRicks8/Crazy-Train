using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableRendererOnStart : MonoBehaviour
{
    private void Start()
    {
        if (TryGetComponent<Renderer>(out var rend)) rend.enabled = false;
        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer)) spriteRenderer.enabled = false;
    }
}
