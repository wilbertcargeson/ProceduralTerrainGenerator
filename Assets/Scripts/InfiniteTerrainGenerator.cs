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


    void Start()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        Debug.Log(terrainGenerator);
        terrainSize = TerrainGenerator.terrainSize;
        visibleTerrainDistance = Mathf.RoundToInt(maxViewPov / terrainSize);
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

        // Loop through all direction from negative to positive visible terrain distance to optain coordinates
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

                    terrainDictionary.Add(viewedTerrainCoor, new TerrainObject(viewedTerrainCoor, terrainSize, this.transform, terrainMaterial, meshLOD));
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

        MeshFilter terrainMeshFilter;
        public TerrainObject(Vector2 coordinate, int size, Transform parentTransform, Material material, int meshLOD)
        {
            position = coordinate * size;
            Debug.Log(position);
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionVector = new Vector3(position.x, 0, position.y);

            // Generate terrain
            terrainObj = new GameObject("Terrain plane");
            terrainRenderer = terrainObj.AddComponent<MeshRenderer>();
            terrainMeshFilter = terrainObj.AddComponent<MeshFilter>();
            terrainRenderer.material = material;

            // terrainObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrainObj.transform.position = positionVector;
            terrainObj.transform.parent = parentTransform;
            terrainMeshFilter.mesh = new Mesh();
            SetVisible(false);

            terrainGenerator.RequestTerrainMeshData(OnMeshReceived, position, meshLOD);
        }

        void OnMeshReceived(TerrainGenerator.TerrainMeshData terrainData)
        {
            terrainMeshFilter.mesh.vertices = terrainData.vertices;
            terrainMeshFilter.mesh.colors = terrainData.colors;
            terrainMeshFilter.mesh.triangles = terrainData.triangles;
            terrainMeshFilter.mesh.RecalculateNormals();
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
