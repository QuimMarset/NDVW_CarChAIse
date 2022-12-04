using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Police : CarController
{
    [Header("General Parameters")]// Look at the documentation for a detailed explanation 
    public List<string> NavMeshLayers;
    public int MaxRPM = 150;

    [Header("Destination Parameters")]// Look at the documentation for a detailed explanation
    public bool Patrol = false;
    public Transform TargetObject;

    [HideInInspector] public bool move;// Look at the documentation for a detailed explanation

    private Vector3 PositionToFollow = Vector3.zero;
    private int currentWayPoint;
    [SerializeField] private float AIFOV = 60;
    private int NavMeshLayerBite;
    private List<Vector3> waypoints = new List<Vector3>();
    private int Fails;
    private Vector3 direction;
    private float actionTime = 0.0f;
    [SerializeField] private float defaultReactionTime = 0.5f;
    private float patrolCheckTime = -Mathf.Infinity;

    private bool Debugger = false;
    private bool ShowGizmos = true;

    private float MaxSpeedAtCurve = 30;

    private float waypointDistanceThreshold = 20;

    private float backwardStartTime = Mathf.Infinity;
    private float stopCheckStartTime = Mathf.Infinity;
    private float stopCheckTime = 0.5f;
    private float backwardTime = 1f;
    private float reactionTime = 0f;
    private Vector3 lastCarPosition = Vector3.zero;


	// Start is called before the first frame update
	protected override void Start()
    {
        base.Start();

        // Initialization of path settings
        currentWayPoint = 0;
        move = true;
        TargetObject = FindObjectOfType<Player>().transform;
        CalculateNavMashLayerBite();
        // Add itself to the GameManager police list
        FindObjectOfType<GameManager>()?.PoliceObjects.Add(this);
    }

    protected override void FixedUpdate()
	{
        actionTime += Time.deltaTime;
        base.FixedUpdate();
        PathProgress();
        lastCarPosition = CarFront.position;
    }

    private void CalculateNavMashLayerBite()
    {
        if (NavMeshLayers == null || NavMeshLayers[0] == "AllAreas")
            NavMeshLayerBite = UnityEngine.AI.NavMesh.AllAreas;
        else if (NavMeshLayers.Count == 1)
            NavMeshLayerBite += 1 << UnityEngine.AI.NavMesh.GetAreaFromName(NavMeshLayers[0]);
        else
        {
            foreach (string Layer in NavMeshLayers)
            {
                int I = 1 << UnityEngine.AI.NavMesh.GetAreaFromName(Layer);
                NavMeshLayerBite += I;
            }
        }
    }

    private void PathProgress() //Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
    {
        wayPointManager();
        Move();
        void wayPointManager()
        {
            CreatePath();
            if (waypoints.Count > 0 && currentWayPoint < waypoints.Count) {
                // Debug.Log("Waypoints Count: " + waypoints.Count + ", currentWayPoint: " + currentWayPoint);
                PositionToFollow = waypoints[currentWayPoint];
                if (Vector3.Distance(CarFront.position, PositionToFollow) < 1) currentWayPoint++;
            }
            else PositionToFollow = Vector3.zero;

            // if (waypoints.Count > 2) reactionTime += Time.deltaTime;
            
        }

        void CreatePath()
        {
            if (Patrol == true){
                // CreateRandomPath();
            }
            else
            {
                patrolCheckTime = -Mathf.Infinity;
                CreateTargetPath(TargetObject);   
            }
        }
    }


    public void CreateTargetPath(Transform destination) //Creates a path to the Custom destination
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        Vector3 sourcePosition;
        currentWayPoint = 1;
    
        sourcePosition = CarFront.position;
        if(direction == null) direction = CarFront.forward;
        List<Vector3> currentWaypointList = Calculate(destination.position, sourcePosition, direction, NavMeshLayerBite);
        
        if (actionTime > reactionTime || (actionTime > defaultReactionTime && currentWaypointList.Count == 2)){
            waypoints = new List<Vector3>(currentWaypointList);
            actionTime = 0.0f;
            if (currentWaypointList.Count != 2) reactionTime += 0.25f;
            else reactionTime = defaultReactionTime;
        }

        // List<Vector3> waypoints = new List<Vector3>();
        List<Vector3> Calculate(Vector3 destination, Vector3 sourcePosition, Vector3 direction, int NavMeshAreaBite)
        {
            List<Vector3> waypointsList = new List<Vector3>();
            if (UnityEngine.AI.NavMesh.SamplePosition(destination, out UnityEngine.AI.NavMeshHit hit, 1000000, NavMeshAreaBite) &&
                UnityEngine.AI.NavMesh.CalculatePath(sourcePosition, hit.position, NavMeshAreaBite, path))
            {
                
                if (path.corners.ToList().Count() > 1 && CheckForAngle(path.corners[1], sourcePosition, direction))
                {
                    waypointsList.AddRange(path.corners.ToList());
                    debug("Custom Path generated successfully", false);
                }
                else
                {
                    if (path.corners.Length > 2 && CheckForAngle(path.corners[2], sourcePosition, direction))
                    {
                        waypointsList.AddRange(path.corners.ToList());
                        debug("Custom Path generated successfully", false);
                    }
                    else
                    {
                        debug("Failed to generate a Custom path. Waypoints are outside the AIFOV. Generating a new one", false);
                        Fails++;
                    }
                }
            }
            else
            {
                debug("Failed to generate a Custom path. Invalid Path. Generating a new one", false);
                Fails++;
            }

            return waypointsList;
        }
    }

    // public void CreateRandomPath() // Creates a path to a random destination
    // {   
    //     // In every 2 seconds
    //     if (patrolCheckTime == -Mathf.Infinity || Time.time - patrolCheckTime > 1 ) {
    //         if (patrolCheckTime == -Mathf.Infinity || waypoints.Count > 5) {
    //             waypoints.Clear();
    //             currentWayPoint = 1;
    //         } 
    //         patrolCheckTime = Time.time;

    //         NavMeshPath path = new NavMeshPath();
    //         Vector3 sourcePosition;


    //         if (waypoints.Count == 0)
    //         {
    //             Vector3 randomDirection = Random.insideUnitSphere * 150;
    //             randomDirection += transform.position;
    //             sourcePosition = CarFront.position;
    //             Calculate(randomDirection, sourcePosition, direction, NavMeshLayerBite);
    //         }
    //         else
    //         {
    //             sourcePosition = waypoints[waypoints.Count - 1];
    //             Vector3 randomPosition = Random.insideUnitSphere * 100;
    //             randomPosition += sourcePosition;
    //             Calculate(randomPosition, sourcePosition, direction, NavMeshLayerBite);
    //         }

    //         void Calculate(Vector3 destination, Vector3 sourcePosition, Vector3 direction, int NavMeshAreaByte)
    //         {
    //             if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 1000000, NavMeshAreaByte) &&
    //                 NavMesh.CalculatePath(sourcePosition, hit.position, NavMeshAreaByte, path) && path.corners.Length > 2)
    //             {
    //                 if (CheckForAngle(path.corners[1], sourcePosition, direction))
    //                 {
    //                     waypoints.AddRange(path.corners.ToList());
    //                     debug("Random Path generated successfully", false);
    //                 }
    //                 else
    //                 {
    //                     if (CheckForAngle(path.corners[2], sourcePosition, direction))
    //                     {
    //                         waypoints.AddRange(path.corners.ToList());
    //                         debug("Random Path generated successfully", false);
    //                     }
    //                     else
    //                     {
    //                         debug("Failed to generate a random path. Waypoints are outside the AIFOV. Generating a new one", false);
    //                         Fails++;
    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 debug("Failed to generate a random path. Invalid Path. Generating a new one", false);
    //                 Fails++;
    //             }
    //         }
    //     }
    // }

    private bool CheckForAngle(Vector3 pos, Vector3 source, Vector3 direction) //calculates the angle between the car and the waypoint 
    {
        Vector3 distance = (pos - source).normalized;
        float CosAngle = Vector3.Dot(distance, direction);
        float Angle = Mathf.Acos(CosAngle) * Mathf.Rad2Deg;

        if (Angle < AIFOV)
            return true;
        else
            return false;
    }

    protected override float GetSteeringAngle()
	{
        Vector3 relativeVector = transform.InverseTransformPoint(PositionToFollow);
        float SteeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;

        if(backwardStartTime!= Mathf.Infinity & Time.time < backwardStartTime + backwardTime) SteeringAngle = -SteeringAngle;
        return SteeringAngle;
    }


    protected override float GetMovementDirection()
	{
        float movementDirection = 0;

        // Check if go backwards for avoid blocking
        if (stopCheckStartTime != Mathf.Infinity)
        {
            // If too many time stopped (ergo blocked) and no backward time started
            if (Time.time >= (stopCheckStartTime + stopCheckTime) && backwardStartTime == Mathf.Infinity)
                backwardStartTime = Time.time;

            // If backward time started
            if (backwardStartTime != Mathf.Infinity){
                if(Time.time < (backwardStartTime + backwardTime))
                    movementDirection = -1.0f;
                else {
                    stopCheckStartTime = Mathf.Infinity;
                    backwardStartTime = Mathf.Infinity;
                }
            }
        } 
        else {
            // If car don't move and target is far, start the stop timer (consider the car blocked)
            if (Mathf.Abs(CurrentForwardSpeed) < 0.1f && Vector3.Distance(CarFront.position, TargetObject.position) > 5)
                stopCheckStartTime = Time.time;
            else
                stopCheckStartTime = Mathf.Infinity;

            // Move to waypoints
            if (waypoints.Count == 2) movementDirection = 1.0f;
            else if (waypoints.Count > 2) 
            {
                float distanceToWaypoint = Vector3.Distance(CarFront.position, waypoints[currentWayPoint]);
                
                if (distanceToWaypoint > waypointDistanceThreshold) movementDirection = 1.0f;
                else 
                {
                    if (CurrentForwardSpeed > MaxSpeedAtCurve) movementDirection = -(waypointDistanceThreshold/distanceToWaypoint) * CurrentForwardSpeed / MaxSpeedAtCurve;
                    else if (CurrentForwardSpeed == MaxSpeedAtCurve) movementDirection = 0;
                    else movementDirection = 1.0f;
                }
                
            }
            else movementDirection = 1.0f;
        }
        return movementDirection;
    }

    void debug(string text, bool IsCritical)
    {
        if (Debugger){
            if (IsCritical)
                Debug.LogError(text);
            else
                Debug.Log(text);
        }
    }

    private void OnDrawGizmos() // shows a Gizmos representing the waypoints and AI FOV
    {
        if (ShowGizmos == true)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (i == currentWayPoint)
                    Gizmos.color = Color.blue;
                else
                {
                    if (i > currentWayPoint)
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.green;
                }
                Gizmos.DrawWireSphere(waypoints[i], 2f);
            }
            CalculateFOV();
        }

        void CalculateFOV()
        {
            Gizmos.color = Color.white;
            float totalFOV = AIFOV * 2;
            float rayRange = 10.0f;
            float halfFOV = totalFOV / 2.0f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            Gizmos.DrawRay(CarFront.position, leftRayDirection * rayRange);
            Gizmos.DrawRay(CarFront.position, rightRayDirection * rayRange);
        }
    }

}
