using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.VisualScripting;

public class AirPathfind : MonoBehaviour
{
    public Seeker seeker;

    private List<Vector3> path = new List<Vector3>();

    private bool shouldStopPathfinding = false;
    private bool isPathfinding = false;

    private float pathfindCooldown = 0.2f;

    private Vector3 startPosition;
    private Vector3 endPosition;

    public void StartPathfinding()
    {
        if (isPathfinding) return;
        isPathfinding = true;
        InvokeRepeating(nameof(Pathfind), 0.0f, pathfindCooldown);
    }

    public void StopPathfinding()
    {
        shouldStopPathfinding = true;
    }

    private void OnPathCalculated(Path path)
    {
        if (path.error)
        {
            Debug.LogError(path.errorLog);
            return;
        }

        this.path = path.vectorPath;
    }

    private void Pathfind()
    {
        if (shouldStopPathfinding)
        {
            CancelInvoke(nameof(Pathfind));
            shouldStopPathfinding = false;
            isPathfinding = false;
            return;
        }

        seeker.StartPath(startPosition, endPosition, OnPathCalculated, GraphMask.everything);
    }

    // Moves along the path. Return value is the success of movement.
    public bool MoveAlongPath(Character character)
    {
        if (path == null)
        {
            Debug.LogError("Attempt to move along path has failed: Path is null!");
            return false;
        }
        if (path.Count <= 2) return true;

        Vector2 dir = (path[1] - character.middle.position).normalized;
        character.Move(dir);
        return true;
    }

    // Getters and Setters
    public void SetStartPosition(Vector3 startPosition)
    {
        this.startPosition = startPosition;
    }

    public void SetEndPosition(Vector3 endPosition)
    {
        this.endPosition = endPosition;
    }

    public void SetPathfindCooldown(float pathfindCooldown)
    {
        this.pathfindCooldown = pathfindCooldown;
    }

    public List<Vector3> GetPath()
    {
        return path;
    }

    public bool GetIsPathfinding()
    {
        return isPathfinding;
    }
}
