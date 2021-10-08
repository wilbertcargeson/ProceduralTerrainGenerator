using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{

    public static float maxViewPov = 600;

    public GameObject player;

    public static Vector2 playerPosition;

    int terrainSize;
    int visibleTerrainDistance;

    public Material terrainMaterial;

    Dictionary<Vector2, TerrainObject> terrainDictionary = new Dictionary<Vector2, TerrainObject>();

    List<TerrainObject> terrainsVisible = new List<TerrainObject>();

    static TerrainGenerator terrainGenerator;

    public GameObject treePrefab;

    System.Random pseudoRandom;



    void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        Debug.Log(terrainGenerator);
        terrainSize = TerrainGenerator.terrainSize;
        visibleTerrainDistance = Mathf.RoundToInt(maxViewPov / terrainSize);
        pseudoRandom = new System.Random(TerrainGenerator.seed + 1);
        UpdateVissibleTerrain();

    }

    void Update()
    {
        playerPosition = new Vector2(player.transform.position.x, player.transform.position.z);
        UpdateVissibleTerrain();
    }

    void UpdateVissibleTerrain()
    {


        for (int i = 0; i < terrainsVisible.Count; i++)
        {
            terrainsVisible[i].SetVisible(false);
        }
        terrainsVisible.Clear();

        int currX = Mathf.RoundToInt(playerPosition.x / terrainSize);
        int currZ = Mathf.RoundToInt(playerPosition.y / terrainSize);

        // Loop through all direction from negative to positive visible terrain distance to obtain coordinates
        for (int zOffset = -visibleTerrainDistance; zOffset <= visibleTerrainDistance; zOffset++)
        {
            for (int xOffset = -visibleTerrainDistance; xOffset <= visibleTerrainDistance; xOffset++)
            {
                Vector2 viewedTerrainCoor = new Vector2(currX + xOffset, currZ + zOffset);

                if (terrainDictionary.ContainsKey(viewedTerrainCoor))
                {
                    terrainDictionary[viewedTerrainCoor].UpdateTerrainObject();
                    if (terrainDictionary[viewedTerrainCoor].IsVisible())
                    {
                        terrainsVisible.Add(terrainDictionary[viewedTerrainCoor]);
                    }
                }
                else
                {
                    // Determining the level of detail in each mesh, the further it is, 
                    // the lower the level of detail ( in this case: higher number )
                    int meshLOD = 1;

                    terrainDictionary.Add(viewedTerrainCoor, new TerrainObject(viewedTerrainCoor, terrainSize, this.transform, terrainMaterial, meshLOD, treePrefab, pseudoRandom));
                }
            }
        }
    }



    public class TerrainObject
    {
        GameObject terrainObj;
        Vector2 position;
        Bounds bounds;

        MeshRenderer terrainRenderer;
        MeshCollider terrainMeshCollider;

        MeshFilter terrainMeshFilter;

        System.Random pseudoRandom;

        GameObject treePrefab;

        Vector3 positionVector;


        public TerrainObject(Vector2 coordinate, int size, Transform parentTransform, Material material, int meshLOD, GameObject treePrefab, System.Random random)
        {
            this.treePrefab = treePrefab;
            pseudoRandom = random;
            position = coordinate * size;
            Debug.Log(position);
            bounds = new Bounds(position, Vector2.one * size);
            positionVector = new Vector3(position.x, 0, position.y);

            // Generate terrain
            terrainObj = new GameObject("Terrain plane");
            terrainRenderer = terrainObj.AddComponent<MeshRenderer>();
            terrainMeshFilter = terrainObj.AddComponent<MeshFilter>();
            terrainRenderer.material = material;
            terrainMeshCollider = terrainObj.AddComponent<MeshCollider>();

            // terrainObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrainObj.transform.position = positionVector;
            terrainObj.transform.parent = parentTransform;
            SetVisible(false);

            terrainGenerator.RequestTerrainMeshData(OnMeshReceived, position, meshLOD);

        }

        // Function to call for threading
        void OnMeshReceived(TerrainGenerator.TerrainMeshData terrainData)
        {
            Mesh newMesh = new Mesh();
            newMesh.vertices = terrainData.vertices;
            newMesh.colors = terrainData.colors;
            newMesh.triangles = terrainData.triangles;
            newMesh.uv = terrainData.uvs;
            newMesh.RecalculateNormals();
            terrainMeshFilter.mesh = newMesh;
            terrainMeshCollider.sharedMesh = newMesh;
            spawnTrees(terrainData.vertices);
        }

        void spawnTrees(Vector3[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                float heightPercentile = Mathf.InverseLerp(0, TerrainGenerator.estimatedMaxHeight * TerrainGenerator.noiseWeightOnHeight, vertices[i].y);
                if (heightPercentile < 0.6 && heightPercentile > 0.10 && pseudoRandom.Next(1, 100) < 5)
                {
                    Vector3 placedTreeLocation = new Vector3(vertices[i].x, vertices[i].y + 0.1f, vertices[i].z) + positionVector;
                    Instantiate(treePrefab, placedTreeLocation, Quaternion.identity, terrainObj.transform);
                }
            }
        }

        public void UpdateTerrainObject()
        {
            float viewerDistanceNrstFromEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            bool visible = viewerDistanceNrstFromEdge <= maxViewPov;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            terrainObj.SetActive(visible);
        }

        public bool IsVisible()
        {
            return terrainObj.activeSelf;
        }
    }
}
