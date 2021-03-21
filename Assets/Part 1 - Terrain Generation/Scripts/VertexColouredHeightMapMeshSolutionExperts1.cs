using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class generates a mesh using a height map texture. The texture defines
 * the size of the mesh, and also the height of each vertex in the mesh. We also
 * define vertex colours for the mesh based on the height map, to colour the mesh
 *
 * This shows the extra for experts solution 1 which, in addition to the solution,
 * makes the vertex colours a Unity Inspector property we can edit outside of
 * the code.
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

public class VertexColouredHeightMapMeshSolutionExperts1 : MonoBehaviour
{
    // Defines the height map texture used to create the mesh and set the heights
    public Texture2D heightMapTexture;

    // Defines the height scale that we multiply the height of each vertex by
    public float heightScale = 30;

    // Create a new public structure to store our colour map values
    // This will take a "minValue" which is the minimum height, as well
    // as the colour to apply if the vertex is above this height
    [System.Serializable]
    public struct ColourMap
    {
        // Store the min height value
        public float minValue;

        // Store the colour
        public Color colour;

        // A new constructor which takes a minvalue and colour
        public ColourMap(float _minValue, Color _colour)
        {
            // and assigns it to the struct variables
            minValue = _minValue; colour = _colour;
        }
    }

    // Default colour map values
    public List<ColourMap> colourMaps = new List<ColourMap>()
    {
        // yVal > 0, set blue
        new ColourMap(0.0f, Color.blue),
        // yVal > 0.1, set yellow
        new ColourMap(0.1f, Color.yellow),
        // yVal > 0.2, set green
        new ColourMap(0.2f, Color.green),
        // yVal > 0.6, set gray
        new ColourMap(0.6f, Color.gray),
        // yVal > 0.9, set white
        new ColourMap(0.9f, Color.white)
    };

    // This function will return the mapped colour based on a height value
    Color getMappedColour(float height)
    {
        // Loop through all the colour maps from last to first
        for (int i = colourMaps.Count - 1; i >= 0; i--)
            // If the height value is greater than the min height value
            // for this colour map
            if (height > colourMaps[i].minValue)
                // return the colour
                return colourMaps[i].colour;

        // Something has gone wrong, none of the colours have matched,
        // just return white
        return Color.white;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create a list to store our vertices
        List<Vector3> vertices = new List<Vector3>();

        // Create a list to store our triangles
        List<int> triangles = new List<int>();

        // Create a list to store our vertex colours
        List<Color> vertexColours = new List<Color>();

        // Calculate the Height and Width of our mesh from the heightmap's
        // height and width 
        int height = heightMapTexture.height;
        int width = heightMapTexture.width;

        // Generate our Vertices
        // Loop through the meshes length and width
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create a new vertex using the x and z positions. Get the
                // y position as the pixel from the height map texture and
                // store in in yVal for use for vertex position and colouring
                // As the height map is in gray scale, we can use any colour
                // channel, in this case red.

                // Multiply the pixel value by the height scale to get the final
                // y value
                float yVal = heightMapTexture.GetPixel(x, z).r;
                vertices.Add(new Vector3(x, yVal * heightScale, z));

                // Add the vertex colour using the getMappedColour function for
                // the yVal at this pixel
                vertexColours.Add(getMappedColour(yVal));
            }
        }

        // Generate our triangle Indicies
        // Loop through the meshes length-1 and width-1
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // Multiply the Z value by the mesh width to get the number
                // of pixels in the rows, then add the value of x to get the
                // final index. Increase the values of X and Z accordingly
                // to get the neighbouring indicies
                int vTL = z * width + x;
                int vTR = z * width + x + 1;
                int vBR = (z + 1) * width + x + 1;
                int vBL = (z + 1) * width + x;

                // Create the two triangles which make each element in the quad
                // Triangle Top Left->Bottom Left->Bottom Right
                triangles.Add(vTL);
                triangles.Add(vBL);
                triangles.Add(vBR);

                // Triangle Top Left->Bottom Right->Top Right
                triangles.Add(vTL);
                triangles.Add(vBR);
                triangles.Add(vTR);
            }
        }

        // Create our mesh object
        Mesh mesh = new Mesh();

        // Assign the vertices, triangle indicies and vertex colours to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = vertexColours.ToArray();

        // Use recalculate normals to calculate the vertex normals for our mesh
        mesh.RecalculateNormals();

        // Create a new mesh filter, and assign the mesh from before to it
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Create a new renderer for our mesh, and use our custom coloured vertex
        // Surface Shader
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Custom/ColouredVertexSurfaceShader"));

    }
}
