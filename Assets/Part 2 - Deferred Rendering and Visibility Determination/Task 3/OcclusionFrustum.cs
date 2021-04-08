using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class creates an occlusion frustum for any given
 * object. This is quite complicated, as we need to be
 * able to find the silhoutte edges of an object, and then
 * create a frustum plane for each edge.
 * 
 * To find silhouette edges, we loop through every triangle
 * in our object, and for each edge of each triangle, see if
 * two or more non-coplanar triangles share this edge - i.e.
 * two triangles at an angle to each other. If so, it could 
 * possibly be a silhoutte edge, depending on where the camera 
 * is at a given frame
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class OcclusionFrustum : MonoBehaviour
{
    // The mesh filter of our occluding object
    public MeshFilter occludingObjectMeshFilter;

    // The game object which contains our frustum renderer
    public GameObject frustumRenderer;

    // The mesh filter for the frustum of our occluding object
    MeshFilter frustumMeshFilter;

    // A list of the outer edges (silhoutte edges) of our
    // occluding object
    List<SharedEdge> outerEdges;

    // The center of mass of our object
    Vector3 centerOfMass = Vector3.zero;

    // A helper class which represents an edge and also
    // keeps a track of the normals and centers of all triangles
    // which share that edge
    class SharedEdge
    {
        // The two vertices which define the edge
        public Vector3 v1, v2;
        public List<Vector3> triNormals;
        public List<Vector3> triCenters;

        // The constructor takes the two vertices which make this edge
        // as well as the the normal and center of the triangle which
        // we first found this edge as a part of
        public SharedEdge(Vector3 _v1, Vector3 _v2, Vector3 n, Vector3 center)
        {
            // Store the vertices which make up the edge
            v1 = _v1; v2 = _v2;

            // Initialize our list of normals for triangles which
            // share this edge, and add the normal of the first triangle
            // we found this edge part of
            triNormals = new List<Vector3>();
            triNormals.Add(n);

            // Initialize our list of centers for triangles which
            // share this edge, and add the center of the first triangle
            // we found this edge part of
            triCenters = new List<Vector3>();
            triCenters.Add(center);
        }

        // This edge against two vertices which make another edge
        // returning true if they are within a threshold
        public bool Compare(Vector3 otherV1, Vector3 otherV2, float threshold)
        {
            // Calculate the distances between this edge's vertices
            // and the other edges vertices
            float dv1v1 = Vector3.Distance(v1, otherV1);
            float dv2v2 = Vector3.Distance(v2, otherV2);

            // return true if they are within a threshold
            if (dv1v1 < threshold && dv2v2 < threshold)
                return true;

            // calculate the distances between this edge's vertices
            // and the other edges opposite vertices
            float dv1v2 = Vector3.Distance(v1, otherV2);
            float dv2v1 = Vector3.Distance(v2, otherV1);

            // return true if they are within a threshold
            if (dv1v2 < threshold && dv2v1 < threshold)
                return true;

            // otherwise return false
            return false;
        }

        // Check to see if a new triangle which shares this edge has not
        // been seen before. Takes the triangles normal and center, and a threshold
        // within which we consider the same normal
        public bool AddTriNormalCenterIfUnique(Vector3 normal, Vector3 center, float threshold)
        {
            // Loop through all the triangle normals we know of for this edge
            for (int i=0; i<triNormals.Count; i++)
            {
                // Calculate the distance between this new triangle and the
                // existing triangle - if the distance (magnitude) is less than
                // the threshold - we'll assume we've seen this triangle before
                if ((triNormals[i] - normal).magnitude < threshold)
                    // return false
                    return false;
            }

            // Otherwise, if the normal doesn't match anything we've seen before
            // Add it to the list of normals and face centers
            triNormals.Add(normal);
            triCenters.Add(center);

            // return true
            return true;
        }
    };

    // Start is called before the first frame update
    void Start()
    {
        // Find our Occlusion Frustum Culling script
        OcclusionFrustumCulling occlusionFrustumCulling = FindObjectOfType<OcclusionFrustumCulling>();

        // If our frustum renderer hasn't been set, create a new GameObject for it
        if (frustumRenderer == null) frustumRenderer = new GameObject("FrustumRenderer");
        // Set this frustums position and rotation to identity
        frustumRenderer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        // Add a mesh renderer to our frustrum renderer GameObject
        MeshRenderer mr = frustumRenderer.AddComponent<MeshRenderer>();
        // Set it's material to the material defined in our Occlusion Frustum Culling script
        mr.material = occlusionFrustumCulling.frustumMaterial;

        // Add a mesh filer to our frustrum renderer GameObject
        frustumMeshFilter = frustumRenderer.AddComponent<MeshFilter>();
        // Set it's mesh to a new empty mesh
        frustumMeshFilter.mesh = new Mesh();

        // If we haven't set our occluding objects mesh filter, grab the
        // mesh filter attached to the game object this script is attached to
        if (occludingObjectMeshFilter == null)
            occludingObjectMeshFilter = GetComponent<MeshFilter>();

        // Get the occluding objects mesh filter
        Mesh m = occludingObjectMeshFilter.mesh;

        // Get the vertices of this mesh
        Vector3[] v = m.vertices;

        // Calculate the centre of mass of the mesh by averaging the
        // position of all the vertices in the mesh
        for (int i=0; i<v.Length; i++)
            centerOfMass += v[i];
        centerOfMass /= v.Length;

        // Set our thresholds for what is a unique vertex and unique normal
        float uniqueVertexDistanceThreshold = 0.0001f;
        float uniqueNormalDistanceThreshold = 0.0001f;

        // Create a list of all edges in our occlusion object
        List<SharedEdge> allEdges = new List<SharedEdge>();

        // Get the array of all triangle indices in our mesh
        int[] t = m.triangles;

        // Loop through each triplet of triangle indices
        for (int i=0; i<t.Length; i+=3)
        {
            // Get the three vertices which make up this triangle
            Vector3 v1 = v[t[i]];
            Vector3 v2 = v[t[i+1]];
            Vector3 v3 = v[t[i+2]];

            // Calculate the centre of these three vertices
            Vector3 c = (v1 + v2 + v3) / 3;

            // Calculate the normal of this triangle
            Vector3 n = Vector3.Cross(v2 - v1, v3 - v1);

            // Set up booleans for the three edges of this triangle
            bool foundv1v2 = false;
            bool foundv2v3 = false;
            bool foundv3v1 = false;

            // Loop through all the edges we know of so for
            for (int j=0; j< allEdges.Count; j++)
            {
                // If we haven't found an edge which matches v1v2 yet
                if (!foundv1v2)
                {
                    // Check to see if this known edge matches it
                    if (allEdges[j].Compare(v1, v2, uniqueVertexDistanceThreshold))
                    {
                        // If so, add the triangles normal and center if we haven't
                        // already seen this triangle sharing this edge before
                        allEdges[j].AddTriNormalCenterIfUnique(n, c, uniqueNormalDistanceThreshold);
                        // update found v1v2 to be true
                        foundv1v2 = true;
                    }
                }

                // If we haven't found an edge which matches v2v3 yet
                if (!foundv2v3)
                {
                    // Check to see if this known edge matches it
                    if (allEdges[j].Compare(v2, v3, uniqueVertexDistanceThreshold))
                    {
                        // If so, add the triangles normal and center if we haven't
                        // already seen this triangle sharing this edge before
                        allEdges[j].AddTriNormalCenterIfUnique(n, c, uniqueNormalDistanceThreshold);
                        // update found v2v3 to be true
                        foundv2v3 = true;
                    }
                }

                // If we haven't found an edge which matches v3v1 yet
                if (!foundv3v1)
                {
                    // Check to see if this known edge matches it
                    if (allEdges[j].Compare(v3, v1, uniqueVertexDistanceThreshold))
                    {
                        // If so, add the triangles normal and center if we haven't
                        // already seen this triangle sharing this edge before
                        allEdges[j].AddTriNormalCenterIfUnique(n, c, uniqueNormalDistanceThreshold);
                        // update found v3v1 to be true
                        foundv3v1 = true;
                    }
                }

                // If we've found all the edges of this triangle, break out of the loop
                if (foundv1v2 && foundv2v3 && foundv3v1) break;
            }

            // If we didn't find a match for the edge v1v2
            if (!foundv1v2)
                // Create a new edge for it
                allEdges.Add(new SharedEdge(v1, v2, n, c));

            // If we didn't find a match for the edge v2v3
            if (!foundv2v3)
                // Create a new edge for it
                allEdges.Add(new SharedEdge(v2, v3, n, c));

            // If we didn't find a match for the edge v3v1
            if (!foundv3v1)
                // Create a new edge for it
                allEdges.Add(new SharedEdge(v3, v1, n, c));
        }

        // Create a new list of shared edges for the outer edges
        outerEdges = new List<SharedEdge>();

        // Loop through all the edges we've seen
        for (int i = 0; i < allEdges.Count; i++)
        {
            // If at least two triangles share this edge
            // (i.e. we have two triangle normals stored)
            if (allEdges[i].triNormals.Count > 1)
                // add it as an "outer edge"
                outerEdges.Add(allEdges[i]);
        }

    }

    // Calculate the transformed frustum centers and normals for our
    // occluding object at this frame, relevant to the camera position
    public void CalcFrustum(Camera visibilityTestingCamera, ref List<Vector3> transformedFrustumCenters, ref List<Vector3> transformedFrustumNormals)
    {
        // A list for the occluding object's frustums vertices and triangles
        List<Vector3> frustumVertices = new List<Vector3>();
        List<int> frustumTriangles = new List<int>();

        // The current camera position
        Vector3 cameraPos = visibilityTestingCamera.transform.position;

        // Loop through each of our potential outer edges
        foreach (SharedEdge edge in outerEdges)
        {
            // Get the first two triangles which share the edge, and transform
            // their centers and normals from model space to world space
            Vector3 c1 = occludingObjectMeshFilter.transform.TransformPoint(edge.triCenters[0]);
            Vector3 c2 = occludingObjectMeshFilter.transform.TransformPoint(edge.triCenters[1]);
            Vector3 n1 = occludingObjectMeshFilter.transform.TransformDirection(edge.triNormals[0]);
            Vector3 n2 = occludingObjectMeshFilter.transform.TransformDirection(edge.triNormals[1]);

            // Calculate the dot products of the vector between the camera
            // and the two triangle centers and the triangle normals
            // These dot products will be negative if they face the camera
            // and positive or zero if they're facing away from the camera
            float cn1 = Vector3.Dot(c1 - cameraPos, n1);
            float cn2 = Vector3.Dot(c2 - cameraPos, n2);

            // If one triangle of an edge is facing the camera, and one is
            // facing away from the camera, then this will be a silhouette edge
            // (we can see a triangle on one side of the edge, but not the other)
            // it doesn't matter which triangle faces towards and which is away
            if ((cn1 < 0 && cn2 >= 0) || (cn2 <0 && cn1 >=0) )
            {
                // Transform the vertices of the edge from model space to world
                // Space
                Vector3 v1 = occludingObjectMeshFilter.transform.TransformPoint(edge.v1);
                Vector3 v2 = occludingObjectMeshFilter.transform.TransformPoint(edge.v2);

                // Create a frustum plane using these two vertices, updating
                // the frustumVertices, frustumTriangles, transformedFrustumCenters
                // and transformedFrustumNormals as we do
                CreateFrustumPlane(v1, v2, visibilityTestingCamera, ref frustumVertices, ref frustumTriangles, ref transformedFrustumCenters, ref transformedFrustumNormals);

                // Calculate the center of the edge
                Vector3 vC = (v1 + v2) / 2;

                // Draw a line between the two vertices
                Debug.DrawLine(v1, v2, Color.black);
                // Draw two normals from the center of the edge pointing
                // the direction of the two triangle normals
                Debug.DrawRay(vC, n1, Color.red);
                Debug.DrawRay(vC, n2, Color.blue);
            }
        }

        // Destroy the existing mesh filter mesh
        Destroy(frustumMeshFilter.mesh);

        // Create a new mesh and populate it with our occluding object's
        // frustum's vertices and triangles
        Mesh mesh = new Mesh();
        mesh.vertices = frustumVertices.ToArray();
        mesh.triangles = frustumTriangles.ToArray();

        // set our frustums mesh filter to use this new mesh
        frustumMeshFilter.mesh = mesh;

    }

    // Create a frustum plane using two points and a camera, and update
    // the frustumVertices, frustumTriangles, transformedFrustumCenters
    // and transformedFrustumNormals as well
    void CreateFrustumPlane(Vector3 v1, Vector3 v2, Camera c, ref List<Vector3> frustumVertices, ref List<int> frustumTriangles, ref List<Vector3> transformedFrustumCenters, ref List<Vector3> transformedFrustumNormals)
    {
        // Calculate the position of the vertices are the far plane
        // by calculating a vector from the camera to the near plane vertices
        // and multiplying it's length by the camera's far clip plane
        Vector3 v3 = v2 + ((v2 - c.transform.position).normalized) * c.farClipPlane;
        Vector3 v4 = v1 + ((v1 - c.transform.position).normalized) * c.farClipPlane;

        // Get the starting vertex index for our new frustum plane
        int triStartIdx = frustumVertices.Count;

        // Add the four vertices for this plane to our list of frustum vertices
        frustumVertices.Add(v1);
        frustumVertices.Add(v2);
        frustumVertices.Add(v3);
        frustumVertices.Add(v4);

        // Add the two triangles which make up this plane, starting at
        // triStartIdx
        frustumTriangles.Add(triStartIdx);
        frustumTriangles.Add(triStartIdx + 1);
        frustumTriangles.Add(triStartIdx + 2);
        frustumTriangles.Add(triStartIdx);
        frustumTriangles.Add(triStartIdx + 2);
        frustumTriangles.Add(triStartIdx + 3);

        // Calculate the center of the plane as the average of the four vertices
        Vector3 planeCenter = (v1 + v2 + v3 + v4) / 4;
        // Calculate the normal of the plane as the cross product of two
        // of the edges of the plane
        Vector3 planeNormal = Vector3.Cross(v4 - v1, v2 - v1).normalized;

        // Transform the occluding objects center of mass from model space to
        // world space
        Vector3 transformedCOM = occludingObjectMeshFilter.transform.TransformPoint(centerOfMass);

        // Check to make sure that our planes normal is pointing outside of the
        // frustum rather than inside.
        // Calculate the dot product of the vector from the centre of the plane
        // to the center of the mesh and the planes normal - if it is negative 
        // (i.e. they are pointing opposite directions), reverse the direction
        // of the normal.
        if (Vector3.Dot((planeCenter - transformedCOM).normalized, planeNormal) < 0) planeNormal = -planeNormal;

        // Transform the frustum plane centre and normal from model space to
        // World space, and then add these to the transformed frustum centers
        // and normals lists.
        transformedFrustumCenters.Add(frustumRenderer.transform.TransformPoint(planeCenter));
        transformedFrustumNormals.Add(frustumRenderer.transform.TransformDirection(planeNormal));
    }
}
