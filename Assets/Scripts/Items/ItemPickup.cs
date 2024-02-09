using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemPickup : MonoBehaviour
{
    [Header("Item To Pickup")]
    [SerializeField] private GameObject itemPrefab;
    [Space]
    [Header("Pickup Attributes")]
    [Range(1f, 10f)] public float floatingHeight;
    [Range(1f, 50f)] public float floatingForce;
    [SerializeField] private float price; // Applicable if this pickup is marked as a shop item
    // Use the linear drag variable in the rigidbody to effect the damping

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    private Rigidbody2D rb;

    private UnityAction<SpriteRenderer> aSpriteChanged;

    private RaycastHit2D ray;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (itemPrefab != null) 
            spriteRenderer.sprite = itemPrefab.GetComponent<Item>().sRenderer.sprite;

        aSpriteChanged += RecalculateCollision;
        spriteRenderer.RegisterSpriteChangeCallback(aSpriteChanged);
    }

    private void Start()
    {
        RecalculateCollision(spriteRenderer);
    }

    private void FixedUpdate()
    {
        LayerMask mask = LayerMask.GetMask(new string[] { "Terrain", "OneWayTerrain" });
        ray = Physics2D.Raycast(transform.position, Vector2.down, floatingHeight, mask);

        if (ray.transform != null)
        {
            if (ray.distance <= 0.1f)
            {
                rb.velocity = new Vector2(rb.velocity.x, 1.0f);
            }
            else
            {
                Vector2 force = new Vector2(0, 1 / (ray.distance / floatingHeight) * floatingForce) * rb.mass;
                rb.AddForce(force);
            }
        }
    }

    private void RecalculateCollision(SpriteRenderer sr)
    {
        Sprite sprite = sr.sprite;
        if (sprite == null) return;

        for (int i = 0; i < polygonCollider.pathCount; i++) polygonCollider.pathCount = 0;
        polygonCollider.pathCount = sprite.GetPhysicsShapeCount();

        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            path.Clear();
            sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }
    }

    public void SetItemPrefab(GameObject gun)
    {
        itemPrefab = gun;
        spriteRenderer.sprite = itemPrefab.GetComponent<Item>().sRenderer.sprite;
    }

    public GameObject GetItemPrefab()
    {
        return itemPrefab;
    }

    public void SetPrice(float value)
    {
        price = value;
    }

    public float GetPrice()
    {
        return price;
    }
}
