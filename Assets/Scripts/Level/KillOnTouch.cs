using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnTouch : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (PlayerController.instance == null) return;

        if (collision.gameObject.TryGetComponent(out PlayerController playerController))
        {
            collision.gameObject.GetComponent<Health>().TakeDamage(1.0f);
            collision.gameObject.AddComponent<BounceUp>();
            return;
        }

        if (collision.gameObject.TryGetComponent(out Health healthScript) && healthScript.GetHealth() > 0)
        {
            healthScript.SetHealth(0.0f);
        }

        if (collision.gameObject.TryGetComponent(out Rigidbody2D rb))
        {
            rb.AddForce(new Vector2(-100.0f, 0.0f));
        }
    }
}
