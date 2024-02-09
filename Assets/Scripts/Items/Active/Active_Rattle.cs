using UnityEngine;

public class Active_Rattle : Item
{
    [Header("Rattle")]
    [SerializeField] private float cooldownTime;
    [SerializeField] private float fearDuration;
    [SerializeField] private float fearRange;
    [SerializeField] private LayerMask fearableObjects;

    private float cooldownTimer = 0.0f;

    private void Awake()
    {
        sRenderer.enabled = false; // Don't show the snake rattle
        itemInfo = ItemData.RattleInfo;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public override void UpdateItem(Vector2 aimPoint, Vector2 aimingFrom, Vector2 handPosition, bool handOnLeft)
    {
        base.UpdateItem(aimPoint, aimingFrom, handPosition, handOnLeft);
    }

    public override bool Use(Vector2 direction)
    {
        if (cooldownTimer <= 0)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, fearRange, fearableObjects);

            foreach (Collider2D collider in colliders)
            {
                if (collider.TryGetComponent(out Character c))
                {
                    c.GiveEffect(Effect.Fear, fearDuration);
                }
            }

            Debug.Log("Successful Use");
            cooldownTimer = cooldownTime;
            return true;
        }
        return false;
    }
}
