using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCrowBehavior : StateMachineBehaviour
{
    public enum Parameters
    {
        dieTrigger,
    }

    private Animator crowAnimator;
    private Health healthScript;

    public void SetReferences(GameObject crow, Animator animator)
    {
        healthScript = crow.GetComponent<Health>();
        crowAnimator = animator;

        healthScript.OnDeath += OnCrowDie;
    }

    private void OnCrowDie(GameObject enemy)
    {
        crowAnimator.SetTrigger(Parameters.dieTrigger.ToString());
    }
}
