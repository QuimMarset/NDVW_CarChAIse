using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidanceBehavior : MonoBehaviour
{
    [SerializeField]
    private float sensorLength;
    [SerializeField]
    private float horizontalOffset;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float steerDirection;
    [SerializeField]
    private bool detected;

    public float SteerDirection
    {
        get => steerDirection;
    }

    public bool ObstacleDetected(Transform carFront, Vector3 targetPosition)
    {
        bool detectedLeft = CheckLeftSensor(carFront);
        bool detectedRight = CheckRightSensor(carFront);
        bool detectedLeftCenter = CheckLeftCenterSensor(carFront);
        bool detectedRightCenter = CheckRightCenterSensor(carFront);
        bool detectedCenter = CheckCenterSensor(carFront, targetPosition);
        bool detectedRightSide = CheckRightSideSensor(targetPosition, carFront);

        detected = (detectedLeft || detectedRight || detectedLeftCenter 
            || detectedRightCenter || detectedCenter ||detectedRightSide);

        if (!detected)
        {
            steerDirection = 0f;
        }
        return detected;
    }

    private bool CheckLeftSensor(Transform carFront)
    {
        RaycastHit hit;
        Vector3 startPosition = carFront.position - carFront.right * horizontalOffset;
        if (ComputeRaycast(startPosition, -angle, sensorLength, out hit))
        {
            steerDirection = Mathf.Min(steerDirection + 1.0f, 1.0f);
            return true;
        }
        return false;
    }

    private bool CheckRightSensor(Transform carFront)
    {
        RaycastHit hit;
        Vector3 startPosition = carFront.position + carFront.right * horizontalOffset;
        if (ComputeRaycast(startPosition, angle, sensorLength, out hit))
        {
            steerDirection = Mathf.Max(steerDirection - 1.0f, -1.0f);
            return true;
        }
        return false;
    }

    private bool CheckLeftCenterSensor(Transform carFront)
    {
        RaycastHit hit;
        Vector3 startPosition = carFront.position - carFront.right * horizontalOffset;
        if (ComputeRaycast(startPosition, 0f, sensorLength, out hit))
        {
            steerDirection = Mathf.Min(steerDirection + 0.5f, 1.0f);
            return true;
        }
        return false;
    }

    private bool CheckRightCenterSensor(Transform carFront)
    {
        RaycastHit hit;
        Vector3 startPosition = carFront.position + carFront.right * horizontalOffset;
        if (ComputeRaycast(startPosition, 0f, sensorLength, out hit))
        {
            steerDirection = Mathf.Max(steerDirection - 0.5f, -1.0f);
            return true;
        }
        return false;
    }

    private bool CheckCenterSensor(Transform carFront, Vector3 targetPosition)
    {
        RaycastHit hit;
        if (ComputeRaycast(carFront.position, 0f, sensorLength, out hit))
        {
            float rightComponent = Vector3.Dot(carFront.position, transform.right);
            float rightComponentTarget = Vector3.Dot(targetPosition, transform.right);
            if (rightComponentTarget > rightComponent)
            {
                steerDirection = 1;
            }
            else if (rightComponentTarget < rightComponent)
            {
                steerDirection = -1;
            }
            else
            {
                steerDirection = 0;
            }
            return true;
        }
        return false;
    }

    private bool CheckRightSideSensor(Vector3 targetPosition, Transform carFront)
    {
        RaycastHit hit;
        Vector3 startPosition = transform.position + transform.right * horizontalOffset;
        if (ComputeRaycast(startPosition, 90f, sensorLength / 4, out hit))
        {
            steerDirection = -0.1f;
            return true;
        }

        startPosition = carFront.position + transform.right * horizontalOffset;
        if (ComputeRaycast(startPosition, 90f, sensorLength / 4, out hit))
        {
            steerDirection = -0.1f;
            return true;
        }

        return false;
    }

    private bool ComputeRaycast(Vector3 startPosition, float sensorAngle, float sensorLength, out RaycastHit hit)
    {
        Vector3 direction = Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward;
        int layerMask = LayerMask.GetMask(new string[3] { "Police", "Civilian", "Player" });
        bool hasHit = Physics.Raycast(startPosition, direction, out hit, sensorLength, layerMask);
        if (hasHit)
        {
            Debug.DrawLine(startPosition, hit.point);
        }
        return hasHit;
    }
}
