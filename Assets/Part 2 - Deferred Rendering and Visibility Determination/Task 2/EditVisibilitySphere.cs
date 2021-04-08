using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditVisibilitySphere : MonoBehaviour
{

    bool isChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (!isChanged)
        {
            ChangeSize();
        }


    }

    public void ChangeSize()
    {

        if (transform.Find("VisibilitySphere") != null)
        {

            Transform vs = transform.Find("VisibilitySphere").transform;

            vs.localScale = new Vector3(7, 7, 7);
 

        }

    }

}
