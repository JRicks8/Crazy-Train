using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorScripts : MonoBehaviour
{
    private static GameObject originNodeObject;

    [MenuItem("Node Connector/Set Origin Connection &k")]
    private static void SetOriginConnection()
    {
        originNodeObject = Selection.activeGameObject;
        if (originNodeObject != null && originNodeObject.GetComponent<PathNode>() != null)
        {
            Debug.Log("Success");
        }
        else
        {
            Debug.Log("Failed");
        }
    }

    [MenuItem("Node Connector/Set Destination Connections &l")]
    private static void SetDestinationConnection()
    {
        PathNode originNode = originNodeObject.GetComponent<PathNode>();

        GameObject[] destinationNodeObjects = Selection.gameObjects;

        for (int i = 0; i < destinationNodeObjects.Length; i++)
        {
            if (destinationNodeObjects[i].TryGetComponent(out PathNode p) && originNodeObject != destinationNodeObjects[i])
            {
                MoveType moveType;
                if (destinationNodeObjects[i].transform.position.y > originNodeObject.transform.position.y)
                    moveType = MoveType.JUMP;
                else
                    moveType = MoveType.MOVE;
                originNode.connections.Add(new Connection()
                {
                    node = p,
                    moveType = moveType
                });
            }
        }

        Debug.Log("Added " + destinationNodeObjects.Length + " new connections");
    }
}
