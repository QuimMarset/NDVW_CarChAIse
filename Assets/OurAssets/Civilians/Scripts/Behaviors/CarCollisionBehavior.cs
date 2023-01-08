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

    [SerializeField]
    private bool useFOV;

    public float FovRadius { get => fovRadius; }

    public float FovRadiusBackward { get => fovRadiusBackward; }
    
    public float FovAngle { get => fovAngle; }

    public float FovAngleBackward { get => fovAngleBackward; }

    public bool DetectionMode { get => useFOV; }

    public void SetFOVMode()
    {
        useFOV = true;
    }

    public void SetRayMode()
    {
        useFOV = false;
    }

    private bool IsThereACarInFOV(Vector3 fovDirection, float fovRadius, float fovAngle, out Collider colliderInFOV)
    {
        int layerMask = LayerMask.GetMask(new string[] { "Civilian", "Police", "Player" });
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            if (GameObject.ReferenceEquals(gameObject, collider.gameObject))
            {
                continue;
            }

            Vector3 closesPoint = collider.ClosestPoint(transform.position);
            Vector3 direction = (closesPoint - transform.position).normalized;

            if (Vector3.Angle(fovDirection, direction) < fovAngle / 2)
            {
                colliderInFOV = collider;
                return true;
            }
        }

        colliderInFOV = null;
        return false;
    }

    private bool IsThereACarInForwardRay(Vector3 rayDirection, float rayDistance, out Collider colliderInRay)
    {
        int layerMask = LayerMask.GetMask(new string[] { "Civilian", "Police", "Player" });
        bool detection = Physics.Raycast(transform.position, rayDirection, out RaycastHit hitInfo, rayDistance, layerMask);
        if (detection)
        {
            colliderInRay = hitInfo.collider;
            return true;
        }
        colliderInRay = null;
        return false;
    }

    public bool IsThereACarInFront()
    {
        if (useFOV)
        {
            return IsThereACarInFOV(transform.forward, fovRadius, fovAngle, out Collider colliderInFOV);
        }
        return IsThereACarInForwardRay(transform.forward, fovRadius, out Collider colliderInRay);
    }

    public bool IsThereACarBehind()
    {
        int layerMask = LayerMask.GetMask(new string[] { "Civilian", "Police", "Player" });
        carBehind = IsThereACarInFOV(-transform.forward, fovRadiusBackward, fovAngleBackward, out Collider colliderInFOV);
        return carBehind;
    }

    public bool IsThereACarInFrontMovingInTheOppositeDirection()
    {
        int layerMask = LayerMask.GetMask(new string[] { "Civilian", "Police", "Player" });
        carInFront = IsThereACarInFOV(transform.forward, fovRadius, fovAngle, out Collider colliderInFOV);

        if (carInFront)
        {
            Vector3 otherCarForward = colliderInFOV.transform.forward;
            float angle = Vector3.Angle(transform.forward, otherCarForward);
            return angle > 90;
        }

        return false;
    }

    public bool IsThereACarInFrontMovingBackwards()
    {
        carInFront = IsThereACarInFOV(transform.forward, fovRadius, fovAngle, out Collider colliderInFOV);
        if (carInFront)
        {
            CivilianController otherController = colliderInFOV.GetComponent<CivilianController>();
            return otherController.IsMovingBackwards();
        }
        return false;
    }
}
