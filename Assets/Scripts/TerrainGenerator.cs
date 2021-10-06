using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{



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

    Queue<TerrainMeshDataInfo<TerrainMeshData>> terrainInfoQueue = new Queue<TerrainMeshDataInfo<TerrainMeshData>>();

    // void Update()
    // {
    //     // Spawn a single terrain, only for editing
    //     Mesh mesh = new Mesh();
    //     GetComponent<MeshFilter>().mesh = mesh;
    //     TerrainMeshData data = CreateMeshData(Vector2.zero);
    //     mesh.vertices = data.vertices;
    //     mesh.colors = data.colors;
    //     mesh.triangles = data.triangles;
    //     mesh.RecalculateNormals();
    // }
    void Update()
    {
        if (terrainInfoQueue.Count > 0)
        {
            for (int i = 0; i < terrainInfoQueue.Count; i++)
            {
                TerrainMeshDataInfo<TerrainMeshData> terrainInfo = terrainInfoQueue.Dequeue();
                terrainInfo.callback(terrainInfo.parameter);
            }
        }
    }

    public void RequestTerrainMeshData(System.Action<TerrainMeshData> callback, Vector2 threadOffSet, int meshLOD)
    {
        System.Threading.ThreadStart threadStart = delegate
        {
            TerrainMeshDataThread(callback, threadOffSet, meshLOD);
        };

        new System.Threading.Thread(threadStart).Start();
    }

    void TerrainMeshDataThread(System.Action<TerrainMeshData> callback, Vector2 threadOffSet, int meshLOD)
    {
        levelOfDetail = meshLOD;
        TerrainMeshData data = CreateMeshData(threadOffSet);
        lock (terrainInfoQueue)
        { // Avoid race condition
            terrainInfoQueue.Enqueue(new TerrainMeshDataInfo<TerrainMeshData>(callback, data));
        }
    }


    TerrainMeshData CreateMeshData(Vector2 threadOffset)
    {
        Vector3[] vertices;
        int[] triangles;
        Color[] colors;

        vertices = new Vector3[((terrainSize + 1) * (terrainSize + 1))];
        colors = new Color[((terrainSize + 1) * (terrainSize + 1))];
        System.Random pseudoRandom = new System.Random(seed);

        octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float octaveOffSetX = pseudoRandom.Next(-100000, 100000) + offsetx + threadOffset.x;
            float octaveOffSetY = pseudoRandom.Next(-100000, 100000) - offsety - threadOffset.y;
            octaveOffsets[i] = new Vector2(octaveOffSetX, octaveOffSetY);
        }

        int simplificationInc = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (terrainSize - 1) / simplificationInc + 1;


        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        float halfterrainSize = terrainSize / 2f;


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
                    float x1 = (float)(x - halfterrainSize + octaveOffsets[i].x) / scale * frequency;
                    float z1 = (float)(z - halfterrainSize + octaveOffsets[i].y) / scale * frequency;
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

        return new TerrainMeshData(vertices, triangles, colors);
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

    // Struct for storing necessary information to create a mesh
    public struct TerrainMeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Color[] colors;

        public TerrainMeshData(Vector3[] vertices, int[] triangles, Color[] colors)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.colors = colors;
        }
    }

    // Struct for threading
    public struct TerrainMeshDataInfo<T>
    {
        public readonly System.Action<T> callback;
        public readonly T parameter;

        public TerrainMeshDataInfo(System.Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }


}
