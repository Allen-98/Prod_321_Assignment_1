using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class implements frustum culling in code,
 * does some debug rendering, and stored objects
 * within the frustum in a publically accessible list
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */


public class FrustumCull : MonoBehaviour
{
    // The camera we will be doing frustum culling on
    public Camera frustumCamera;

    // An array of all the game objects we want to check for visibility
    public Renderer[] gameObjectsToTestForVisibility;

    // The game objects which are within the frustum at any given frame
    public List<GameObject> gameObjectsInFrustum = new List<GameObject>();

    // For each game object we will create a "visibility sphere" which
    // will be blue if it's visible and red otherwise. This sphere is
    // also used as the bounding volume to determine visibility
    SphereCollider[] visibilitySpheres;

    // The material to use for our debug frustum volume
    public Material frustumMaterial;
    // The material to use for our visibility spheres
    public Material visibilitySphereMaterial;

    // An array of the 8 vertices which make up the frustum
    Vector3[] fv = new Vector3[8];

    // An array of the center points of each of the 6 sides of the frustum
    // Stored in model space
    Vector3[] frustumCenters = new Vector3[6];
    // An array of the normals for each of the 6 sides of the frustum
    // Stored in model space
    Vector3[] frustumNormals = new Vector3[6];

    // Arrays to store the frustum centers and normals transformed into
    // World space at each frame
    Vector3[] transformedFrustumCenters = new Vector3[6];
    Vector3[] transformedFrustumNormals = new Vector3[6];

