using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourWayRoad : Road
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

    public override bool ComputeWaitingPosition(Vector3 forwardDirection, Vector3 position, out Vector3 waitingPosition)
    {
        waitingPosition = Vector3.positiveInfinity;
        return false;
    }

    public override bool IsInsideBounds(Vector3 position, Transform referenceTransform)
    {
        return false;
    }
}
