using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToWaypointBehavior : MonoBehaviour
{
    [SerializeField]
    private Marker targetMarker;
    [SerializeField]
    private float arriveThreshold;

    public void SetTargetMarker(Marker targetMarker)
    {
        this.targetMarker = targetMarker;
    }

    public Vector3 GetMarkerPosition()
    {
        AdvanceMarkerIfNeeded();
        return targetMarker.Position;
    }

    public Vector3 GetMarkerForward()
    {
        AdvanceMarkerIfNeeded();
        return targetMarker.Forward;
    }


    private void AdvanceMarkerIfNeeded()
    {
        if (Utilities.DestinationReached(transform.position, targetMarker.Position, arriveThreshold))
        {
            AdvanceMarker();
        }
    }

    private void AdvanceMarker()
    {
        targetMarker = targetMarker.GetNextAdjacentMarker();
    }

    private Road FindRoadUnderCar()
    {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, -transform.up, out hitInfo, 5f, LayerMask.GetMask("Road"));
        if (hit)
        {
            Road road = hitInfo.collider.GetComponent<Road>();
            return road;
        }
        return null;
    }

    public void LocalizeMarker(Vector3 carFrontPosition)
    {
        Road road = FindRoadUnderCar();
        if (road != null)
        {
            targetMarker = road.GetClosestMarker(carFrontPosition, transform.forward);
            if (targetMarker == null)
            {
                Debug.Log(name + " NOT FOUND MARKER " + carFrontPosition);
            }

            float dif = Vector3.Dot(carFrontPosition, targetMarker.Forward) - 
                Vector3.Dot(targetMarker.Position, targetMarker.Forward);
            
            if (dif > 0)
            {
                targetMarker = targetMarker.GetNextAdjacentMarker();
            }
        }
        else
        {
            Debug.Log(name + " UNDEFINED ROAD");
        }
    }
}
