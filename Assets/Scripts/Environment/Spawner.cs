using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject SpawnEntity(GameObject entityPrefab)
    {
        if (entityPrefab == null) return null;
        GameObject e = Instantiate(entityPrefab);
        e.transform.position = transform.position;
        return e;
    }
}
