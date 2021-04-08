using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class provides some basic player controller
 * functionality using the keyboard, and also
 * stores references to the frustum cull and occlusion cull
 * scripts
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class PlayerControllerSolution : MonoBehaviour
{
    // Reference to the Frustum Cull Script
    public FrustumCull frustumCull;

    // Reference to the Occlusion Cull Script
    public OcclusionFrustumCulling occlusionCull;

    // The speed that the player can move
    public float moveSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Keep track of whether our player is "out"
    bool isOut = false;

    // Update is called once per frame
    void Update()
    {
        // if our player is "out", don't move anymore
        if (isOut) return;

        // Get the horizontal and vertical values for movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Update the position of the Game Object this script is attached to
        // Multiply the vertical movement amount by the transforms forward vector
        // and multiply that by the move speed multiplied by the amount of time
        // elapsed since the last frame (Time.deltaTime). Do the same for
        // the horizontal movement, but using the transform's right vector
        transform.position += transform.forward * v * Time.deltaTime * moveSpeed;
        transform.position += transform.right * h * Time.deltaTime * moveSpeed;

        // Try and get the visibility sphere which is attached to this player
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
                isOut = true;
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
