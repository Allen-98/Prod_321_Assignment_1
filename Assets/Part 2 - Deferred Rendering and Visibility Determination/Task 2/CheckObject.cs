using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObject : MonoBehaviour
{

    FrustumCull frustumCull;

    

    // Start is called before the first frame update
    void Start()
    {
        frustumCull = FindObjectOfType<FrustumCull>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.Find("VisibilitySphere") != null)
        {
            Renderer sphereRenderer = transform.Find("VisibilitySphere").GetComponent<Renderer>();

            if (frustumCull.gameObjectsInFrustum.Contains(gameObject))
            {
                sphereRenderer.material.color = Color.red;
            }
            else
            {
                sphereRenderer.material.color = Color.blue;
            }
        }
    }
}
