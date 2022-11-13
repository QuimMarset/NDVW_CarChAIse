using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CivilianAI : MonoBehaviour
{
    public Marker targetMarker;

    [SerializeField]
    private float arriveDistance = .3f;

    [field: SerializeField]
    public UnityEvent<Vector3> OnDrive { get; set; }

    [field: SerializeField]
    public UnityEvent<bool> OnTrafficLightCollision { get; set; }

    private void Update()
    {
        CheckIfArrived();
        SendPosition();
    }

    public void SetTargetMarker(Marker targetMarker)
    {
        this.targetMarker = targetMarker;
    }

    private void SendPosition()
    {
        OnDrive?.Invoke(targetMarker.Position);
    }

    private void CheckIfArrived()
    {
        Vector2 pos1 = new(targetMarker.Position.x, targetMarker.Position.z);
        Vector2 pos2 = new(transform.position.x, transform.position.z);
        float distance = Vector2.Distance(pos1, pos2);
        if (distance < arriveDistance)
        {
            SetNextTarget();
        }
    }

    private void SetNextTarget()
    {
        targetMarker = targetMarker.GetNextAdjacentMarker();
    }

    public void StopToRedLight()
    {
        OnTrafficLightCollision?.Invoke(true);
    }

    public void MoveToGreenLight()
    {
        OnTrafficLightCollision?.Invoke(false);
    }
}