using System.Collections.Generic;
using UnityEngine;


public class InvisibleBlocker : MonoBehaviour
{
    [SerializeField]
    private WaitingReason waitingReason; 
    private BoxCollider boxCollider;
    [SerializeField]
    private List<TrafficLightBehavior> trafficLightBehaviors;
    [SerializeField]
    private List<PoliceAvoidanceBehavior> policeAvoidanceBehaviors;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        DisableCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Civilian") || other.CompareTag("Police"))
        {
            if (waitingReason == WaitingReason.TrafficLight)
            {
                trafficLightBehaviors.Add(other.GetComponent<TrafficLightBehavior>());
            }
            else if (waitingReason == WaitingReason.PoliceCar)
            {
                policeAvoidanceBehaviors.Add(other.GetComponent<PoliceAvoidanceBehavior>());
            }
        }
    }

    public void EnableCollider(WaitingReason enableReason)
    {
        boxCollider.enabled = true;
        waitingReason = enableReason;
        trafficLightBehaviors.Clear();
        policeAvoidanceBehaviors.Clear();
    }

    public void DisableCollider()
    {
        boxCollider.enabled = false;
        HandleColliderDisabling();
        waitingReason = WaitingReason.None;
    }

    private void HandleColliderDisabling()
    {
        if (waitingReason == WaitingReason.TrafficLight)
        {
            ResumeMovementWhenRedLight();
        }
        else if (waitingReason == WaitingReason.PoliceCar)
        {
            ResumeMovementWhenPolice();
        }
    }

    public bool IsATrafficLight()
    {
        return waitingReason == WaitingReason.TrafficLight;
    }

    public bool IsAPoliceCar()
    {
        return waitingReason == WaitingReason.PoliceCar;
    }

    private void ResumeMovementWhenRedLight()
    {
        foreach (TrafficLightBehavior behavior in trafficLightBehaviors)
        {
            behavior.ResumeMovement();
        }
    }

    private void ResumeMovementWhenPolice()
    {
        foreach (PoliceAvoidanceBehavior behavior in policeAvoidanceBehaviors)
        {
            behavior.ResumeMovement();
        }
    }
}
