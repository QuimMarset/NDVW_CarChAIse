using System;
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
    private CollidersManager collidersManager;
    [SerializeField]
    private bool allowedToModifyColliders;

    private void Awake()
    {
        collidersManager = GetComponent<CollidersManager>();
    }

    private void Start()
    {
        currentGreen = 0;
        SetCurrentToGreen();
        SetOthersToRed();
        StartCoroutine(UpdateLights());
    }

    public void AllowToModifyColliders()
    {
        allowedToModifyColliders = true;
    }

    public void BlockColliderModification()
    {
        allowedToModifyColliders = false;
    }

    private IEnumerator UpdateLights()
    {
        while (true)
        {
            yield return new WaitForSeconds(greenTime);
            SetCurrentToRed();
            UpdateCurrent();
            SetCurrentToGreen();
        }
    }

    private void SetCurrentToGreen()
    {
        trafficLights[currentGreen].SetGreen();
        if (allowedToModifyColliders)
        {
            collidersManager.DisableCollider(currentGreen);
        }
    }

    public void FixTrafficLights()
    {
        SetCurrentToGreen();
        SetOthersToRed();
    }

    private void SetCurrentToRed()
    {
        trafficLights[currentGreen].SetRed();
        if (allowedToModifyColliders)
        {
            collidersManager.EnableCollider(currentGreen, WaitingReason.TrafficLight);
        }
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
                if (allowedToModifyColliders)
                {
                    collidersManager.EnableCollider(i, WaitingReason.TrafficLight);
                }
            }
        }
    }
}
