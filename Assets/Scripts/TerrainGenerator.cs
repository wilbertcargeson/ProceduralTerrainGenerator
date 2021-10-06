using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    Vector2[] uv;

    Color[] colors;

    public static int terrainSize = 256;

    public float offsetx = 1;

    public float offsety = 1;

    public float scale = 65;

    public int seed = 1;

    [Range(0, 10)]
    public int octaves = 4;

    [Range(0, 1)]
    public float persistence = 0.2f;

    [Range(0, 100)]
    public float lacunarity = 5;

    public float depth = 20;

    Vector2[] octaveOffsets;

    public Gradient gradient;

    public float movementSpeed = 5;

    public AnimationCurve meshHeightCurve;

    [Range(0, 6)]
    public int levelOfDetail; // The higher, less triangles will be used

    void Start()
    {

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }



    void CreateShape()
    {
        vertices = new Vector3[((terrainSize + 1) * (terrainSize + 1))];
        colors = new Color[((terrainSize + 1) * (terrainSize + 1))];
        System.Random pseudoRandom = new System.Random(seed);
        octaveOffsets = new Vector2[octaves];

        int simplificationInc = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (terrainSize - 1) / simplificationInc + 1;


        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        float halfterrainSize = terrainSize / 2f;

        for (int i = 0; i < octaves; i++)
        {
            float offSetX = pseudoRandom.Next(-100000, 100000) + offsetx;
            float offSetY = pseudoRandom.Next(-100000, 100000) + offsety;
            octaveOffsets[i] = new Vector2(offSetX, offSetY);
        }

        // Getting height of each vertices using perlin noise
        for (int z = 0, index = 0; z <= terrainSize; z += simplificationInc)
        {
            for (int x = 0; x <= terrainSize; x += simplificationInc)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float x1 = (float)x / scale * frequency + octaveOffsets[i].x;
                    float z1 = (float)z / scale * frequency + octaveOffsets[i].y;
                    float perlin = Mathf.PerlinNoise(x1, z1);

                    noiseHeight += perlin * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // Update max and min height value
                if (noiseHeight > maxHeight) { maxHeight = noiseHeight; }
                else if (noiseHeight < minHeight) { minHeight = noiseHeight; }

                vertices[index] = new Vector3(x, noiseHeight, z);
                index++;
            }
        }

        // Evaluate height and colors of vertices
        for (int i = 0; i < vertices.Length; i++)
        {

            float lerpedHeight = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
            colors[i] = gradient.Evaluate(lerpedHeight);

            // Sets a curve for ensuring water is not curvy
            float curvedHeight = meshHeightCurve.Evaluate(lerpedHeight);
            vertices[i].y = curvedHeight * depth;
        }

        // Generate triangles for mesh
        int vert = 0;
        int tris = 0;
        triangles = new int[6 * terrainSize * terrainSize];

        for (int z = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + verticesPerLine + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + verticesPerLine + 1;
                triangles[tris + 5] = vert + verticesPerLine + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    void OnValidate()
    {
        if (terrainSize < 0)
        {
            terrainSize = 1;
        }

        if (scale <= 0)
        {
            scale = 1;
        }
    }


}
