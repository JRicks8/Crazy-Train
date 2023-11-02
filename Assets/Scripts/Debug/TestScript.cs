using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// testing for OnDeath delegate
public class TestScript : MonoBehaviour
{
    public Health healthScript;

    private void Start()
    {
        healthScript.OnDeath += OnEnemyDeath;
        healthScript.SetHealth(0);
    }

    void OnEnemyDeath(GameObject e)
    {
        Debug.Log("Woohoo! Enemy Died!");
    }
}
