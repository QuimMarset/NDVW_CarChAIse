using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollisionBehavior : MonoBehaviour
{
    [SerializeField]
    private float fovRadius;
    [SerializeField]
    private float fovAngle;
    [SerializeField]
    private float fovRadiusBackward;
    [SerializeField]
    private float fovAngleBackward;

    [SerializeField]
    private bool carInFront;
    [SerializeField]
    private bool carBehind;

    public float FovRadius { get => fovRadius; }

    public float FovRadiusBackward { get => fovRadiusBackward; }
    
    public float FovAngle { get => fovAngle; }

    public float FovAngleBackward { get => fovAngleBackward; }

    public bool IsThereACarInFront()
    {
        carInFront = IsThereACarInFOV(transform.forward, fovRadius, fovAngle);
        return carInFront;
    }

    public bool IsThereACarBehind()
    {
        carBehind = IsThereACarInFOV(-transform.forward, fovRadiusBackward, fovAngleBackward);
        return carBehind;
    }

    private bool IsThereACarInFOV(Vector3 fovDirection, float fovRadius, float fovAngle)
    {
        int layerMask = LayerMask.GetMask(new string[] { "Civilian", "Police" });

        Collider[] colliders = Physics.OverlapSphere(transform.position, fovRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            if (GameObject.ReferenceEquals(gameObject, collider.gameObject))
            {
                continue;
            }

            Vector3 direction = (collider.transform.position - transform.position).normalized;
            if (Vector3.Angle(fovDirection, direction) < fovAngle / 2)
            {
                return true;
            }
        }
        return false;
    }
}
