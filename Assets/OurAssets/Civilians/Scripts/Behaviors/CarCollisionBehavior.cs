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
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovRadius, layerMask, QueryTriggerInteraction.Ignore);

        foreach (Collider collider in colliders)
        {
            if (GameObject.ReferenceEquals(gameObject, collider.gameObject))
            {
                continue;
            }

            Vector3 closesPoint = collider.ClosestPoint(transform.position);
            Vector3 direction = (closesPoint - transform.position).normalized;
            Vector3 direction2 = (collider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(fovDirection, direction);
            float angle2 = Vector3.Angle(fovDirection, direction2);

            if (angle < fovAngle / 2 || angle2 < fovAngle / 2)
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
        bool detection = Physics.Raycast(transform.position, rayDirection, out RaycastHit hitInfo, rayDistance, layerMask, QueryTriggerInteraction.Ignore);
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
            if (otherController == null)
            {
                return false;
            }
            return otherController.IsMovingBackwards();
        }
        return false;
    }

    public bool IsThereSomeNonCarObstacleBlocking()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int civilianLayer = LayerMask.NameToLayer("Civilian");
        int policeLayer = LayerMask.NameToLayer("Police");
        int defaultLayer = 1 << LayerMask.NameToLayer("Default");

        bool detection = Physics.BoxCast(transform.position, new Vector3(1.5f, 1.5f, 1.5f), transform.forward,
            out RaycastHit hitInfo, transform.rotation, fovRadius, defaultLayer, QueryTriggerInteraction.Ignore);

        if (detection)
        {
            int layer = hitInfo.transform.gameObject.layer;
            Debug.Log(LayerMask.LayerToName(layer));
            if (layer != playerLayer && layer != civilianLayer && layer != policeLayer)
            {
                return true;
            }
        }
        return false;
    }
}
