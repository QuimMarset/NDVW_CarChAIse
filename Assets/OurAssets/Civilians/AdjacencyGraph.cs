using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class AdjacencyGraph
{
    Dictionary<Marker, List<Marker>> adjacencyDictionary = new Dictionary<Marker, List<Marker>>();

    private void AddMarker(Marker marker)
    {
        if (adjacencyDictionary.ContainsKey(marker))
        {
            return;
        }
        adjacencyDictionary.Add(marker, new List<Marker>());
    }

    private Marker GetMarkerAt(Vector3 position)
    {
        return adjacencyDictionary.Keys.FirstOrDefault(x => CompareVertices(position, x.Position));
    }

    private bool CompareVertices(Vector3 position1, Vector3 position2)
    {
        return Vector3.SqrMagnitude(position1 - position2) < 0.0001f;
    }

    public void AddEdge(Marker origin, Marker destination)
    {
        if (origin.Equals(destination))
        {
            return;
        }
        
        if (MarkersAlreadyConnected(origin, destination) == false)
        {
            adjacencyDictionary[origin].Add(destination);
            ConnectMarkers(origin, destination);

        }
    }

    private void ConnectMarkers(Marker origin, Marker destination)
    {
        origin.adjacentMarkers.Add(destination);
    }

    private bool MarkersAlreadyConnected(Marker origin, Marker destination)
    {
        List<Marker> adjacentMarkers = adjacencyDictionary[origin];
        foreach (Marker marker in adjacentMarkers)
        {
            if (marker.Equals(destination))
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var Marker in adjacencyDictionary.Keys)
        {
            builder.AppendLine("Marker " + Marker.ToString() + " neighbours: " + String.Join(", ", adjacencyDictionary[Marker]));
        }
        return builder.ToString();
    }


}