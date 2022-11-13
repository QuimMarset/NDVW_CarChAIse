using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightsManager : MonoBehaviour
{
    [SerializeField]
    private List<TrafficLight> trafficLights;
    [SerializeField]
    private int greenTime;
    [SerializeField]
    private int currentGreen;

    // Start is called before the first frame update
    void Start()
    {
        currentGreen = 0;
        SetCurrentToGreen();
        SetOthersToRed();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLights();
    }

    private void UpdateLights()
    {
        if (TimeToTurnCurrentRed())
        {
            SetCurrentToRed();
            UpdateCurrent();
            SetCurrentToGreen();
        }
    }

    private void SetCurrentToGreen()
    {
        trafficLights[currentGreen].SetGreen();
        trafficLights[currentGreen].UpdateGreenStartTime();
    }

    private void SetCurrentToRed()
    {
        trafficLights[currentGreen].SetRed();
    }

    private bool TimeToTurnCurrentRed()
    {
        return trafficLights[currentGreen].TimeToTurnRed();
    }

    private void UpdateCurrent()
    {
        currentGreen++;
        if (currentGreen >= trafficLights.Count)
        {
            currentGreen = 0;
        }
    }

    private void SetOthersToRed()
    {
        for (int i = 0; i < trafficLights.Count; i++)
        {
            if (i != currentGreen)
            {
                trafficLights[i].SetRed();
            }
        }
    }
}
