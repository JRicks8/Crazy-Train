using System;
using System.Collections.Generic;
using UnityEngine;

// Acts as a base for any character
// Should not be attached to any character directly, this class is meant to be inherited (See Enemy_Gunner.cs for example)
public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration;
    public float jumpPower;
    public float maxHorizontalSpeed;
    public bool grounded;
    [Header("Other Settings")]
    public float handOffset;
    [Header("Object References")]
    public Transform target;
    public Transform bottom;
    public Transform middle;
    public Transform hand;
    public Gun equippedGun;
    public Rigidbody2D rb;
    public SpriteRenderer sRenderer;
    [Header("Pathfinding")]
    public float satisfiedNodeDistance = 0.5f;

    [HideInInspector]
    public GroundPathfind groundPathfinder;
    private bool movedThisFrame = false;
    private bool facingLeft = false;

    protected void Initialize()
    {
        groundPathfinder = GetComponent<GroundPathfind>();
        rb = GetComponent<Rigidbody2D>();

        equippedGun = GetComponentInChildren<Gun_Revolver>();
        if (!equippedGun.info.showHand) hand.gameObject.SetActive(false);
        equippedGun.SetReferences(this);
    }

    protected void UpdateCharacter()
    {
        movedThisFrame = false;

        grounded = CheckGrounded();

        // don't recalculate pathfinding while airborne
        groundPathfinder.pausePathfinding = !grounded;

        UpdateHandAndGun();

        // If not attempting to move, actively stifle the velocity to prevent excessive sliding
        if (!movedThisFrame) rb.velocity = new Vector2(0.98f * rb.velocity.x, rb.velocity.y);

        if (target != null) // if there is a target, always face the target
        {
            float difX = (target.position - middle.position).x;
            if (difX < -0.1f) facingLeft = true;
            else if (difX > 0.1f) facingLeft = false;
        }
        else // no target, so just face where we're moving
        {
            if (rb.velocity.x > 0.1f) facingLeft = false;
            else if (rb.velocity.x < -0.1f) facingLeft = true;
        }
        sRenderer.flipX = facingLeft;
    }

    private void UpdateHandAndGun()
    {
        // Update gun
        equippedGun.UpdateGun();
        equippedGun.transform.position = hand.position;
        equippedGun.sRenderer.flipY = facingLeft;

        // Conditional updates
        if (target != null) // We have a target
        {
            // Face the gun towards the target
            equippedGun.transform.rotation = Quaternion.LookRotation(Vector3.forward, (target.position - equippedGun.transform.position).normalized) * Quaternion.Euler(0, 0, 90);

            // Depending on the target location, change the position of the hand
            float difX = (target.position - middle.position).x;
            if (difX < -0.1f) hand.localPosition = new Vector2(handOffset * -1, 0);
            else if (difX > 0.1f) hand.localPosition = new Vector2(handOffset, 0);
        }
        else // We don't have a target
        {
            // Neutral aiming position
            equippedGun.transform.rotation = Quaternion.identity;

            // Neutral hand position
            if (facingLeft) hand.localPosition = new Vector2(handOffset * -1, 0);
            else hand.localPosition = new Vector2(handOffset, 0);
        }
    }

    /// <summary>
    /// If there is a gameObject with the "Player" tag, this function sets the target gameObject reference to the found gameObject
    /// and returns true. Else, it returns false.
    /// </summary>
    /// <returns></returns>
    public bool LookForTarget()
    {
        GameObject t = GameObject.FindGameObjectWithTag("Player");
        if (t != null)
        {
            LayerMask mask = LayerMask.GetMask(new string[] { "Friendly", "Terrain" });
            RaycastHit2D hit = Physics2D.Linecast(transform.position, t.transform.position, mask);
            Debug.DrawLine(transform.position, t.transform.position, Color.blue);
            //Debug.Log("Ray Info: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == t)
            {
                //Debug.Log("Successful raycast hit the player gameobject");
                target = t.transform;
                return true;
            }
            //else Debug.Log("Hit distance is <= 0 or the hit collider is not the same as the .");
        }
        return false;
    }

    public bool CanSeeTarget()
    {
        if (target == null) return false;

        LayerMask mask = LayerMask.GetMask(new string[] { "Friendly", "Terrain" });
        RaycastHit2D hit = Physics2D.Linecast(transform.position, target.transform.position, mask);
        if (hit.collider.transform.position == target.position)
        {
            return true;
        }
        return false;
    }

    public void Move(int direction)
    {
        movedThisFrame = true;
        rb.velocity += new Vector2(acceleration * direction * Time.deltaTime * rb.mass, 0);
        if (Math.Abs(rb.velocity.x) > maxHorizontalSpeed)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed), rb.velocity.y);
        }
    }

    public void Jump()
    {
        if (!grounded) return;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
    }

    public bool CheckGrounded()
    {
        LayerMask mask = LayerMask.GetMask(new string[] { "Terrain" });
        RaycastHit2D hit = Physics2D.Linecast(bottom.position, bottom.position + new Vector3(0, -0.2f), mask);

        if (hit.collider == null) Debug.DrawLine(bottom.position, bottom.position + new Vector3(0, -0.2f), Color.red);
        else Debug.DrawLine(bottom.position, bottom.position + new Vector3(0, -0.2f), Color.green);

        return hit.collider != null;
    }
}
