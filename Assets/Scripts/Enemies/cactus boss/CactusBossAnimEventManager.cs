using UnityEngine;

public class CactusBossAnimEventManager : MonoBehaviour
{
    [SerializeField] private Enemy_CactusBoss parentScript;

    public void AnimOnJump()
    {
        parentScript.AnimOnJump();
    }

    public void AnimOnLand()
    {
        parentScript.AnimOnLand();
    }

    public void AnimOnJumpFinish()
    {
        parentScript.AnimOnJumpFinish();
    }

    public void AnimWalkCheckpoint1()
    {
        parentScript.AnimWalkCheckpoint1();
    }

    public void AnimWalkCheckpoint2()
    {
        parentScript.AnimWalkCheckpoint2();
    }

    public void OnAttack1Apex()
    {
        parentScript.OnAttack1Apex();
    }

    public void OnAttack2Apex()
    {
        parentScript.OnAttack2Apex();
    }
}
