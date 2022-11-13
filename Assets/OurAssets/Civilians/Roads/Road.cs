using System.Collections.Generic;
using UnityEngine;


public abstract class Road : MonoBehaviour
{
    [SerializeField]
    protected List<Marker> connectableMarkers;
    [SerializeField]
    protected List<Marker> spawnMarkers;

    public virtual Vector3 Forward
    {
        get => transform.forward;
    }

    public List<Marker> GetConnectableMarkers()
    {
        return connectableMarkers;
    }

    public Marker GetPositionForCarToSpawn()
    {
        if (spawnMarkers.Count == 0)
        {
            return null;
        }
        int index = Random.Range(0, spawnMarkers.Count);
        return spawnMarkers[index];
    }

    private bool IsTheSame(Road road)
    {
        return GameObject.ReferenceEquals(this.gameObject, road.gameObject);
    }

    public abstract void ConnectMarkers(List<Road> roads);

    protected Marker GetMarkerToConnectWith(Marker queryMarker, List<Road> roads)
    {
        Marker closestMarker = null;
        float minDistance = float.MaxValue;

        foreach (Road road in roads)
        {
            if (IsTheSame(road))
            {
                continue;
            }

            Marker candidateClosestMarker = GetClosestMarker(queryMarker, road);
            if (candidateClosestMarker == null)
            {
                continue;
            }

            float distance = Vector3.Distance(queryMarker.Position, candidateClosestMarker.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestMarker = candidateClosestMarker;
            }
        }

        return closestMarker;
    }

    protected Marker GetClosestMarker(Marker queryMarker, Road road)
    {
        Marker closestMarker = null;
        float minDistance = float.MaxValue;
        foreach (Marker marker in road.GetConnectableMarkers())
        {
            float distance = Vector3.Distance(queryMarker.Position, marker.Position);

            // We make sure we are looking for a marker going in the same direction as queryMarker (same side of the road)
            Vector3 dif = Vector3.Normalize(marker.Position - queryMarker.Position);
            float angle = Vector3.Angle(queryMarker.Forward, dif);

            if (queryMarker.GoesInSameDirection(marker) && distance < minDistance && angle < 45)
            {
                minDistance = distance;
                closestMarker = marker;
            }
        }
        return closestMarker;
    }
}

