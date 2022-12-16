using UnityEngine;

public class TrafficLightBehavior : MonoBehaviour
{
    [SerializeField]
    private bool redLight;

    private void Awake()
    {
        redLight = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InvisibleBlocker"))
        {
            InvisibleBlocker invisibleBlocker = other.GetComponent<InvisibleBlocker>();
            if (invisibleBlocker.IsATrafficLight())
            {
                redLight = true;
            }
        }
    }

    public void ResumeMovement()
    {
        redLight = false;
    }


    public bool IsThereARedLightInFront()
    {
        return redLight;
    }
}
