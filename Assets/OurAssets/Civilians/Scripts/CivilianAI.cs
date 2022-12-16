using UnityEngine;


[RequireComponent(typeof(CivilianController))]
[RequireComponent(typeof(MoveToWaypointBehavior))]
[RequireComponent(typeof(CarCollisionBehavior))]
[RequireComponent(typeof(TrafficLightBehavior))]
[RequireComponent(typeof(PoliceAvoidanceBehavior))]
[RequireComponent(typeof(MoveBackwardsBehavior))]
public class CivilianAI : MonoBehaviour
{
    private CivilianController civilianController;
    private MoveToWaypointBehavior moveToWaypointBehavior;
    private CarCollisionBehavior carCollisionBehavior;
    private TrafficLightBehavior trafficLightBehavior;
    private PoliceAvoidanceBehavior policeAvoidanceBehavior;
    private MoveBackwardsBehavior moveBackwardsBehavior;

    [SerializeField]
    private Transform carFront;

    [SerializeField]
    private Vector3 targetPosition;

    [SerializeField]
    private bool wasAvoidingPolice;

    private void Awake()
    {
        civilianController = GetComponent<CivilianController>();
        moveToWaypointBehavior = GetComponent<MoveToWaypointBehavior>();
        carCollisionBehavior = GetComponent<CarCollisionBehavior>();
        trafficLightBehavior = GetComponent<TrafficLightBehavior>();
        policeAvoidanceBehavior = GetComponent<PoliceAvoidanceBehavior>();
        moveBackwardsBehavior = GetComponent<MoveBackwardsBehavior>();
    }

    private void Update()
    {
        
        if (policeAvoidanceBehavior.IsPoliceDetected())
        {
            civilianController.SetForwardMovement();

            if (!policeAvoidanceBehavior.IsWaitingPositionComputed())
            {
                bool isValid = policeAvoidanceBehavior.ComputePositionToWait(carFront, out targetPosition);
                if (isValid)
                {
                    MoveToAvoidPolice();
                    wasAvoidingPolice = true;
                }
                else
                {
                    if (carCollisionBehavior.IsThereACarInFront() || trafficLightBehavior.IsThereARedLightInFront())
                    {
                        civilianController.StopCar();
                    }

                    else
                    {
                        MoveToWaypoint();
                    }
                }
            }

            else if (policeAvoidanceBehavior.WaitingPositionReached(carFront))
            {
                civilianController.StopCar();
            }
        }

        else if (policeAvoidanceBehavior.StopBeforeIntersection())
        {
            civilianController.StopCar();
        }

        else
        {
            MoveToWaypoint();

            if (wasAvoidingPolice && moveBackwardsBehavior.BackwardMovementNeeded(targetPosition, carFront))
            {
                civilianController.SetBackwardMovement();
            }

            if (civilianController.IsMovingBackwards() && !moveBackwardsBehavior.BackwardMovementNeeded(targetPosition, carFront))
            {
                civilianController.SetForwardMovement();
            }

            if (carCollisionBehavior.IsThereACarInFront() || trafficLightBehavior.IsThereARedLightInFront())
            {
                civilianController.StopCar();
            }

            wasAvoidingPolice = false;
        }

    }

    private void MoveToWaypoint()
    {
        civilianController.SetNormalDriving();
        targetPosition = moveToWaypointBehavior.GetMarkerPosition();
        civilianController.ChangeDestination(targetPosition);
    }

    private void MoveToAvoidPolice()
    {
        civilianController.SetAvoidanceDriving();
        civilianController.ChangeDestination(targetPosition);
        moveToWaypointBehavior.LocalizeMarker(carFront.position);
    }

}
