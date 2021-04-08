using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObjectTask3 : MonoBehaviour
{

    public FrustumCull frustumCull;
    public OcclusionFrustumCulling occlusionCull;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.Find("VisibilitySphere") != null)
        {
            // If we've found it, get it's renderer
            Renderer sphereRenderer = transform.Find("VisibilitySphere").GetComponent<Renderer>();

            // Check to see if this game object is in the frustum cull frustum
            // and not occluded
            if (frustumCull.gameObjectsInFrustum.Contains(gameObject) && occlusionCull.gameObjectsNotOccluded.Contains(gameObject))
            {
                // If so set up the the sphere to black and set is Out to true
                sphereRenderer.material.color = Color.black;
                
            }
            // Otherwise if the gameobject is either in the frustum OR is not
            // occluded
            else if (frustumCull.gameObjectsInFrustum.Contains(gameObject) || occlusionCull.gameObjectsNotOccluded.Contains(gameObject))
            {
                // Change the colour to red
                sphereRenderer.material.color = Color.red;
            }

            else
            {
                // If it's completely hidden, set the colour to blue
                sphereRenderer.material.color = Color.blue;
            }
        }
    }
}