    // Start is called before the first frame update
    void Start()
    {
        // If the frustum camera hasn't been defined, try get the
        // camera tagged with MainCamera
        if (frustumCamera == null)
            frustumCamera = Camera.main;

        // Get the near plane, far plane, vertical field of view and aspect
        // ratio from the frustumCamera
        float nearPlane = frustumCamera.nearClipPlane;
        float farPlane = frustumCamera.farClipPlane;
        float fovY = frustumCamera.fieldOfView * Mathf.Deg2Rad;
        float aspect = frustumCamera.aspect;

        // Calculate the width and height of the near plane of the camera
        // Since the centre of these planes will be at 0,0, we can just
        // Store half this value and -/+ the value to find the near plane
        // vertices
        float nearHalfHeight = Mathf.Tan(fovY / 2f) * nearPlane;
        float nearHalfWidth = nearHalfHeight * aspect;

        // As above, but for the far plane
        float farHalfHeight = Mathf.Tan(fovY / 2f) * farPlane;
        float farHalfWidth = farHalfHeight * aspect;

        // Calculate the four vertices which make up the near plane
        fv[0] = new Vector3(-nearHalfWidth, -nearHalfHeight, nearPlane);
        fv[1] = new Vector3(nearHalfWidth, -nearHalfHeight, nearPlane);
        fv[2] = new Vector3(nearHalfWidth, nearHalfHeight, nearPlane);
        fv[3] = new Vector3(-nearHalfWidth, nearHalfHeight, nearPlane);

        // Calculate the four vertices which make up the far plane
        fv[4] = new Vector3(-farHalfWidth, -farHalfHeight, farPlane);
        fv[5] = new Vector3(farHalfWidth, -farHalfHeight, farPlane);
        fv[6] = new Vector3(farHalfWidth, farHalfHeight, farPlane);
        fv[7] = new Vector3(-farHalfWidth, farHalfHeight, farPlane);

        // Calculate the centres of the 6 planes that make up the frustum
        // (Just the average of the 4 points that make up the plane)
        frustumCenters[0] = (fv[0] + fv[1] + fv[2] + fv[3]) / 4; //Near
        frustumCenters[1] = (fv[4] + fv[5] + fv[6] + fv[7]) / 4; //Far
        frustumCenters[2] = (fv[4] + fv[0] + fv[3] + fv[7]) / 4; //Left
        frustumCenters[3] = (fv[1] + fv[5] + fv[6] + fv[2]) / 4; //Right
        frustumCenters[4] = (fv[7] + fv[6] + fv[2] + fv[3]) / 4; //Top
        frustumCenters[5] = (fv[4] + fv[5] + fv[1] + fv[0]) / 4; //Bottom

        // Calculate the normals of the 6 planes that make up the frustum
        // Take the cross product of two perpendicular edges of each plane
        frustumNormals[0] = Vector3.Cross(fv[3] - fv[0], fv[1] - fv[0]).normalized; //Near
        frustumNormals[1] = Vector3.Cross(fv[5] - fv[4], fv[6] - fv[5]).normalized; //Far
        frustumNormals[2] = Vector3.Cross(fv[7] - fv[4], fv[0] - fv[4]).normalized; //Left
        frustumNormals[3] = Vector3.Cross(fv[2] - fv[1], fv[5] - fv[1]).normalized; //Right
        frustumNormals[4] = Vector3.Cross(fv[3] - fv[2], fv[6] - fv[2]).normalized; //Top
        frustumNormals[5] = Vector3.Cross(fv[1] - fv[0], fv[4] - fv[0]).normalized; //Bottom

        // An array to store the 12 triangles that make up the frustum
        // 6 sides with each side being made of 2 triangles, 3 vertices per triangle
        // = 6 * 2 * 3 = 36
        int[] t = new int[36];

        // Vertices for the triangles for the near plane
        t[0] = 2;
        t[1] = 1;
        t[2] = 0;
        t[3] = 3;
        t[4] = 2;
        t[5] = 0;

        // Vertices for the triangles for the far plane
        t[6] = 6;
        t[7] = 5;
        t[8] = 4;
        t[9] = 7;
        t[10] = 6;
        t[11] = 4;

        // Vertices for the triangles for the left plane
        t[12] = 0;
        t[13] = 4;
        t[14] = 7;
        t[15] = 0;
        t[16] = 7;
        t[17] = 3;

        // Vertices for the triangles for the right plane
        t[18] = 5;
        t[19] = 1;
        t[20] = 2;
        t[21] = 5;
        t[22] = 2;
        t[23] = 6;

        // Vertices for the triangles for the top plane
        t[24] = 3;
        t[25] = 7;
        t[26] = 6;
        t[27] = 3;
        t[28] = 6;
        t[29] = 2;

        // Vertices for the triangles for the bottom plane
        t[30] = 0;
        t[31] = 4;
        t[32] = 5;
        t[33] = 0;
        t[34] = 5;
        t[35] = 1;

        // Create a mesh from the vertices and triangles defined above
        Mesh mesh = new Mesh();
        mesh.vertices = fv;
        mesh.triangles = t;

        // Create a mesh filter for this mesh and assign the mesh to it
        MeshFilter m = gameObject.AddComponent<MeshFilter>();
        m.mesh = mesh;

        // Create a mesh renderer and assign the frustum material to it
        Renderer r = gameObject.AddComponent<MeshRenderer>();
        r.material = frustumMaterial;

        // Populate the visibility spheres array with sphere colliders for all
        // the game objects to test for visibility
        visibilitySpheres = new SphereCollider[gameObjectsToTestForVisibility.Length];

        // Loop through this array
        for (int i=0; i< gameObjectsToTestForVisibility.Length; i++)
        {
            // If the game object already has a visibility sphere
            if (gameObjectsToTestForVisibility[i].transform.Find("VisibilitySphere") != null)
            {
                // Lets just get use that one instead
                GameObject visibilitySphere = gameObjectsToTestForVisibility[i].transform.Find("VisibilitySphere").gameObject;

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
                visibilitySphere.transform.SetParent(gameObjectsToTestForVisibility[i].transform, false);
                // Set it's size to the the game objects bounding box's extent magnitude * 2
                // (the magnitude is the half the size)
                visibilitySphere.transform.localScale = Vector3.one * gameObjectsToTestForVisibility[i].bounds.extents.magnitude * 2;
                // Get the sphere's Renderer and update the material to our visibility sphere material
                visibilitySphere.GetComponent<Renderer>().material = visibilitySphereMaterial;
                // Store the collider for the sphere into the visibility spheres array
                visibilitySpheres[i] = visibilitySphere.GetComponent<SphereCollider>();
            }
        }
    }

