using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundPathfind : MonoBehaviour
{
    public GameObject pathfindData;
    public List<PathNode> pathNodes = new List<PathNode>();

    public List<PathNode> path;

    public Vector2 targetPosition;

    public float recalculateCooldown = 0.5f;
    public bool continuouslyUpdate = true;
    public bool currentlyPathfinding = false;
    public bool pausePathfinding = false;

    private IEnumerator pathFindCoroutine;

    private void Start()
    {
        pathFindCoroutine = PathFindCoroutine();
        SearchForPathfindData();
        if (pathfindData == null) InvokeRepeating(nameof(SearchForPathfindData), 1.0f, 1.0f);
        else
        {
            pathNodes = pathfindData.GetComponentsInChildren<PathNode>().ToList();
        }
    }

    public void StartPathfinding(Vector2 p)
    {
        targetPosition = p;
        StartCoroutine(pathFindCoroutine);
    }

    public void EndPathfinding()
    {
        StopCoroutine(pathFindCoroutine);
    }

    IEnumerator PathFindCoroutine()
    {
        if (currentlyPathfinding) yield break;
        currentlyPathfinding = true;
        //Debug.Log("Starting Pathfind Coroutine");
        while (true)
        {
            Debug.Log("Pathfinding to: " + targetPosition);
            //float timeStart = Time.time;

            PathNode startNode = FindClosestNode(transform.position);
            PathNode endNode = FindClosestNode(targetPosition);
            List<PathNode> openList = new List<PathNode>();
            List<PathNode> closedList = new List<PathNode>();

            //Debug.Log("start position is at " + transform.position);
            //Debug.Log("end position is at " + targetPosition);
            //Debug.Log("start node is at position " + startNode.transform.position);
            //Debug.Log("end node is at position " + endNode.transform.position);

            if (startNode == null || endNode == null || pathNodes.Count <= 0)
            {
                Debug.LogError("Error: start node, end node, or pathnodes data is null. Aborting pathfinding.");
                currentlyPathfinding = false;
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

            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i].transform.position, path[i + 1].transform.position, Color.red, 0.5f);
            }

            //Debug.Log("Time Taken: " + (Time.time - timeStart));

            if (continuouslyUpdate)
                yield return new WaitForSeconds(recalculateCooldown);
            else
                break;

            while (pausePathfinding) yield return new WaitForSeconds(recalculateCooldown);
        }
        currentlyPathfinding = false;
    }

    /// <summary>
    /// Finds the closest node in the node data.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public PathNode FindClosestNode(Vector2 p)
    {
        if (pathNodes.Count == 0) return null;
        PathNode closest = pathNodes[0];
        float closestDistance = ((Vector2)closest.transform.position - p).magnitude;
        foreach (PathNode n in pathNodes)
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

    /// <summary>
    /// Finds the closest node in the node data that is at least d distance away.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    public PathNode FindClosestNode(Vector2 p, float d)
    {
        if (pathNodes.Count == 0) return null;
        PathNode closest = pathNodes[0];
        float closestDistance = ((Vector2)closest.transform.position - p).magnitude;
        foreach (PathNode n in pathNodes)
        {
            float dist = ((Vector2)n.transform.position - p).magnitude;
            if (dist < closestDistance && dist > d)
            {
                closest = n;
                closestDistance = dist;
            }
        }
        return closest;
    }

    public PathNode FindFurthestNode(Vector2 p)
    {
        if (pathNodes.Count == 0) return null;
        PathNode furthest = pathNodes[0];
        float furthestDistance = ((Vector2)furthest.transform.position - p).magnitude;
        foreach (PathNode n in pathNodes)
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

    private void SearchForPathfindData()
    {
        Debug.Log("Searching for PathfindData GameObject");
        pathfindData = GameObject.FindGameObjectWithTag("PathfindData");

        if (pathfindData != null)
        {
            Debug.Log("Found PathfindData GameObject");
            pathNodes = pathfindData.GetComponentsInChildren<PathNode>().ToList();
            CancelInvoke(nameof(SearchForPathfindData));
        }
        else Debug.Log("Pathfind Data not found. Searching again.");
    }
}