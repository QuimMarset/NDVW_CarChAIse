using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CarController))]
public class CarSound : MonoBehaviour
{
    [SerializeField] protected float MinPitch;
    [SerializeField] protected float MaxPitch;

    protected CarController Car;
    protected AudioSource Source;

    // Start is called before the first frame update
    void Start()
    {
        Car = GetComponent<CarController>();
        Source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float speedRatio = Mathf.Abs(Car.CurrentWheelsSpeed) / Car.MaxSpeed;
        Source.pitch = Mathf.LerpUnclamped(MinPitch, MaxPitch, speedRatio);
    }
}
