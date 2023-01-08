using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MoveBackwardsBehavior : MonoBehaviour
{

    [SerializeField]
    private bool movingBackwards;

    public bool BackwardMovementNeeded(Vector3 targetPosition, Transform carFront)
    {
        Vector3 direction = targetPosition - carFront.position;
        float angle = Vector3.Angle(carFront.forward, direction);
        movingBackwards = angle > 40f;
        return movingBackwards;
    }

}
