using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeWayRoad : Road
{
    [SerializeField]
    private Marker rightMarkerToConnect;
    [SerializeField]
    private Marker leftMarkerToConnect;
    [SerializeField]
    private Marker rightMarkerToConnectSecond;


    public override void ConnectMarkers(List<Road> roads)
    {
        Marker closestToRight = GetMarkerToConnectWith(rightMarkerToConnect, roads);
        rightMarkerToConnect.ConnectMarker(closestToRight);

        Marker closestToLeft = GetMarkerToConnectWith(leftMarkerToConnect, roads);
        leftMarkerToConnect.ConnectMarker(closestToLeft);

        Marker closestToSecondRight = GetMarkerToConnectWith(rightMarkerToConnectSecond, roads);
        rightMarkerToConnectSecond.ConnectMarker(closestToSecondRight);
    }

}
