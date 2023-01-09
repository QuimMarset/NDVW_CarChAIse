using System.Collections.Generic;
using UnityEngine;


public abstract class Road : MonoBehaviour
{
    [SerializeField]
    private List<Marker> markersSendingConnections;
    [SerializeField]
    private List<Marker> markersReceivingConnections;
    [SerializeField]
    private List<Marker> spawnMarkers;
    public List<Marker> allMarkers { get; protected set; }

    public virtual Vector3 Forward
    {
        get => transform.forward;
    }

    protected virtual void Start()
    {
        GatherAllMarkers();
    }

    private void GatherAllMarkers()
    {
        allMarkers = new();
        Transform markersContainer = transform.Find("Markers");
        for (int i = 0; i < markersContainer.childCount; ++i)
        {
            Marker marker = markersContainer.GetChild(i).GetComponent<Marker>();
            allMarkers.Add(marker);
        }
    }

    public void ConnectMarkers(List<Road> roads)
    {
        foreach (Marker marker in markersSendingConnections)
        {
            Marker closestMarker = marker.SearchMarkerToConnectWith(roads);
            marker.ConnectMarker(closestMarker);
        }
    }

    public List<Marker> GetMarkersReceivingConnections()
    {
        return markersReceivingConnections;
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

    public bool IsTheSame(Road road)
    {
        return GameObject.ReferenceEquals(this.gameObject, road.gameObject);
    }

    public Marker GetClosestMarker(Vector3 queryPosition, Vector3 forward)
    {
        Marker closestMarker = null;
        float minDistance = float.MaxValue;
        foreach (Marker marker in allMarkers)
        {
            float distance = Vector3.Distance(queryPosition, marker.Position);
            float angle = Vector3.Angle(forward, marker.Forward);
            if (distance < minDistance && angle <= 90)
            {
                minDistance = distance;
                closestMarker = marker;
            }
        }
        return closestMarker;
    }

    public abstract bool ComputeWaitingPosition(Vector3 forwardDirection, Vector3 position, out Vector3 waitingPosition);

    public abstract bool IsInsideBounds(Vector3 position, Transform referenceTransform);
}

