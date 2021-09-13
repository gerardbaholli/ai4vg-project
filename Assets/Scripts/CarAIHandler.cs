using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CarAIHandler : MonoBehaviour
{
    [SerializeField] float throttleLevel = 16;
    [SerializeField] bool isAvoidingCars = true;

    // Local variables
    Vector3 targetPosition = Vector3.zero;

    // Avoidance
    Vector2 avoidanceVectorLerped = Vector3.zero;

    // Waypoints
    WaypointNode currentWaypoint = null;
    WaypointNode previousWaypoint = null;
    WaypointNode[] allWayPoints;
    List<WaypointNode> raceNodes;
    List<WaypointNode> pitstopNodes;

    // Colliders
    PolygonCollider2D polygonCollider2D;

    // Components
    CarController carController;

    
    private void Awake()
    {
        carController = GetComponent<CarController>();
        allWayPoints = FindObjectsOfType<WaypointNode>();
        raceNodes = new List<WaypointNode>();
        pitstopNodes = new List<WaypointNode>();

        foreach (WaypointNode node in allWayPoints)
        {
            if (node.nodeType == WaypointNode.NodeType.raceNode)
            {
                raceNodes.Add(node);
            }
            else if (node.nodeType == WaypointNode.NodeType.pitstopNode)
            {
                pitstopNodes.Add(node);
            }
        }

        polygonCollider2D = GetComponentInChildren<PolygonCollider2D>();
    }

    public void SetCurrentWaypoint(WaypointNode value)
    {
        currentWaypoint = value;
    }

    public void FollowRaceWaypoints()
    {
        if (currentWaypoint == null)
        {
            currentWaypoint = FindClosestRaceWaypoint();
            previousWaypoint = currentWaypoint;
        }

        if (currentWaypoint != null)
        {
            ChangeWaypointIfBehindCar(-0.4f);

            targetPosition = currentWaypoint.transform.position;
            float distanceToWaypoint = (targetPosition - transform.position).magnitude;

            if (distanceToWaypoint > 3f)
            {
                Vector3 nearestPointOnTheWayPointLine = FindNearestPointOnLine(previousWaypoint.transform.position, currentWaypoint.transform.position, transform.position);
                float segments = distanceToWaypoint / 2f;

                targetPosition = (targetPosition + nearestPointOnTheWayPointLine * segments) / (segments + 1);
                Debug.DrawLine(transform.position, targetPosition, Color.white);
            }

            if (distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint)
            {
                if (currentWaypoint.maxSpeed > 0)
                    throttleLevel = currentWaypoint.maxSpeed;
                else throttleLevel = 1000;

                previousWaypoint = currentWaypoint;
                currentWaypoint = currentWaypoint.nextWaypointNode;
            }
        }

        ApplyMovement();
    }

    public void FollowPitlaneWaypoints()
    {

        if (currentWaypoint == null || (currentWaypoint.nextWaypointNode != null && currentWaypoint.nextWaypointNode.nodeType == WaypointNode.NodeType.raceNode))
        {
            currentWaypoint = FindClosestPitlaneWaypoint();
            previousWaypoint = currentWaypoint;
        }

        if (currentWaypoint != null)
        {
            ChangeWaypointIfBehindCar(-0.2f);

            targetPosition = currentWaypoint.transform.position;

            float distanceToWaypoint = (targetPosition - transform.position).magnitude;

            if (distanceToWaypoint > 0.1f)
            {
                Debug.Log("My previous is: " + previousWaypoint.name + " @@@ My target is: " + currentWaypoint.name);
                Vector3 nearestPointOnTheWayPointLine = FindNearestPointOnLine(previousWaypoint.transform.position, currentWaypoint.transform.position, transform.position);

                float segments = distanceToWaypoint / 2f;

                targetPosition = (targetPosition + nearestPointOnTheWayPointLine * segments) / (segments + 1);

                Debug.DrawLine(transform.position, targetPosition, Color.white);
            }

            if (distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint)
            {
                if (currentWaypoint.maxSpeed > 0)
                    throttleLevel = currentWaypoint.maxSpeed;
                else throttleLevel = 1000;

                previousWaypoint = currentWaypoint;

                if (currentWaypoint.nextWaypointNode == null)
                {
                    currentWaypoint = FindClosestRaceWaypoint();
                } else
                {
                    currentWaypoint = currentWaypoint.nextWaypointNode;
                }
            }


        }

        ApplyMovement();
    }

    private WaypointNode FindClosestRaceWaypoint()
    {
        float minDistance = (raceNodes[0].transform.position - gameObject.transform.position).magnitude;
        WaypointNode minDistanceNode = raceNodes[0];

        foreach (WaypointNode n in raceNodes)
        {
            if (minDistance > (n.transform.position - gameObject.transform.position).magnitude)
            {
                minDistance = (n.transform.position - gameObject.transform.position).magnitude;
                minDistanceNode = n;
            }
        }

        return minDistanceNode;
    }

    private WaypointNode FindClosestPitlaneWaypoint()
    {
        return pitstopNodes
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();
    }

    private void ApplyMovement()
    {
        Vector2 inputVector = Vector2.zero;

        inputVector.x = TurnTowardTarget();
        inputVector.y = ApplyThrottleOrBrake(inputVector.x);

        //Debug.Log("CURRENT WAYPOINT: " + currentWaypoint.name);

        carController.SetInputVector(inputVector);
    }

    private float TurnTowardTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        vectorToTarget.Normalize();

        // Apply avoidance to steering
        if (isAvoidingCars)
            AvoidCars(vectorToTarget, out vectorToTarget);

        // Calculate an angle towards the target 
        float angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

        // We want the car to turn as much as possible if the angle is greater than 45 degrees and
        // we wan't it to smooth out so if the angle is small we want the AI to make smaller corrections. 
        float steerAmount = angleToTarget / 45.0f;

        // Clamp steering to between -1 and 1.
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        return steerAmount;
    }

    private float ApplyThrottleOrBrake(float inputX)
    {
        // If we are going too fast then do not accelerate further. 
        if (carController.GetVelocityMagnitude() > throttleLevel)
            return 0;

        // Apply throttle forward based on how much the car wants to turn.
        // If it's a sharp turn this will cause the car to apply less speed forward.
        return 1.05f - Mathf.Abs(inputX) / 1.0f;
    }

    private Vector2 FindNearestPointOnLine(Vector2 lineStartPosition, Vector2 lineEndPosition, Vector2 point)
    {
        //Get heading as a vector
        Vector2 lineHeadingVector = (lineEndPosition - lineStartPosition);

        //Store the max distance
        float maxDistance = lineHeadingVector.magnitude;
        lineHeadingVector.Normalize();

        //Do projection from the start position to the point
        Vector2 lineVectorStartToPoint = point - lineStartPosition;
        float dotProduct = Vector2.Dot(lineVectorStartToPoint, lineHeadingVector);

        //Clamp the dot product to maxDistance
        dotProduct = Mathf.Clamp(dotProduct, 0f, maxDistance);

        return lineStartPosition + lineHeadingVector * dotProduct;
    }

    private bool IsCarsInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
    {
        // Disable the cars own collider to avoid having the AI car detect itself. 
        polygonCollider2D.enabled = false;

        // Perform the circle cast in front of the car with a slight offset forward and only in the Car layer
        RaycastHit2D raycastHit2d = Physics2D.CircleCast(transform.position + transform.up * 0.5f, 0.5f, transform.up, 2, 1 << LayerMask.NameToLayer("Car"));

        // Enable the colliders again so the car can collide and other cars can detect it.  
        polygonCollider2D.enabled = true;

        if (raycastHit2d.collider != null)
        {
            // Draw a red line showing how long the detection is, make it red since we have detected another car
            Debug.DrawRay(transform.position, transform.up * 1, Color.red);

            position = raycastHit2d.collider.transform.position;
            otherCarRightVector = raycastHit2d.collider.transform.right;
            return true;
        }
        else
        {
            // We didn't detect any other car so draw black line with the distance that we use to check for other cars. 
            Debug.DrawRay(transform.position, transform.up * 1, Color.black);
        }

        // No car was detected but we still need assign out values so lets just return zero. 
        position = Vector3.zero;
        otherCarRightVector = Vector3.zero;

        return false;
    }

    private void AvoidCars(Vector2 vectorToTarget, out Vector2 newVectorToTarget)
    {
        if (IsCarsInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
        {
            Vector2 avoidanceVector = Vector2.zero;

            // Calculate the reflecing vector if we would hit the other car. 
            avoidanceVector = Vector2.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);

            float distanceToTarget = (targetPosition - transform.position).magnitude;

            // We want to be able to control how much desire the AI has to drive towards the waypoint vs avoiding the other cars. 
            // As we get closer to the waypoint the desire to reach the waypoint increases.
            float driveToTargetInfluence = 2.0f / distanceToTarget;

            // Ensure that we limit the value to between 30% and 100% as we always want the AI to desire to reach the waypoint.  
            driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.30f, 1.0f);

            // The desire to avoid the car is simply the inverse to reach the waypoint
            float avoidanceInfluence = 1.0f - driveToTargetInfluence;

            // Reduce jittering a little bit by using a lerp
            avoidanceVectorLerped = Vector2.Lerp(avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * 4);

            // Calculate a new vector to the target based on the avoidance vector and the desire to reach the waypoint
            newVectorToTarget = (vectorToTarget * driveToTargetInfluence + avoidanceVector * avoidanceInfluence);
            newVectorToTarget.Normalize();

            // Draw the vector which indicates the avoidance vector in green
            Debug.DrawRay(transform.position, avoidanceVector * 1, Color.green);

            // Draw the vector that the car will actually take in yellow. 
            Debug.DrawRay(transform.position, newVectorToTarget * 1, Color.yellow);

            // We are done so we can return now. 
            return;
        }

        // We need assign a default value if we didn't hit any cars before we exit the function. 
        newVectorToTarget = vectorToTarget;
    }

    private void ChangeWaypointIfBehindCar(float value)
    {
        //Debug.Log("Waypoint changed");
        //Debug.Log("currentWaypoint: " + currentWaypoint.name + " " +  currentWaypoint.transform.position + " @@@ gameObject: " + gameObject.transform.up);
        //Debug.Log("DOT: " + Vector3.Dot((currentWaypoint.transform.position - gameObject.transform.position).normalized, gameObject.transform.up.normalized));
        if (Vector3.Dot((currentWaypoint.transform.position - gameObject.transform.position).normalized, gameObject.transform.up.normalized) < value)
        {
            currentWaypoint = currentWaypoint.nextWaypointNode;
        }
    }

    public void StopCar()
    {
        Vector2 inputVector = Vector2.zero;

        carController.SetInputVector(inputVector);
    }

    public void SetThrottleLevel(float value)
    {
        throttleLevel = value;
    }

    private void PrintAllRaceNodeDistances()
    {
        foreach (WaypointNode n in raceNodes)
        {
            Debug.Log("Node: " + n.name + " # Distance: " +
                (n.transform.position - gameObject.transform.position).magnitude);
        }
    }

}