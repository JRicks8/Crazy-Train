using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCowboyBehavior : StateMachineBehaviour
{

    private enum Parameters
    {
        horizSpeed,
        vertSpeed,
        velocityMag,
        grounded,
        dieTrigger,
        ouchTrigger,
        landTrigger,
    }

    private PlayerMovement pMovement;
    private Health pHealth;

    private Animator playerAnimator;
    private Rigidbody2D pRigidbody;

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        animator.SetFloat(Parameters.horizSpeed.ToString(), pRigidbody.velocity.x);
        animator.SetFloat(Parameters.vertSpeed.ToString(), pRigidbody.velocity.y);
        animator.SetFloat(Parameters.velocityMag.ToString(), pRigidbody.velocity.magnitude);
        animator.SetBool(Parameters.grounded.ToString(), pMovement.IsGrounded());
    }

    public void SetReferences(GameObject player)
    {
        PlayerController pController = player.GetComponent<PlayerController>();

        pRigidbody = player.GetComponent<Rigidbody2D>();
        pHealth = player.GetComponent<Health>();
        pMovement = player.GetComponent<PlayerMovement>();
        playerAnimator = pController.GetAnimator();

        foreach (Component comp in player.GetComponents<Component>())
            Debug.Log(comp.GetType());

        pHealth.OnDeath += OnCowboyDie;
        pHealth.OnDamageTaken += OnCowboyDamageTaken;
        pMovement.OnLand += OnCowboyLand;
    }

    public void OnCowboyDie(GameObject entity)
    {
        playerAnimator.SetTrigger(Parameters.dieTrigger.ToString());
    }

    public void OnCowboyDamageTaken(GameObject entity)
    {
        playerAnimator.SetTrigger(Parameters.ouchTrigger.ToString());
    }

    public void OnCowboyLand(GameObject entity)
    {
        playerAnimator.SetTrigger(Parameters.landTrigger.ToString());
    }
}