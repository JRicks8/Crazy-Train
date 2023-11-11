using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundPathfind : MonoBehaviour
{
    public GameObject pathfindData;
    List<PathNode> pathNodes = new List<PathNode>();

    private void Start()
    {
        SearchForPathfindData();
        if (pathfindData == null) InvokeRepeating(nameof(SearchForPathfindData), 1.0f, 1.0f);
        else pathNodes = pathfindData.GetComponentsInChildren<PathNode>().ToList();
    }

    public void FindPath(Vector2 start, Vector2 end)
    {

    }

    private PathNode FindClosestNode(Vector2 p)
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

    private PathNode FindFurthestNode(Vector2 p)
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
        pathfindData = GameObject.FindGameObjectWithTag("PathfindData");

        if (pathfindData != null)
        {
            pathNodes = pathfindData.GetComponentsInChildren<PathNode>().ToList();
            CancelInvoke(nameof(SearchForPathfindData));
        }
        else Debug.Log("Pathfind Data not found. Searching for GameObject...");
    }
}