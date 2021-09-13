using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    public enum NodeType { raceNode, pitstopNode };

    [SerializeField] public NodeType nodeType;

    public float maxSpeed = 0;

    public float minDistanceToReachWaypoint;

    [SerializeField] public WaypointNode nextWaypointNode;

}
