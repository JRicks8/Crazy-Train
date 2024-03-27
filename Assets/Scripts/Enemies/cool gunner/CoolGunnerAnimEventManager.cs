using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolGunnerAnimEventManager : MonoBehaviour
{
    [SerializeField] private Enemy_CoolGunner parentScript;

    public void OnAnimDeathEnd()
    {
        Destroy(parentScript.gameObject);
    }
}
