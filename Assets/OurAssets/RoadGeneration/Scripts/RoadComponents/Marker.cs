using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class Marker : MonoBehaviour, IEquatable<Marker>
{
    public List<Marker> adjacentMarkers;
    
    private Road parentRoad;

    private void Awake()
    {
        parentRoad = parentRoad = transform.parent.parent.GetComponent<Road>();
    }

    public Vector3 Position
    {
        get => transform.position;
    }

    public Vector3 Forward
    {
        get => transform.forward;
    }

    public Road ParentRoad
    {
        get => parentRoad;
    }

    public void ConnectMarker(Marker nextMarker)
    {
        adjacentMarkers.Add(nextMarker);
    }

    private bool GoesInSameDirection(Marker marker)
    {
        return Vector3.Angle(this.Forward, marker.Forward) < 45;
    }

    private bool MovesForward(Marker marker)
    {
        Vector3 difference = Vector3.Normalize(marker.Position - Position);
        return Vector3.Angle(Forward, difference) < 45;
    }

    private Marker SearchMarkerToConnectWith(Road road)
    {
        Marker closestMarker = null;
        float minDistance = float.MaxValue;
        foreach (Marker marker in road.GetMarkersReceivingConnections())
        {
            // We look for the closest marker going in the same direction (same side of the road) and moving forward
            float distance = Vector3.Distance(Position, marker.Position);
            if (GoesInSameDirection(marker) && MovesForward(marker) && distance < minDistance)
            {
                minDistance = distance;
                closestMarker = marker;
            }
        }
        return closestMarker;
    }

    public Marker SearchMarkerToConnectWith(List<Road> roads)
    {
        Marker closestMarker = null;
        float minDistance = float.MaxValue;
        foreach (Road road in roads)
        {
            if (parentRoad.IsTheSame(road))
            {
                continue;
            }

            Marker candidateClosestMarker = SearchMarkerToConnectWith(road);
            if (candidateClosestMarker == null)
            {
                continue;
            }

            float distance = Vector3.Distance(Position, candidateClosestMarker.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestMarker = candidateClosestMarker;
            }
        }
        return closestMarker;
    }

    public Marker GetNextAdjacentMarker()
    {
        int index = UnityEngine.Random.Range(0, adjacentMarkers.Count);
        return adjacentMarkers[index];
    }

    public bool Equals(Marker other)
    {
        return Vector3.SqrMagnitude(this.Position - other.Position) < 0.001f;
    }

    private void OnDrawGizmos()
    {
        if (Selection.activeObject == gameObject)
        {
            Gizmos.color = Color.red;
            if (adjacentMarkers != null && adjacentMarkers.Count > 0)
            {
                foreach (var marker in adjacentMarkers)
                {
                    Gizmos.DrawLine(this.Position, marker.Position);
                }
            }
            Gizmos.color = Color.white;
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(Position, 1f);
    }
}

