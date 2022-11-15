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

    private Vector3 PostionToFollow = Vector3.zero;
    private int currentWayPoint;
    [SerializeField] private float AIFOV = 60;
    private int NavMeshLayerBite;
    private List<Vector3> waypoints = new List<Vector3>();
    private int Fails;
    private Vector3 direction;
    private float nextActionTime = 0.0f;
    private float period = 0.1f;



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

            PostionToFollow = waypoints[currentWayPoint];
            if (Vector3.Distance(CarFront.position, PostionToFollow) < 2)
                currentWayPoint++;

            // Debug.Log(Vector3.Distance(carFront.position, CustomDestination.position));
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
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        Vector3 sourcePostion;
        if (Time.time > nextActionTime ) {
            nextActionTime += period;
            waypoints = new List<Vector3>();
            currentWayPoint = 0;
        }

        if (waypoints.Count == 0)
        {
            sourcePostion = CarFront.position;
            if(direction == null) direction = CarFront.forward;
            // Calculate(destination.position, sourcePostion, CarFront.forward, NavMeshLayerBite);
            Calculate(destination.position, sourcePostion, direction, NavMeshLayerBite);
        }
        else
        {
            sourcePostion = waypoints[waypoints.Count - 1];
            Vector3 direction = (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
            Calculate(destination.position, sourcePostion, direction, NavMeshLayerBite);
        }

        void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int NavMeshAreaBite)
        {
            Debug.Log(NavMeshAreaBite);
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
        Vector3 relativeVector = transform.InverseTransformPoint(PostionToFollow);
        float SteeringAngle = (relativeVector.x / relativeVector.magnitude) * MaxSteeringAngle;
        return SteeringAngle;
    }


    protected override float GetMovementDirection()
	{
        return 1.0f;
    }

    void debug(string text, bool IsCritical)
    {
        if (IsCritical)
            Debug.LogError(text);
        else
            Debug.Log(text);
    }
}