    // Draw the normals at each side of the frustum
    void DrawNormals()
    {
        float normalLength = 5;
        // Draw a ray from the centre of the side of the frustum, in the direction of the normal * by the normal length
        // User a different colour for each normal
        Debug.DrawRay(transformedFrustumCenters[0], transformedFrustumNormals[0] * normalLength, Color.blue); // Near Plane Normal
        Debug.DrawRay(transformedFrustumCenters[1], transformedFrustumNormals[1] * normalLength, Color.red); // Far Plane Normal
        Debug.DrawRay(transformedFrustumCenters[2], transformedFrustumNormals[2] * normalLength, Color.cyan); // Left Plane Normal
        Debug.DrawRay(transformedFrustumCenters[3], transformedFrustumNormals[3] * normalLength, Color.yellow); // Right Plane Normal
        Debug.DrawRay(transformedFrustumCenters[4], transformedFrustumNormals[4] * normalLength, Color.black); // Top Plane Normal
        Debug.DrawRay(transformedFrustumCenters[5], transformedFrustumNormals[5] * normalLength, Color.white); // Bottom Plane Normal

    }

    // Update is called once per frame
    void Update()
    {
        // Clear the list of game objects in the frustum
        gameObjectsInFrustum.Clear();

        // Loop through all 6 of the frustum planes
        for (int i = 0; i < 6; i++)
        {
            // And update the transformed frustum centers and transformed frustum normals
            transformedFrustumCenters[i] = transform.TransformPoint(frustumCenters[i]);
            transformedFrustumNormals[i] = transform.TransformDirection(frustumNormals[i]);
        }

        // Call the draw normals method
        DrawNormals();

        // Loop through every visibility sphere to test
        for (int i=0; i<visibilitySpheres.Length; i++)
        {
            // Get it's position
            Vector3 spherePos = visibilitySpheres[i].transform.position;

            // Calculate the h distance of the sphere relative to each of the frustum planes
            // the formula for h distance is h = (P - P_0) . N
            // Where P is the point we are wanting to check what side of the plane it lays on
            // P_0 is a point on the plane we're testing, and N is the planes normal
            float hNear = Vector3.Dot((spherePos - transformedFrustumCenters[0]), transformedFrustumNormals[0]); //Test Near
            float hFar = Vector3.Dot((spherePos - transformedFrustumCenters[1]), transformedFrustumNormals[1]); //Test Far
            float hLeft = Vector3.Dot((spherePos - transformedFrustumCenters[2]), transformedFrustumNormals[2]); //Test Left
            float hRight = Vector3.Dot((spherePos - transformedFrustumCenters[3]), transformedFrustumNormals[3]); //Test Right
            float hTop = Vector3.Dot((spherePos - transformedFrustumCenters[4]), transformedFrustumNormals[4]); //Test Top
            float hBottom = Vector3.Dot((spherePos - transformedFrustumCenters[5]), transformedFrustumNormals[5]); //Test Bottom

            // Get the mesh render of the sphere we're going to test
            MeshRenderer mr = visibilitySpheres[i].GetComponent<MeshRenderer>();

            // Find out the radius of the sphere based on the renderers size and
            // the transformed scale of the sphere
            float radius = visibilitySpheres[i].radius * visibilitySpheres[i].transform.localScale.x;

            // if h < radius, it means that the sphere is on the opposite side
            // of the plane to the planes normal, or in the case of our frustum
            // it is on the "inside" of that plane. If the sphere is on the "inside"
            // of all the frustum planes, it must be inside the frustum
            if (hNear < radius && hFar < radius && hLeft < radius && hRight < radius && hTop < radius && hBottom < radius)
            {
                // And add the game object to the list which are in the frustum
                gameObjectsInFrustum.Add(gameObjectsToTestForVisibility[i].gameObject);
            } 
        }
    }

    
}
