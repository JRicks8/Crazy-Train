using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private Dictionary<int, float> cooldowns = new();
    [SerializeField] private float cooldownThres = 0.2f;
    private IEnumerator autoEnableCollision;
    private Collider2D coll;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        List<int> keysToRemove = new List<int>();

        foreach (var item in cooldowns)
        {
            if (Time.time - item.Value >= cooldownThres)
            {
                keysToRemove.Add(item.Key);
            }
        }

        for (int i = 0; i < keysToRemove.Count; i++)
        {
            cooldowns.Remove(keysToRemove[i]);
        }
    }

    // Disables collision with the target gameobject, effectively causing them to fall through.
    public void DisableCollision(GameObject requestor, bool autoEnable = false, float noCollisionDuration = 0.2f)
    {
        int instanceID = requestor.GetInstanceID();
        // If the requestor has already requested no collision
        if (cooldowns.TryGetValue(instanceID, out float _))
        {
            return;
        }

        cooldowns.Add(instanceID, Time.time);

        Collider2D[] requestorColliders = requestor.GetComponents<Collider2D>();
        for (int i = 0; i < requestorColliders.Length; i++) 
            Physics2D.IgnoreCollision(coll, requestorColliders[i], true);

        if (autoEnable)
        {
            autoEnableCollision = AutoEnableCollision(requestor, noCollisionDuration);
            StartCoroutine(autoEnableCollision);
        }
    }
    
    public void EnableCollision(GameObject requestor)
    {
        if (requestor == null) return;
        Collider2D[] requestorColliders = requestor.GetComponents<Collider2D>();
        for (int i = 0; i < requestorColliders.Length; i++)
            Physics2D.IgnoreCollision(coll, requestorColliders[i], false);
    }

    private IEnumerator AutoEnableCollision(GameObject requestor, float duration)
    {
        yield return new WaitForSeconds(duration);
        EnableCollision(requestor);
    }
}
