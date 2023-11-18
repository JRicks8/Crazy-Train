using System;
using System.Collections.Generic;
using UnityEngine;

// State Machine States
public class Wander : IState
{
    Enemy owner;

    public Wander(Enemy owner) { this.owner = owner; }

    private float wanderTimer = 0.0f;
    private float wanderTimerStart = 5.0f;
    private float wanderTimerVariance = 2.0f;

    void IState.Enter()
    {
        owner.StopPathfinding();
        wanderTimer = wanderTimerStart + wanderTimerVariance * UnityEngine.Random.Range(-1.0f, 1.0f);
    }

    void IState.Execute()
    {
        wanderTimer -= Time.deltaTime;

        if (owner.LookForTarget())
        {
            owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
        } 
        else if (wanderTimer <= 0)
        {
            owner.stateMachine.ChangeState(new Wandering(owner));
        }
    }

    void IState.Exit()
    {
        
    }
}

public class Wandering : IState
{
    Enemy owner;

    public Wandering(Enemy owner) { this.owner = owner; }

    private Vector2 destination;

    void IState.Enter()
    {
        List<PathNode> nodes = owner.GetAllPathfindNodes();
        if (nodes.Count == 0) owner.stateMachine.ChangeState(new Wander(owner));

        destination = nodes[UnityEngine.Random.Range(0, nodes.Count)].transform.position;
        owner.StartPathfinding(destination);
    }

    void IState.Execute()
    {
        if (owner.LookForTarget())
        {
            owner.stateMachine.ChangeState(new InCombatWithTarget(owner));
        }
        else
        {
            owner.MoveAlongPath();
            if (owner.GetCurrentPath().Count == 0) owner.stateMachine.ChangeState(new Wander(owner));
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

    private float moveTimer = 0.0f;
    private float moveTimerThreshold = 2.0f;
    private float moveTimerVariance = 1.0f;

    private float lastSeenTargetTimer = 0.0f;
    private float lastSeenTargetTimeThreshold = 5.0f;
    private Vector2 lastSeenPosition = Vector2.zero;

    void IState.Enter()
    {
        lastSeenPosition = owner.target.position;
    }

    void IState.Execute()
    {
        lastSeenTargetTimer += Time.deltaTime;
        moveTimer += Time.deltaTime;

        if (moveTimer >= moveTimerThreshold)
        {
            moveTimer = 0.0f + moveTimerVariance * UnityEngine.Random.Range(-1.0f, 1.0f);
            // move to a random nearby node
            PathNode n = owner.GetClosestNode();
            int size = n.connections.Count;
            n = n.connections[UnityEngine.Random.Range(0, size - 1)].node;

            owner.UpdatePathfindDestination(n.transform.position);
            if (!owner.pathing) owner.StartPathfinding(n.transform.position);
        }

        if (owner.CanSeeTarget())
        {
            lastSeenTargetTimer = 0.0f;
            lastSeenPosition = owner.target.position;

            owner.Shoot();
        }
        else
        {
            if (lastSeenTargetTimer >= lastSeenTargetTimeThreshold)
            {
                owner.stateMachine.ChangeState(new Wander(owner));
                owner.target = null;
            }
            else 
                owner.UpdatePathfindDestination(lastSeenPosition);
        }

        owner.MoveAlongPath();
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
    public Transform bottom;
    public GameObject bulletPrefab;
    [Header("State Machine")]
    public StateMachine stateMachine;

    public bool pathing => groundPathfinder.currentlyPathfinding;

    private GroundPathfind groundPathfinder;
    private Rigidbody2D rb;
    private float shootTimer = 0;
    private bool movedThisFrame = false;

    private void Start()
    {
        stateMachine = new StateMachine();
        stateMachine.ChangeState(new Wander(this));
        groundPathfinder = GetComponent<GroundPathfind>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        movedThisFrame = false;
        shootTimer += Time.deltaTime;

        grounded = CheckGrounded();

        // don't recalculate pathfinding while airborne
        groundPathfinder.pausePathfinding = !grounded;

        stateMachine.Update();

        // If not attempting to move, actively stifle the velocity to prevent excessive sliding
        if (!movedThisFrame) rb.velocity = new Vector2(0.98f * rb.velocity.x, rb.velocity.y);
    }

    /// <summary>
    /// If there is a gameObject with the "Player" tag, this function sets the target gameObject reference to the found gameObject
    /// and returns true. Else, it returns false.
    /// </summary>
    /// <returns></returns>
    public bool LookForTarget()
    {
        GameObject t = GameObject.FindGameObjectWithTag("Player");
        if (t != null)
        {
            LayerMask mask = LayerMask.GetMask(new string[] { "Friendly", "Terrain" });
            RaycastHit2D hit = Physics2D.Linecast(transform.position, t.transform.position, mask);
            Debug.DrawLine(transform.position, t.transform.position, Color.blue);
            //Debug.Log("Ray Info: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == t)
            {
                //Debug.Log("Successful raycast hit the player gameobject");
                target = t.transform;
                return true;
            }
            //else Debug.Log("Hit distance is <= 0 or the hit collider is not the same as the .");
        }
        return false;
    }

    public bool CanSeeTarget()
    {
        if (target == null) return false;

        LayerMask mask = LayerMask.GetMask(new string[] { "Friendly", "Terrain" });
        RaycastHit2D hit = Physics2D.Linecast(transform.position, target.transform.position, mask);
        if (hit.collider.transform.position == target.position)
        {
            return true;
        }
        return false;
    }

    public void Move(int direction)
    {
        movedThisFrame = true;
        rb.velocity += new Vector2(acceleration * direction * Time.deltaTime * rb.mass, 0);
        if (Math.Abs(rb.velocity.x) > maxHorizontalSpeed)
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed), rb.velocity.y);
        }
    }

