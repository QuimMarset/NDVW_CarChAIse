using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum LightEnum
{
    Red,
    Blue
}

public class PoliceEmitter : MonoBehaviour
{

    [SerializeField]
    private Light redLight;
    [SerializeField]
    private Light blueLight;
    [SerializeField]
    private LightEnum lightState;


    // Start is called before the first frame update
    void Start()
    {
        lightState = LightEnum.Red;
        StartCoroutine(ChangeLights());
    }

    IEnumerator ChangeLights()
    {
        while (true)
        {
            ChangeLight();
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void ChangeLight()
    {
        if (lightState == LightEnum.Red)
        {
            redLight.enabled = false;
            blueLight.enabled = true;
            lightState = LightEnum.Blue;
        }
        else
        {
            redLight.enabled = true;
            blueLight.enabled = false;
            lightState = LightEnum.Red;
        }
    }
}
