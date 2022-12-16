using UnityEngine;


public class TrafficLight : MonoBehaviour
{
    [SerializeField]
    private TrafficLightState state;
  
    private Light redLight;
    private Light greenLight;
    

    protected virtual void Awake()
    {
        Transform lightsContainer = transform.Find("Lights");
        redLight = lightsContainer.Find("RedLight").GetComponent<Light>();
        greenLight = lightsContainer.Find("GreenLight").GetComponent<Light>();
    }

    protected void Start()
    {
        redLight.range *= transform.lossyScale.x;
        greenLight.range *= transform.lossyScale.x;
    }

    public bool IsRedState()
    {
        return state == TrafficLightState.Red;
    }

    public bool IsGreenState()
    {
        return state == TrafficLightState.Green;
    }

    public virtual void SetRed()
    {
        state = TrafficLightState.Red;
        redLight.enabled = true;
        greenLight.enabled = false;
    }

    public virtual void SetGreen()
    {
        state = TrafficLightState.Green;
        greenLight.enabled = true;
        redLight.enabled = false;
    }
}
