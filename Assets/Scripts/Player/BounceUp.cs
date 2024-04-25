using UnityEngine;

public class BounceUp : MonoBehaviour
{
    public float bounceForce = 700.0f;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("NoCollision");
    }

    private void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, bounceForce));
    }

    private void Update()
    {
        if (GetComponent<Rigidbody2D>().velocity.y <= 0) 
        {
            gameObject.layer = LayerMask.NameToLayer("Friendly");
            Destroy(this);
        }
    }
}
