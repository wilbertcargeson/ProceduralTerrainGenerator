using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{


    // Terrain size for each object, note that maximum vertices is 256*256, hence maximum is 256 if square
    public static int terrainSize = 256;

    // Custom offsets for the noise
    public float offsetx = 1;

    public float offsety = 1;

    // How zoomed in the terrain is
    public float scale = 65;

    // Seed for randomizer to calculate overall noise offset
    public static int seed = 1;

    [Range(0, 10)]
    public int octaves = 4;

    [Range(0, 1)]
    public float persistence = 0.2f;


    [Range(0, 100)]
    public float lacunarity = 5;

    // The magnitude in which the noise will affect the terrain, affects mountain height generally
    public static float noiseWeightOnHeight = 120;


    public Gradient gradient;

    public float movementSpeed = 5;

    public AnimationCurve meshHeightCurve;

    // The higher, less triangles will be used, this is only used in editing phase
    [Range(0, 6)]
    public int levelOfDetail;

    public static float estimatedMaxHeight = 1.75f;
    Mesh mesh;


    Queue<TerrainMeshDataInfo<TerrainMeshData>> terrainInfoQueue = new Queue<TerrainMeshDataInfo<TerrainMeshData>>();

    // void Update()
    // {
    //     // Spawn a single terrain, only for editing
    //     if (mesh) mesh.Clear();
    //     mesh = new Mesh();
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
        Vector2[] octaveOffsets;
        Vector3[] vertices;
        int[] triangles;
        Color[] colors;
        Vector2[] uvs;

        vertices = new Vector3[((terrainSize + 1) * (terrainSize + 1))];
        colors = new Color[vertices.Length];
        triangles = new int[6 * vertices.Length];
        uvs = new Vector2[vertices.Length];

        System.Random pseudoRandom = new System.Random(seed);

        octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float octaveOffSetX = pseudoRandom.Next(-100000, 100000) + offsetx + threadOffset.x;
            float octaveOffSetY = pseudoRandom.Next(-100000, 100000) + offsety + threadOffset.y;
            octaveOffsets[i] = new Vector2(octaveOffSetX, octaveOffSetY);
        }

        int simplificationInc = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (terrainSize) / simplificationInc;


        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        float halfterrainSize = terrainSize / 2f;


        int triangleIndex = 0;
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
                    float perlin = Mathf.PerlinNoise(x1, z1) * 2f;

                    noiseHeight += perlin * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // Update max and min height value
                if (noiseHeight > maxHeight) { maxHeight = noiseHeight; }
                else if (noiseHeight < minHeight) { minHeight = noiseHeight; }

                vertices[index] = new Vector3(x, noiseHeight, z);

                if (x < terrainSize && z < terrainSize)
                {// Generate triangles for mesh
                    triangles[triangleIndex + 0] = index + 0;
                    triangles[triangleIndex + 1] = index + verticesPerLine + 1;
                    triangles[triangleIndex + 2] = index + 1;
                    triangles[triangleIndex + 3] = index + 1;
                    triangles[triangleIndex + 4] = index + verticesPerLine + 1;
                    triangles[triangleIndex + 5] = index + verticesPerLine + 2;

                    triangleIndex += 6;
                }
                uvs[index] = new Vector2((float)x / terrainSize, (float)z / terrainSize);
                index++;
            }
        }


        // Evaluate height and colors of vertices
        for (int i = 0; i < vertices.Length; i++)
        {



            // Ensure height is within range
            float evaluateHeight = Mathf.Clamp(vertices[i].y, 0.1f, estimatedMaxHeight);
            float lerpedHeight = Mathf.InverseLerp(0, estimatedMaxHeight + 0.1f, evaluateHeight);

            // Sets a curve for smooth terrain transition
            float curvedHeight = meshHeightCurve.Evaluate(lerpedHeight);

            colors[i] = gradient.Evaluate(curvedHeight);

            if (curvedHeight > 1 || curvedHeight < 0) Debug.Log(curvedHeight);

            vertices[i].y = curvedHeight * noiseWeightOnHeight;
        }



        return new TerrainMeshData(vertices, triangles, colors, uvs);
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

        public Vector2[] uvs;

        public TerrainMeshData(Vector3[] vertices, int[] triangles, Color[] colors, Vector2[] uvs)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.colors = colors;
            this.uvs = uvs;
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
