using UnityEngine;

public class Weapon_Turret : Item
{
    [Header("Turret")]
    [SerializeField] private GameObject deployedTurretPrefab;
    [SerializeField] private TurretSettings turretSettings;
    [SerializeField] private float throwForce;

    private void Awake()
    {
        itemInfo = ItemData.allItemInfo[(int)ItemData.Items.Turret];
    }

    public override bool Use(Vector2 direction)
    {
        GameObject deployed = Instantiate(deployedTurretPrefab);
        PlayerController playerController = GetComponentInParent<PlayerController>();
        if (deployed.TryGetComponent(out Weapon_Turret_Deployed turretScript) && playerController != null)
        {
            turretScript.Initialize(turretSettings, playerController);
            turretScript.rb.velocity = direction * throwForce;
            turretScript.transform.position = playerController.GetHandLocation();
            Remove(); // Remove the item from the player's inventory
        }
        else
        {
            Destroy(deployed);
        }

        return true;
    }
}
