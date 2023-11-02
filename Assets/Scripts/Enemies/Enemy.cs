using System;
using System.ComponentModel;
using UnityEngine;

// State Machine States
public class Wander : IState
{
    Enemy owner;

    public Wander(Enemy owner) { this.owner = owner; }

    void IState.Enter()
    {
        
    }

    void IState.Execute()
    {
        if (owner.LookForTarget())
        {
            owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
        }
    }

    void IState.Exit()
    {
        
    }
}

public class MovingToDestination : IState
{
    Enemy owner;

    public MovingToDestination(Enemy owner) { this.owner = owner; }

    void IState.Enter()
    {

    }

    void IState.Execute()
    {
        if (owner.LookForTarget())
        {
            owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
        }
    }

    void IState.Exit()
    {

    }
}

public class InCombatWithTarget : IState
{
    Enemy owner;

    public InCombatWithTarget(Enemy owner) { this.owner = owner; }

    void IState.Enter()
    {

    }

    void IState.Execute()
    {
        owner.Shoot();
    }

    void IState.Exit()
    {

    }
}

// Main Class
public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration;
    public float jumpPower;
    public float maxHorizontalSpeed;
    private bool grounded;
    [Header("Combat Settings")]
    public float bulletSpeed;
    public float fireRate; // per second
    [Header("Object References")]
    public Transform target;
    public GameObject bulletPrefab;
    [Header("State Machine")]
    public StateMachine stateMachine;

    private Rigidbody2D rb;
    private float shootTimer = 0;
    private Vector2 moveDestination = Vector2.zero;

    private void Start()
    {
        stateMachine = new StateMachine();
        stateMachine.ChangeState(new Wander(this));
    }

    private void Update()
    {
        shootTimer += Time.deltaTime;

        stateMachine.Update();
    }

    public bool LookForTarget()
    {
        GameObject t = GameObject.FindGameObjectWithTag("Player");
        if (t != null)
        {
            target = t.transform;
            return true;
        }
        return false;
    }

    public void Move(int direction)
    {
        
    }

    public void Jump()
    {
        if (!grounded) return;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
    }

    public void Shoot()
    {
        if (shootTimer < 1 / fireRate) return;
        shootTimer = 0;

        GameObject b = Instantiate(bulletPrefab);
        Bullet bulletScript = b.GetComponent<Bullet>();
        Vector2 bulletVelocity = (target.position - transform.position).normalized * bulletSpeed;
        bulletScript.AddIgnoreTag("Enemy");
        bulletScript.AddHitTag("Player");

        b.layer = LayerMask.NameToLayer("EnemyProjectile");
        b.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(Vector3.forward, (target.position - transform.position).normalized) * Quaternion.Euler(0, 0, 90));
        b.SetActive(true);
        bulletScript.SetVelocity(bulletVelocity);
    }
}
