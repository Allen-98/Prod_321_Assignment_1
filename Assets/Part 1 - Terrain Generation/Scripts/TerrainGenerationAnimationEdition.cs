using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerationAnimationEdition : MonoBehaviour
{

    [Header("Terrain Generation")]

    public Texture2D heightMapTexture;

    public float heightScale = 30;

    [System.Serializable]
    public struct ColourMap
    {

        public float minValue;

        public Color colour;

        public ColourMap(float _minValue, Color _colour)
        {
            minValue = _minValue; colour = _colour;
        }
    }

    public List<ColourMap> colourMaps = new List<ColourMap>()
    {
        new ColourMap(0.0f, Color.blue),
        new ColourMap(0.1f, Color.yellow),
        new ColourMap(0.2f, Color.green),
        new ColourMap(0.6f, Color.gray),
        new ColourMap(0.9f, Color.white)
    };
    Color getMappedColour(float height)
    {

        for (int i = colourMaps.Count - 1; i >= 0; i--)
            if (!seasonIsChanged)
            {
                if (height > colourMaps[i].minValue)
                    return colourMaps[i].colour;
            }
            else
            {
                if (height > colourMaps[i].minValue)
                    if (height > 0.18 && height < 0.5)
                    {
                        return Color.yellow;
                    }
                    else
                    {
                        return colourMaps[i].colour;
                    }

            }
        return Color.white;
    }


    [Header("Object Placement")]

    public bool HaveObjectsToPut;
    public Texture2D objectMap;
    public GameObject[] objectToPut;

    [Header("Terrain Animation")]

    public bool DoAnimation;
    //first wave
    public float aniHeightScale1 = 3;
    public float aniPeriodScale1 = 4;
    public float aniTimeScale1 = 2;

    //second wave
    public float aniHeightScale2 = 2;
    public float aniPeriodScale2 = 2;
    public float aniTimeScale2 = 2;


    // other things
    MeshFilter meshFilter;
    bool seasonIsChanged;

    // Start is called before the first frame update
    void Start()
    {
        seasonIsChanged = false;

        List<Vector3> vertices = new List<Vector3>();

        List<int> triangles = new List<int>();

        List<Color> vertexColours = new List<Color>();

        List<Vector3> objectPosR = new List<Vector3>();

        List<Vector3> objectPosB = new List<Vector3>();


        int height = heightMapTexture.height;
        int width = heightMapTexture.width;



        // Generate our Vertices
        // Loop through the meshes length and width
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {

                float yVal = heightMapTexture.GetPixel(x, z).r;
                vertices.Add(new Vector3(x, yVal * heightScale, z));

                // check the object placement

                if (HaveObjectsToPut)
                {
                    if (objectMap.GetPixel(x, z).r > 0.5)
                    {
                        objectPosR.Add(new Vector3(x, yVal * heightScale, z));

                    }

                    if (objectMap.GetPixel(x, z).b > 0.5)
                    {
                        objectPosB.Add(new Vector3(x, yVal * heightScale, z));
                    }
                }

                vertexColours.Add(getMappedColour(yVal));
            }
        }

        // Generate our triangle Indicies
        // Loop through the meshes length-1 and width-1
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {

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
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Create a new renderer for our mesh, and use our custom coloured vertex
        // Surface Shader
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Custom/ColouredVertexSurfaceShader"));

        if (HaveObjectsToPut)
        {

            for (int i = 0; i <= objectPosR.Count - 1; i++)
            {
                Instantiate(objectToPut[0], objectPosR[i], Quaternion.identity);
            }

            for (int i = 0; i <= objectPosB.Count - 1; i++)
            {
                Instantiate(objectToPut[1], objectPosB[i], Quaternion.identity);
            }
        }

    }

    private void Update()
    {
        int height = heightMapTexture.height;
        int width = heightMapTexture.width;

        List<Vector3> aniVertices = new List<Vector3>();
       // List<Vector3> staticVertices = new List<Vector3>();

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {

                float yVal = heightMapTexture.GetPixel(x, z).r;


                if (yVal == 0)
                {
                    float xNorm = (float)x / (float)width;

                    float y1 = (Mathf.Sin(xNorm * 2 * Mathf.PI * aniPeriodScale1 + Time.timeSinceLevelLoad * aniTimeScale1) * aniHeightScale1);

                    float y2 = (Mathf.Sin(xNorm * 2 * Mathf.PI * aniPeriodScale2 + Time.timeSinceLevelLoad * aniTimeScale2) * aniHeightScale2);

                    float y = (y1 + y2);

                    aniVertices.Add(new Vector3(x, y, z));

                }
                else
                {
                    aniVertices.Add(new Vector3(x, yVal * heightScale, z));
                }



            }
        }

        meshFilter.mesh.vertices = aniVertices.ToArray();

        meshFilter.mesh.RecalculateNormals();

    }


    public void SeasonChange()
    {
        if (seasonIsChanged)
        {
            seasonIsChanged = false;
        }
        else
        {
            seasonIsChanged = true;
        }

        int height = heightMapTexture.height;
        int width = heightMapTexture.width;
        List<Color> vertexColours = new List<Color>();


        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {

                float yVal = heightMapTexture.GetPixel(x, z).r;

                vertexColours.Add(getMappedColour(yVal));
            }
        }


        meshFilter.mesh.colors = vertexColours.ToArray();

    }

}
