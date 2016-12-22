using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour {
    /* what the terrain asset should be like (in case it gets changed)
     * 
     * relatively low Terrain Width and Length, depending on arena size
     * low Terrain Height, single digit
     * 
     * Heightmap Resolution impacts performance and the way it looks
     * preferably around 65, 129, 257?
     */


    private Terrain terrain;
    private TerrainData data;
    private int width, height;
    float[,] heights;

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;

        width = data.heightmapWidth;
        height = data.heightmapHeight;
        heights = data.GetHeights(0, 0, width, height);
    }

    void Update()
    {
        // temporary
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                heights[i, j] = Mathf.Sin(Time.time + j/12f)/2 + 0.5f;
            }
        }

        ReloadTerrain();
    }

    void ReloadTerrain()
    {
        data.SetHeights(0, 0, heights);
    }

    void OnApplicationQuit()
    { // inside unity editor, reset terrain
        int width = data.heightmapWidth;
        int height = data.heightmapHeight;
        float[,] heights = data.GetHeights(0, 0, width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                heights[i, j] = 0;
            }
        }

        data.SetHeights(0, 0, heights);
    }

}
