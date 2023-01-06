using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceSirens : MonoBehaviour
{
    public GameObject redLight1, redLight2, blueLight1, blueLight2;
    public float waitTime = 0.05f;
    private float changeTime = 0.0f;
    private int state = 0;
    // Start is called before the first frame update
    void Start()
    {
    }

    void Update(){
        changeTime += Time.deltaTime;

        if (changeTime > waitTime)
        {
            changeTime = 0.0f;
            redLight1.SetActive(false);
            redLight2.SetActive(false);
            blueLight1.SetActive(false);
            blueLight2.SetActive(false);

            switch(state){
                case 0: redLight1.SetActive(true); break;   
                case 1: redLight2.SetActive(true); break;
                case 2: blueLight1.SetActive(true); break;
                case 3: blueLight2.SetActive(true); break;
                default: break;
            }

            state += 1;
            state %= 4;

        }
    }

}
