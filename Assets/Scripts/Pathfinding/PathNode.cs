using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public List<PathNode> nodes = new List<PathNode>();

    public float g;
    public float h;
    public float f;

    public void OnDrawGizmosSelected()
    {
        foreach (var node in nodes) 
            Gizmos.DrawLine(transform.position, node.transform.position);
    }
}
