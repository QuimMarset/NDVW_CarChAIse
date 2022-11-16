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
    public Transform CustomDestination;

    [HideInInspector] public bool move;// Look at the documentation for a detailed explanation

    private Vector3 PositionToFollow = Vector3.zero;
    private int currentWayPoint;
    [SerializeField] private float AIFOV = 60;
    private int NavMeshLayerBite;
    private List<Vector3> waypoints = new List<Vector3>();
    private int Fails;
    private Vector3 direction;
    private float nextActionTime = 0.0f;
    private float period = 0.1f;
    private bool Debugger = false;
    private bool ShowGizmos = true;

    private float waypointDistanceThreshold = 20;

    private float backwardStartTime = Mathf.Infinity;
    private float stopCheckStartTime = Mathf.Infinity;
    private float stopCheckTime = 0.1f;
    private float backwardTime = 1f;

    private Vector3 lastCarPosition = Vector3.zero;



    // private Vector3 direction;
    // private float nextActionTime = 0.0f;
    // private float period = 0.1f;

    void Awake()
    {
        currentWayPoint = 0;
        move = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        CalculateNavMashLayerBite();
    }

    protected override void FixedUpdate()
	{
        base.FixedUpdate();
        PathProgress_v1();
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

    private void PathProgress_v1() //Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
    {
        wayPointManager();
        Move();

        void wayPointManager()
        {
            CreatePath();
            if (waypoints.Count > 0) PositionToFollow = waypoints[currentWayPoint];
            else PositionToFollow = Vector3.zero;
        }

        void CreatePath()
        {
            if (CustomDestination == null)
            {
                if (Patrol == true)
                    // RandomPath();
                    CustomPath_v1(CustomDestination);
                else
                {
                    debug("No custom destination assigned and Patrol is set to false", false);
                }
            }
            else
               CustomPath_v1(CustomDestination);
            
        }
    }


    public void CustomPath_v1(Transform destination) //Creates a path to the Custom destination
    {
        if (Time.time > nextActionTime ) {
            UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
            Vector3 sourcePostion;

            nextActionTime += period;
            waypoints.Clear();
            currentWayPoint = 1;
        
            sourcePostion = CarFront.position;
            if(direction == null) direction = CarFront.forward;
            // Calculate(destination.position, sourcePostion, CarFront.forward, NavMeshLayerBite);
            Calculate(destination.position, sourcePostion, direction, NavMeshLayerBite);
            void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int NavMeshAreaBite)
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(destination, out UnityEngine.AI.NavMeshHit hit, 1000000, NavMeshAreaBite) &&
                    UnityEngine.AI.NavMesh.CalculatePath(sourcePostion, hit.position, NavMeshAreaBite, path))
                {
                    if (path.corners.ToList().Count() > 1 && CheckForAngle(path.corners[1], sourcePostion, direction))
                    {
                        waypoints.AddRange(path.corners.ToList());
                        debug("Custom Path generated successfully", false);
                    }
                    else
                    {
                        if (path.corners.Length > 2 && CheckForAngle(path.corners[2], sourcePostion, direction))
                        {
                            waypoints.AddRange(path.corners.ToList());
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
            }
        }
    }

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
        if (stopCheckStartTime != Mathf.Infinity){
            // Debug.Log("It was stopped");
            // Debug.Log("current: " + Time.time + ", stopTime: " + stopCheckStartTime);
            if (Time.time >= stopCheckStartTime + stopCheckTime & backwardStartTime == Mathf.Infinity) {backwardStartTime = Time.time;}

            if (backwardStartTime != Mathf.Infinity){
                if(Time.time < backwardStartTime + backwardTime) movementDirection = -1.0f;
                else {
                    stopCheckStartTime = Mathf.Infinity;
                    backwardStartTime = Mathf.Infinity;
                }
            }
        } else {
            if (Vector3.Distance(CarFront.position, CustomDestination.position) > 5 & Vector3.Distance(lastCarPosition, CarFront.position) < 0.001f) stopCheckStartTime = Time.time;

            if(waypoints.Count == 2) movementDirection = 1.0f;
            else if (waypoints.Count > 2) {
                float distanceToWaypoint = Vector3.Distance(CarFront.position, waypoints[currentWayPoint]);
                
                if (distanceToWaypoint > waypointDistanceThreshold) movementDirection = 1.0f;
                else {
                    if (CurrentWheelsSpeed > MaxSpeedAtCurve) movementDirection = -(waypointDistanceThreshold/distanceToWaypoint) * CurrentWheelsSpeed / MaxSpeedAtCurve;
                    else if (CurrentWheelsSpeed == MaxSpeedAtCurve) movementDirection = 0;
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
