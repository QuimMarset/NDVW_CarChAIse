using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveRoad : RoadWithWaitingPositions
{
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
            referenceTransform = referencePoints[3];
        }
        return true;
    }

    private Vector3 ComputePositionInArc(Vector3 position, Vector3 referencePosition)
    {
        Vector3 pivot = transform.Find("Reference Points").transform.position;
        Vector3 dir1 = (referencePosition - pivot);
        Vector3 dir2 = (position - pivot);
        float angle = Vector3.Angle(dir1.normalized, dir2.normalized);
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);

        Vector3 newPosition = rotation * dir1 + pivot;
        return newPosition;
    }

    public override bool ComputeWaitingPosition(Vector3 forwardDirection, Vector3 position, out Vector3 waitingPosition)
    {
        GetReferencePosition(forwardDirection, out Transform referenceTransform);
        waitingPosition = ComputePositionInArc(position, referenceTransform.position);
        return IsInsideBounds(waitingPosition, referenceTransform);
    }

    public override bool IsInsideBounds(Vector3 position, Transform referenceTransform)
    {
        float forwardComp = Vector3.Dot(referenceTransform.forward, position);
        float highLimit = Vector3.Dot(referenceTransform.forward, referenceTransform.position);
        return forwardComp <= highLimit;
    }
}
