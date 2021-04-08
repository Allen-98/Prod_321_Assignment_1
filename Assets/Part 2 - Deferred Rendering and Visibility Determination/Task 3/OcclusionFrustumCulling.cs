using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class implements occlusion frustum culling
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class OcclusionFrustumCulling : MonoBehaviour
{
    // The camera we will be generating occlusion frustums from
    public Camera occlusionCamera;

    // An array of mesh filters of objects which can occlude other objects
    public MeshFilter[] occlusionObjects;

    // A list of the occlusion frustums we will create for the objects above
    List<OcclusionFrustum> occlusionFrustums = new List<OcclusionFrustum>();

    // The material to use for our frustums
    public Material frustumMaterial;

    // An array of game objects to test for occulusion
    public Renderer[] gameObjectsToTestForOcclusion;

    // The game objects which are not occluded in the current frame
    public List<GameObject> gameObjectsNotOccluded = new List<GameObject>();

    // An array of visibility spheres for these game objects
    SphereCollider[] visibilitySpheres;

    // The material to use for our visibility spheres
    public Material visibilitySphereMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // If the occlusion camera hasn't been defined, try get the
        // camera tagged with MainCamera
        if (occlusionCamera == null)
            occlusionCamera = Camera.main;

        // Instantiate the visibility spheres list based on the number of game
        // objects to test for visibility
        visibilitySpheres = new SphereCollider[gameObjectsToTestForOcclusion.Length];

        // Loop through each game object to test for visibility
        for (int i = 0; i < gameObjectsToTestForOcclusion.Length; i++)
        {
            // If the game object already has a visibility sphere
            if (gameObjectsToTestForOcclusion[i].transform.Find("VisibilitySphere") != null)
            {
                // Lets just get use that one instead
                GameObject visibilitySphere = gameObjectsToTestForOcclusion[i].transform.Find("VisibilitySphere").gameObject;
                // Store the collider for the sphere into the visibility spheres array
                visibilitySpheres[i] = visibilitySphere.GetComponent<SphereCollider>();
            }
            else
            {
                // Create a new sphere
                GameObject visibilitySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // Set the gameobjects name to "VisibilitySphere"
                visibilitySphere.name = "VisibilitySphere";
                // Set it's parent as the game object
                visibilitySphere.transform.SetParent(gameObjectsToTestForOcclusion[i].transform, false);
                // Set it's size to the the game objects bounding box's extent magnitude * 2
                // (the magnitude is the half the size)
                visibilitySphere.transform.localScale = Vector3.one * gameObjectsToTestForOcclusion[i].bounds.extents.magnitude * 2;
                // Get the sphere's Renderer and update the material to our visibility sphere material
                visibilitySphere.GetComponent<Renderer>().material = visibilitySphereMaterial;
                // Store the collider for the sphere into the visibility spheres array
                visibilitySpheres[i] = visibilitySphere.GetComponent<SphereCollider>();
            }
        }

        // Loop through all our occlusion objects
        foreach (MeshFilter occlusionObject in occlusionObjects)
        {
            // Get the Occlusion Frustum on this object
            OcclusionFrustum occlusionFrustum = occlusionObject.gameObject.GetComponent<OcclusionFrustum>();
            // If there is none, add an occlusion frustum to it
            if (occlusionFrustum == null) occlusionFrustum = occlusionObject.gameObject.AddComponent<OcclusionFrustum>();
            // Add this occlusion frustum to our list of occlusion frustums
            occlusionFrustums.Add(occlusionFrustum);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Clear the list of game objects in the frustum
        gameObjectsNotOccluded.Clear();

        // Create a list of visibile objects, and populate it will all objects
        // to start with (we will remove objects as we determine they are not
        // visible
        List<SphereCollider> visibleObjects = new List<SphereCollider>(visibilitySpheres);

        // Loop through each occlusion frustum
        foreach (OcclusionFrustum occlusionFrustum in occlusionFrustums)
        {
            // Create lists for the frustum's plane's centers and normals
            List<Vector3> transformedFrustumCenters = new List<Vector3>();
            List<Vector3> transformedFrustumNormals = new List<Vector3>();

            // Get the frustum plane's centers and normals and store them in our lists
            occlusionFrustum.CalcFrustum(occlusionCamera, ref transformedFrustumCenters, ref transformedFrustumNormals);

            float normalLength = 5;
            // Draw the normals for our occlusion frustum planes
            for (int j = 0; j < transformedFrustumCenters.Count; j++) 
                Debug.DrawRay(transformedFrustumCenters[j], transformedFrustumNormals[j] * normalLength, Color.white);

            // Loop through all our visible objects
            for (int i = 0; i < visibleObjects.Count; i++)
            {
                // Assume it's in the frustum by default
                bool inFrustum = true;

                // Get the position of this objects bounding sphere
                Vector3 spherePos = visibleObjects[i].transform.position;
                // Calculate the radius of the object's bounding sphere
                float bounds = visibleObjects[i].radius * visibleObjects[i].transform.localScale.x;

                // Loop through all our transformed frustum planes
                for (int j = 0; j< transformedFrustumCenters.Count; j++)
                {
                    // Calculate the h distance of the point relative to each of the frustum planes
                    // the formula for h distance is h = (P - P_0) . N
                    // Where P is the point we are wanting to check what side of the plane it lays on
                    // P_0 is a point on the plane we're testing, and N is the planes normal
                    float h = Vector3.Dot((spherePos - transformedFrustumCenters[j]), transformedFrustumNormals[j]);

                    // If the h value is greater than the outer bounds limit
                    if (h>-bounds)
                    {
                        // The object is at least *partly* out of the occlusion volume
                        // set "inFrustum" to false and break
                        inFrustum = false;
                        break;
                    }
                }

                // Otherwise, if it was within all the bounds
                if (inFrustum)
                {
                    // It's completely in the frustum, remove it from
                    // the list of visible objects
                    visibleObjects.RemoveAt(i);

                    // Reduce i because we have removed this object from the list
                    i--;
                }
            }
        }

        // Loop through each game object we're testing
        for (int i = 0; i < gameObjectsToTestForOcclusion.Length; i++)
        {
            // Get its visibility sphere
            SphereCollider sphereCollider = visibilitySpheres[i].GetComponent<SphereCollider>();

            // Get the visibility sphere's mesh renderer
            MeshRenderer mr = sphereCollider.GetComponent<MeshRenderer>();

            // if the visiblity sphere is a visible object
            if (visibleObjects.Contains(sphereCollider))
            {
                // Add this object to the game objects in frustum list
                gameObjectsNotOccluded.Add(gameObjectsToTestForOcclusion[i].gameObject);
            }
        }
    }
}
