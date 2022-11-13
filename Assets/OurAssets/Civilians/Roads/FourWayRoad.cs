using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourWayRoad : Road
{

    [SerializeField]
    private Marker rightMarkerToConnect;
    [SerializeField]
    private Marker leftMarkerToConnect;
    [SerializeField]
    private Marker rightMarkerToConnectSecond;
    [SerializeField]
    private Marker leftMarkerToConnectSecond;


    public override void ConnectMarkers(List<Road> roads)
    {
        Marker closestToRight = GetMarkerToConnectWith(rightMarkerToConnect, roads);
        rightMarkerToConnect.ConnectMarker(closestToRight);

        Marker closestToLeft = GetMarkerToConnectWith(leftMarkerToConnect, roads);
        leftMarkerToConnect.ConnectMarker(closestToLeft);

        Marker closestToSecondLeft = GetMarkerToConnectWith(leftMarkerToConnectSecond, roads);
        leftMarkerToConnectSecond.ConnectMarker(closestToSecondLeft);

        Marker closestToSecondRight = GetMarkerToConnectWith(rightMarkerToConnectSecond, roads);
        rightMarkerToConnectSecond.ConnectMarker(closestToSecondRight);
    }
}
