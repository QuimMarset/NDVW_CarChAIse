using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class Marker : MonoBehaviour, IEquatable<Marker>
{
    public List<Marker> adjacentMarkers;

    public Vector3 Position
    {
        get => transform.position;
    }

    public Vector3 Forward
    {
        get => transform.forward;
    }

    public void ConnectMarker(Marker nextMarker)
    {
        adjacentMarkers.Add(nextMarker);
    }

    public bool GoesInSameDirection(Marker marker)
    {
        return Vector3.Angle(this.Forward, marker.Forward) < 45;
    }

    private void OnDrawGizmos()
    {
        if (Selection.activeObject == gameObject)
        {
            Gizmos.color = Color.red;
            if (adjacentMarkers.Count > 0)
            {
                foreach (var marker in adjacentMarkers)
                {
                    Gizmos.DrawLine(this.Position, marker.Position);
                }
            }
            Gizmos.color = Color.white;
        }
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
}

