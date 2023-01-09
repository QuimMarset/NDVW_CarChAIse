using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoadWithWaitingPositions : Road
{
    [SerializeField]
    protected List<Transform> referencePoints;

    protected abstract bool GetReferencePosition(Vector3 forwardDirection, out Transform referenceTransform);

    protected Vector3 Abs(Vector3 vector)
    {
        float x = Mathf.Abs(vector.x);
        float y = Mathf.Abs(vector.y);
        float z = Mathf.Abs(vector.z);
        return new(x, y, z);
    }

}
