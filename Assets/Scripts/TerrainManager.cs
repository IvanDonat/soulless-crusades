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
    private float[,] heights;
    private float timePassedSinceTerrainUpdate = -1f;

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
        timePassedSinceTerrainUpdate -= Time.deltaTime;
        if (timePassedSinceTerrainUpdate < 0)
        {
            SetTerrainToCircle(100f - Time.time);

            ReloadTerrain();

            timePassedSinceTerrainUpdate = 1 / 10f;
        }
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
                heights[i, j] = 1;
            }
        }

        data.SetHeights(0, 0, heights);
    }



    public void SetTerrainToCircle(float radius)
    {
        float terrainStartDescentDist = radius;
        float descentLength = 5;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float h = 0;

                float offsetX = Mathf.Abs(i - width / 2);
                float offsetY = Mathf.Abs(j - height / 2);
                float dist = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);

                if (dist < terrainStartDescentDist)
                    h = 1;
                else if (dist >= terrainStartDescentDist + descentLength)
                    h = 0;
                else
                {
                    h = Mathf.Lerp(1, 0, (dist - terrainStartDescentDist) / (dist - terrainStartDescentDist + descentLength));
                }

                heights[i, j] = h;
            }
        }
    }
}
