using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightRoad : RoadWithWaitingPositions
{
    public override bool ComputeWaitingPosition(Vector3 forwardDirection, Vector3 position, out Vector3 waitingPosition)
    {
        GetReferencePosition(forwardDirection, out Transform referenceTransform);
        Vector3 rightComponent = Vector3.Scale(referenceTransform.position, Abs(referenceTransform.right));
        Vector3 forwardComponent = Vector3.Scale(position, Abs(referenceTransform.forward));
        Vector3 upComponent = Vector3.Scale(position, Abs(referenceTransform.up));
        waitingPosition = rightComponent + forwardComponent + upComponent;
        return IsInsideBounds(waitingPosition, referenceTransform);
    }

    public override bool IsInsideBounds(Vector3 position, Transform referenceTransform)
    {
        float forwardComp = Vector3.Dot(referenceTransform.forward, position);
        float highLimit = Vector3.Dot(referenceTransform.forward, referenceTransform.position);
        return forwardComp <= highLimit;
    }

    protected override bool GetReferencePosition(Vector3 forwardDirection, out Transform referenceTransform)
    {
        float angle = Vector3.Angle(forwardDirection, referencePoints[0].forward);
        bool goesSameDirection = angle <= 90;
        if (goesSameDirection)
        {
            referenceTransform = referencePoints[0];
        }
        else
        {
            referenceTransform = referencePoints[1];
        }
        return true;
    }
    
}
