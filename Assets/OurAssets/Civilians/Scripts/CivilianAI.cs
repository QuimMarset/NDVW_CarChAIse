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
    [SerializeField]
    private bool wasHelpingACarToContinue;

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
            SetAvoidanceDriving();

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
                    MoveToWaypoint();

                    if (policeAvoidanceBehavior.StopBeforeIntersection() || carCollisionBehavior.IsThereACarInFront() || 
                        trafficLightBehavior.IsThereARedLightInFront())
                    {
                        civilianController.StopCar();
                    }
                }
            }

            else if (policeAvoidanceBehavior.WaitingPositionReached(carFront))
            {
                civilianController.StopCar();
            }
        }

        else
        {
            SetNormalDriving();
            MoveToWaypoint();

            if (wasAvoidingPolice && moveBackwardsBehavior.BackwardMovementNeeded(targetPosition, carFront))
            {
                civilianController.SetBackwardMovement();
            }

            if (civilianController.IsMovingBackwards())
            {
                if (!moveBackwardsBehavior.BackwardMovementNeeded(targetPosition, carFront))
                {
                    civilianController.SetForwardMovement();
                }

                if (carCollisionBehavior.IsThereACarBehind())
                {
                    civilianController.StopCar();
                }

                if (wasHelpingACarToContinue && !carCollisionBehavior.IsThereACarInFront())
                {
                    wasHelpingACarToContinue = false;
                    civilianController.SetForwardMovement();
                }
            }
           
            if (civilianController.IsMovingForward())
            {
                if (trafficLightBehavior.IsThereARedLightInFront() || policeAvoidanceBehavior.StopBeforeIntersection())
                {
                    civilianController.StopCar();
                }

                if (carCollisionBehavior.IsThereACarInFront() && !carCollisionBehavior.IsThereACarInFrontMovingInTheOppositeDirection())
                {
                    civilianController.StopCar();
                }

                if (carCollisionBehavior.IsThereACarInFrontMovingBackwards())
                {
                    civilianController.SetBackwardMovement();
                    wasHelpingACarToContinue = true;
                }
            }

            if (carCollisionBehavior.IsThereACarInFront() && carCollisionBehavior.IsThereACarInFrontMovingInTheOppositeDirection())
            {
                Debug.Log(name + " deadlock");
                civilianController.SetBackwardMovement();
            }

            wasAvoidingPolice = false;
        }

    }

    private void MoveToWaypoint()
    {
        targetPosition = moveToWaypointBehavior.GetMarkerPosition();
        civilianController.ChangeDestination(targetPosition);
    }

    private void MoveToAvoidPolice()
    {
        civilianController.ChangeDestination(targetPosition);
        moveToWaypointBehavior.LocalizeMarker(carFront.position);
    }

    private void SetNormalDriving()
    {
        civilianController.SetNormalDriving();
        carCollisionBehavior.SetFOVMode();
    }

    private void SetAvoidanceDriving()
    {
        civilianController.SetForwardMovement();
        civilianController.SetAvoidanceDriving();
        carCollisionBehavior.SetRayMode();
    }

}
