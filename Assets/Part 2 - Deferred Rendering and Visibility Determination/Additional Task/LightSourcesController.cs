using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSourcesController : MonoBehaviour
{
    public LightController lightController;

    public GameObject sun;

    float x;



    // Start is called before the first frame update
    void Start()
    {
        x = sun.transform.rotation.x;
    }

    // Update is called once per frame
    void Update()
    {

        if (!lightController.isNight)
        {
            //Debug.Log("Day " + sun.transform.localEulerAngles.x);
            this.gameObject.GetComponent<Light>().enabled = false;
        }
        
        if (lightController.isNight)
        {
            //Debug.Log("Night " + sun.transform.localEulerAngles.x);
            this.gameObject.GetComponent<Light>().enabled = true;        }

    }



}
