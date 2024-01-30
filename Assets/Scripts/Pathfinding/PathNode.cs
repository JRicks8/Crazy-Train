using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System;

public enum MoveType {
    MOVE = 0,
    JUMP = 1,
}

[CustomPropertyDrawer(typeof(Connection))]
public class ConnectionDrawerUIE : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        var nodeField = new PropertyField(property.FindPropertyRelative("node"));
        var moveTypeField = new PropertyField(property.FindPropertyRelative("moveType"));

        container.Add(nodeField);
        container.Add(moveTypeField);

        return container;
    }
}

[Serializable]
public class Connection
{
    public PathNode node;
    public MoveType moveType;
}

public class PathNode : MonoBehaviour
{
    public List<Connection> connections = new List<Connection>();
    [HideInInspector]
    public PathNode parent;
    [HideInInspector]
    public float g; // total distance of the best path from the first starting node to this node
    [HideInInspector]
    public float h; // euclidean distance from the end node

    public void AddConnection(PathNode otherNode, MoveType moveType)
    {
        Connection connection = new Connection();
        connection.node = otherNode;
        connection.moveType = moveType;
        connections.Add(connection);
    }

    public void OnDrawGizmosSelected()
    {
        foreach (var conn in connections)
        {
            // Can leave if statement commented so that I can easily see where connections are invalid
            //if (conn.node != null)
            //{
                  Gizmos.DrawLine(transform.position, conn.node.transform.position);
            //}
        }
    }
}