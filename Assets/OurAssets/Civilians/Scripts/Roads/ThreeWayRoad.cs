using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeWayRoad : RoadWithWaitingPositions
{

    [SerializeField]
    private bool allowCivilians;

    private TrafficLightsManager trafficLightsManager;
    private CollidersManager collidersManager;

    private void Awake()
    {
        trafficLightsManager = GetComponent<TrafficLightsManager>();
        collidersManager = GetComponent<CollidersManager>();
        allowCivilians = true;
        trafficLightsManager.AllowToModifyColliders();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Police"))
        {
            BlockRoads();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Police"))
        {
            UnlockRoads();
        }
    }

    private void BlockRoads()
    {
        allowCivilians = false;
        trafficLightsManager.BlockColliderModification();
        collidersManager.DisableAllColliders();
        collidersManager.EnableAllColliders(WaitingReason.PoliceCar);
        
    }

    private void UnlockRoads()
    {
        allowCivilians = true;
        collidersManager.DisableAllColliders();
        trafficLightsManager.AllowToModifyColliders();
        trafficLightsManager.FixTrafficLights();
    }

    protected override bool GetReferencePosition(Vector3 forwardDirection, out Transform referenceTransform)
    {
        float angle = Vector3.Angle(forwardDirection, referencePoints[0].forward);
        bool goesSameDirection = angle <= 90;
        if (goesSameDirection)
        {
            referenceTransform = referencePoints[0];
            return true;
        }
        referenceTransform = null;
        return false;
    }

    public override bool ComputeWaitingPosition(Vector3 forwardDirection, Vector3 position, out Vector3 waitingPosition)
    {
        waitingPosition = Vector3.positiveInfinity;
        if (GetReferencePosition(forwardDirection, out Transform referenceTransform))
        {
            Vector3 rightComponent = Vector3.Scale(referenceTransform.position, Abs(referenceTransform.right));
            Vector3 forwardComponent = Vector3.Scale(position, Abs(referenceTransform.forward));
            Vector3 upComponent = Vector3.Scale(position, Abs(referenceTransform.up));
            waitingPosition = rightComponent + forwardComponent + upComponent;
            return IsInsideBounds(waitingPosition, referenceTransform);
        }
        return false;
    }

    public override bool IsInsideBounds(Vector3 position, Transform referenceTransform)
    {
        float forwardComp = Vector3.Dot(referenceTransform.forward, position);
        float highLimit = Vector3.Dot(referenceTransform.forward, referenceTransform.position);
        return forwardComp <= highLimit;
    }

}
