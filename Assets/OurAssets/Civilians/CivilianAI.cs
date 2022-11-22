using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CivilianAI : MonoBehaviour
{
    private CivilianController controller;
    [SerializeField]
    public Transform carFront;

    [Header("Stop flags")]

    [SerializeField]
    private bool stopForCollision = false;
    [SerializeField]
    private bool stopForTrafficLight = false;
    [SerializeField]
    private bool stopForObstacle = false;

    [Header("FOV")]

    [SerializeField]
    public float fovRadius;
    [SerializeField]
    public float fovAngle;

    [Header("Target position")]
    
    [SerializeField]
    private Marker targetMarker;
    [SerializeField]
    private float arriveDistance = .3f;
    [SerializeField]
    private Vector3 targetPosition;

    [Header("Police avoidance")]

    [SerializeField]
    private bool avoidPolice = false;


    private void Awake()
    {
        controller = gameObject.GetComponent<CivilianController>();
    }

    private void Start()
    {
        arriveDistance = 1.0f;
        targetPosition = targetMarker.Position;
        SetControllerTargetPosition();
        SetControllerStopFlag();
    }

    private void Update()
    {

        if (CheckForCollisions())
        {
            StopToAvoidCarCollision();
        }
        else
        {
            MoveWhenNoCollision();
        }

        if (avoidPolice)
        {
            if (CheckIfArrivedToTargetPosition())
            {
                stopForObstacle = true;
            }
        }
        else if (CheckIfArrivedToTargetPosition())
        {
            AdvanceTargetMarker();
            SetControllerTargetPosition();
        }
    }

    public void SetTargetMarker(Marker targetMarker)
    {
        this.targetMarker = targetMarker;
    }

    private void SetControllerTargetPosition()
    {
        controller.SetTargetPosition(targetPosition);
    }

    private void SetControllerStopFlag()
    {
        controller.SetStopFlag(stopForTrafficLight || stopForCollision || stopForObstacle);
    }

    private bool CheckIfArrivedToTargetPosition()
    {
        Vector2 subTargetPosition = new(targetPosition.x, targetPosition.z);
        Vector2 subCurrentPosition = new(carFront.position.x, carFront.position.z);
        float distance = Vector2.Distance(subTargetPosition, subCurrentPosition);

        if (avoidPolice)
        {
            Debug.Log(name + " " + carFront.position + " " + distance);
        }
        
        return (distance < arriveDistance);
    }

    private void AdvanceTargetMarker()
    {
        targetMarker = targetMarker.GetNextAdjacentMarker();
        targetPosition = targetMarker.Position;
    }

    public void StopToRedLight()
    {
        stopForTrafficLight = true;
        SetControllerStopFlag();
    }

    public void MoveToGreenLight()
    {
        stopForTrafficLight = false;
        SetControllerStopFlag();
    }

    private void StopToAvoidCarCollision()
    {
        stopForCollision = true;
        SetControllerStopFlag();
    }

    private void MoveWhenNoCollision()
    {
        stopForCollision = false;
        SetControllerStopFlag();
    }

    private bool CheckForCollisions()
    {
        Collider[] colliders = Physics.OverlapSphere(carFront.position, fovRadius, 1 << gameObject.layer);

        foreach (Collider collider in colliders)
        {
            if (GameObject.ReferenceEquals(gameObject, collider.gameObject))
            {
                continue;
            }

            Vector3 direction = (collider.transform.position - carFront.position).normalized;
            if (Vector3.Angle(transform.forward, direction) < fovAngle / 2)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (Selection.activeObject == gameObject)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            
        }
    }

    public void StartDetectingPolice()
    {
        avoidPolice = true;
        targetPosition = carFront.position + transform.forward * 3.0f + transform.right * 3.0f;
        SetControllerTargetPosition();
    }

    public void StopDetectingPolice()
    {
        avoidPolice = false;
        stopForObstacle = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(name + " Trigger enter");
        if (other.CompareTag("Police"))
        {
            StartDetectingPolice();
        }

    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Police"))
        {
            StopDetectingPolice();
        }
    }

}