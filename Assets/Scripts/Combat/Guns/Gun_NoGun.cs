using Unity.VisualScripting;
using UnityEngine;

// The gun to equip when there is no gun to equip.
public class Gun_NoGun : Item
{
    private void Awake()
    {
        info = GunData.NoGunInfo;
    }
}
