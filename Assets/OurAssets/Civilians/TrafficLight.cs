using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum TrafficLightState
{
    Red,
    Green
}

public class TrafficLight : MonoBehaviour
{
    [SerializeField]
    private float greenTime;
    [SerializeField]
    private float lastGreenStart;
    [SerializeField]
    private BoxCollider boxCollider;
    [SerializeField]
    private TrafficLightState state;
    [SerializeField]
    private Light redLight;
    [SerializeField]
    private Light greenLight;
    [SerializeField]
    private CivilianAI stoppedCar;

    public bool IsRedState()
    {
        return state == TrafficLightState.Red;
    }

    public bool IsGreenState()
    {
        return state == TrafficLightState.Green;
    }

    public bool TimeToTurnRed()
    {
        return (Time.time - lastGreenStart >= greenTime);
    }

    public void UpdateGreenStartTime()
    {
        lastGreenStart = Time.time;
    }

    public void SetRed()
    {
        state = TrafficLightState.Red;
        boxCollider.enabled = true;
        redLight.enabled = true;
        greenLight.enabled = false;
        stoppedCar = null;
    }

    public void SetGreen()
    {
        state = TrafficLightState.Green;
        boxCollider.enabled = false;
        greenLight.enabled = true;
        redLight.enabled = false;
        if (stoppedCar != null)
        {
            stoppedCar.MoveToGreenLight();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Civilian"))
        {
            stoppedCar = other.GetComponent<CivilianAI>();
            stoppedCar.StopToRedLight();
        }
    }
}
