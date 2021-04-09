using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{

    public float Mult=5;
    public GameObject sun;

    float angle = 0;
    float result = 0;
    public bool isNight;

    // Start is called before the first frame update
    void Start()
    {
        isNight = false;
    }

    // Update is called once per frame
    void Update()
    {
        sun.transform.Rotate(Vector3.right * Mult * Time.deltaTime);

        angle += 1 * Mult * Time.deltaTime;

        result = ((angle / 360) - Mathf.Floor(angle / 360)) * 360;

        Debug.Log(result);

        if (result >= 0 && result < 180)
        {
            isNight = false;
        } else if (result >= 180 && result < 360)
        {
            isNight = true;
        }





    }
}