    public void Jump()
    {
        if (!grounded) return;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
    }

    public bool CheckGrounded()
    {
        LayerMask mask = LayerMask.GetMask(new string[] { "Terrain" });
        RaycastHit2D hit = Physics2D.Linecast(bottom.position, bottom.position + new Vector3(0, -0.2f), mask);

        if (hit.collider == null) Debug.DrawLine(bottom.position, bottom.position + new Vector3(0, -0.2f), Color.red);
        else Debug.DrawLine(bottom.position, bottom.position + new Vector3(0, -0.2f), Color.green);

        return hit.collider != null;
    }

    /// <summary>
    /// Shoots a bullet projectile towards the target gameObject
    /// </summary>
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

    public void MoveAlongPath()
    {
        if (groundPathfinder.path.Count == 0) return;
        float dist = Vector2.Distance(groundPathfinder.path[0].transform.position, transform.position);
        if (dist <= 0.5f)
        {
            if (groundPathfinder.path.Count > 1)
            {
                List<Connection> connections = groundPathfinder.path[0].connections;
                foreach (Connection connection in connections)
                {
                    if (connection.moveType == MoveType.JUMP && connection.node == groundPathfinder.path[1])
                    {
                        Jump();
                    }
                }
            }

            groundPathfinder.path.RemoveAt(0);
        }
        if (groundPathfinder.path.Count == 0) return;

        float dir = groundPathfinder.path[0].transform.position.x - transform.position.x;
        dir /= Mathf.Abs(dir);
        Move((int)dir);
    }

    public void UpdatePathfindDestination(Vector2 p)
    {
        groundPathfinder.targetPosition = p;
    }

    public void StartPathfinding(Vector2 p)
    {
        if (groundPathfinder == null) return;
        groundPathfinder.StartPathfinding(p);
    }

    public void StopPathfinding()
    {
        if (groundPathfinder != null) groundPathfinder.EndPathfinding();
    }

    public List<PathNode> GetCurrentPath()
    {
        return groundPathfinder.path;
    }

    public List<PathNode> GetAllPathfindNodes()
    {
        return groundPathfinder.pathNodes;
    }

    public PathNode GetClosestNode()
    {
        return groundPathfinder.FindClosestNode(transform.position);
    }
}
