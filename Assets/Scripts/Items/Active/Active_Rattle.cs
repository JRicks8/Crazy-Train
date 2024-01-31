using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Active_Rattle : Item
{
    //[Header("Rattle")]

    private void Awake()
    {
        itemInfo = ItemData.RattleInfo;
    }

    public override bool Use(Vector2 direction)
    {
        bool success = base.Use(direction);

        if (success)
        {
            animator.SetTrigger("shootTrigger");
        }

        return success;
    }
}
