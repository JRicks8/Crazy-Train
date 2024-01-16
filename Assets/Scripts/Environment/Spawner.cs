using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool SpawnEntity(GameObject entityPrefab)
    {
        if (entityPrefab == null) return false;
        GameObject e = Instantiate(entityPrefab);
        e.transform.position = transform.position;
        return true;
    }
}
