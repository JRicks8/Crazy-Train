using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GroundPathfind : MonoBehaviour
{
    [SerializeField] private List<PathNode> path = new List<PathNode>();

    [SerializeField] private Vector2 targetPosition;

    private float recalculateCooldown = 0.5f;
    [SerializeField] private bool continuouslyUpdate = true;
    [SerializeField] private bool isPathfinding = false;
    [SerializeField] private bool isPaused = false;

    private IEnumerator pathFindCoroutine;

    private void Awake()
    {
        pathFindCoroutine = PathFindCoroutine();
    }

    public void StartPathfinding()
    {
        StartCoroutine(pathFindCoroutine);
    }

    public void EndPathfinding()
    {
        StopCoroutine(pathFindCoroutine);
    }

    IEnumerator PathFindCoroutine()
    {
        if (isPathfinding) yield break;
        isPathfinding = true;
        //Debug.Log("Starting Pathfind Coroutine");
        while (true)
        {
            // Wait for the pathnode data
            while (GameController.pathNodes.Count == 0) yield return new WaitForSeconds(recalculateCooldown);
            PathNode startNode = FindClosestNode(transform.position);
            PathNode endNode = FindClosestNode(targetPosition);
            List<PathNode> openList = new List<PathNode>();
            List<PathNode> closedList = new List<PathNode>();

            //Debug.Log("start position is at " + transform.position);
            //Debug.Log("end position is at " + targetPosition);
            //Debug.Log("start node is at position " + startNode.transform.position);
            //Debug.Log("end node is at position " + endNode.transform.position);

            if (startNode == null || endNode == null)
            {
                Debug.LogError("Error: start node, end node, or pathnodes data is null. Aborting pathfinding.");
                isPathfinding = false;
                StopCoroutine(pathFindCoroutine);
                yield break;
            }
            openList.Add(startNode);
            PathNode currentNode = openList.First();

            while (openList.Count > 0)
            {
                //Debug.Log("Next Iteration");
                // first, find the closest node to the current node
                float shortestPath = float.MaxValue;

                foreach (PathNode node in openList)
                {
                    float distance = Vector2.Distance(endNode.transform.position, node.transform.position);

                    if (distance < shortestPath)
                    {
                        shortestPath = distance;
                        currentNode = node;
                    }
                }
                //Debug.Log("current node is at position " + currentNode.transform.position);

                // remove current node from open list and add to closed list
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // check if current node is the end node
                if (currentNode == endNode)
                {
                    //Debug.Log("Found the end!");
                    // Traverse backwards to get the path
                    path.Clear();

                    startNode.parent = null;
                    PathNode node = endNode;
                    path.Add(node);

                    while (node.parent != null)
                    {
                        node.g = 0;
                        node.h = 0;
                        path.Add(node.parent);
                        node = node.parent;
                    }
                    //Debug.LogWarning("Size of path: " + path.Count);
                    path.Reverse();
                    foreach (PathNode n in path) n.parent = null;
                    break;
                }

                // for every node connected to the current node
                //Debug.Log("Adding connected nodes to open list");
                foreach (Connection conn in currentNode.connections)
                {
                    // if the connected node is in the closed list, skip
                    if (closedList.Contains(conn.node)) continue;
                    //Debug.Log("Node at position " + conn.node.transform.position + " added to the open list");

                    conn.node.parent = currentNode;

                    // calculate g & h for connected node
                    conn.node.g = currentNode.g + Vector2.Distance(currentNode.transform.position, conn.node.transform.position);
                    conn.node.h = Vector2.Distance(currentNode.transform.position, endNode.transform.position);

                    // add the connected node to the open list
                    openList.Add(conn.node);
                }
            }

            Debug.Log("Done pathfinding.");
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i].transform.position, path[i + 1].transform.position, Color.red, 0.5f);
            }

            //Debug.Log("Time Taken: " + (Time.time - timeStart));

            if (continuouslyUpdate)
                yield return new WaitForSeconds(recalculateCooldown);
            else
                break;

            while (isPaused) yield return new WaitForSeconds(recalculateCooldown);
        }
        isPathfinding = false;
    }

    /// <summary>
    /// Finds the closest node in the node data.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public PathNode FindClosestNode(Vector2 p)
    {
        if (GameController.pathNodes.Count == 0) return null;
        PathNode closest = GameController.pathNodes[0];
        float closestDistance = ((Vector2)closest.transform.position - p).magnitude;
        foreach (PathNode n in GameController.pathNodes)
        {
            float dist = ((Vector2)n.transform.position - p).magnitude;
            if (dist < closestDistance)
            {
                closest = n;
                closestDistance = dist;
            }
        }
        return closest;
    }

    public PathNode FindFurthestNode(Vector2 p)
    {
        if (GameController.pathNodes.Count == 0) return null;
        PathNode furthest = GameController.pathNodes[0];
        float furthestDistance = ((Vector2)furthest.transform.position - p).magnitude;
        foreach (PathNode n in GameController.pathNodes)
        {
            float dist = ((Vector2)n.transform.position - p).magnitude;
            if (dist > furthestDistance)
            {
                furthest = n;
                furthestDistance = dist;
            }
        }
        return furthest;
    }

    // Moves along the path. Return value is whether we've reached the target destination or not
    public bool MoveAlongPath(Character character, Transform satisfiedDistComparator, float satisfiedDist)
    {
        if (path == null)
        {
            Debug.LogError("Attempt to move along path has failed: Path is null!");
            return false;
        }
        if (path.Count == 0) return true;
        float dist = Vector2.Distance(path[0].transform.position, satisfiedDistComparator.position);
        if (dist <= satisfiedDist)
        {
            if (path.Count > 1)
            {
                List<Connection> connections = path[0].connections;
                foreach (Connection connection in connections)
                {
                    if (connection.node == path[1])
                    {
                        GameObject currentGround = character.GetGround();
                        if (connection.moveType == MoveType.JUMP)
                        {
                            float jumpPower = (path[1].transform.position - path[0].transform.position).magnitude * 3;
                            Debug.Log(jumpPower);
                            character.Jump(jumpPower);
                        }
                        else if (connection.node.transform.position.y < path[0].transform.position.y
                            && currentGround != null
                            && currentGround.TryGetComponent(out OneWayPlatform p))
                        {
                            p.DisableCollision(character.gameObject, true, 0.4f);
                        }
                    }
                }
            }

            path.RemoveAt(0);
        }
        if (path.Count == 0) return true;

        // If our x distance is greater than a certain threshold, then we move towards the destination
        if (Mathf.Abs((path[0].transform.position - satisfiedDistComparator.position).x) > 0.1f)
        {
            float dir = path[0].transform.position.x - transform.position.x;
            dir /= Mathf.Abs(dir);
            character.Move(new Vector2((int)dir, 0));
        }

        return true;
    }

    public void UpdatePathfindDestination(Vector2 p)
    {
        targetPosition = p;
    }

    public List<PathNode> GetCurrentPath()
    {
        return path;
    }

    public List<PathNode> GetAllPathfindNodes()
    {
        return GameController.pathNodes;
    }

    public bool IsPathfinding()
    {
        return isPathfinding;
    }

    public void PausePathfinding(bool paused)
    {
        isPaused = paused;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}