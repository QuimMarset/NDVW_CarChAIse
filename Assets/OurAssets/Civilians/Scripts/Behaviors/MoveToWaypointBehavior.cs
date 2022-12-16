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
        if (Utilities.DestinationReached(transform.position, targetMarker.Position, arriveThreshold))
        {
            AdvanceMarker();
        }
        return targetMarker.Position;
    }

    private void AdvanceMarker()
    {
        targetMarker = targetMarker.GetNextAdjacentMarker();
    }

    private Road FindRoadUnderCar()
    {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, -transform.up, out hitInfo, 2f, LayerMask.GetMask("Road"));
        if (hit)
        {
            Road road = hitInfo.collider.GetComponent<Road>();
            return road;
        }
        return null;
    }

    public void LocalizeMarker(Vector3 position)
    {
        Road road = FindRoadUnderCar();
        if (road != null)
        {
            targetMarker = road.GetClosestMarker(position, transform.forward);
            if (targetMarker == null)
            {
                Debug.Log(name + " NOT FOUND MARKER " + position);
            }

            float dif = Vector3.Dot(position, targetMarker.Forward) - Vector3.Dot(targetMarker.Position, targetMarker.Forward);
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
