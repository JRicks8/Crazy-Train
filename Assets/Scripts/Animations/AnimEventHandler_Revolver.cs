using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventHandler_Revolver : MonoBehaviour
{
    public Gun_Revolver gun;

    // Functions connect the events fired by the animation to the scripts in the Gun
    public void OnFinishReloadAnimation()
    {
        gun.OnFinishReloadAnimation();
    }

    public void OnFinishShootAnimation()
    {
        gun.OnFinishShootAnimation();
    }
}
