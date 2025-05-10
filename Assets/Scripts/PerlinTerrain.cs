using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    [SerializeField] private GameObject cube;
    [SerializeField] private Vector2 terrainGrid;
    [SerializeField] private bool minecraftLike = false;
    [SerializeField] private float heightTemperature;
    private bool started = false;
    private float elapsed;

    [ContextMenu("Generate")]
    public void Spawn()
    {
        started = true;
        foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube")) {
            Destroy(cube);
        }
        for (int i = 0; i < terrainGrid.x; i++)
        {
            for (int j = 0; j < terrainGrid.y; j++)
            {
                float perlinHeight = GetHeight(i, j);
                Vector3 pos = new(i, perlinHeight, j);
                Instantiate(cube, pos, Quaternion.identity);
            }
        }
    }
    private float GetHeight(float i, float j)
    {
        Debug.Log(Mathf.PerlinNoise((i+elapsed) / Mathf.Max(terrainGrid.x, terrainGrid.y), (j+elapsed) / Mathf.Max(terrainGrid.x, terrainGrid.y)));
        if (minecraftLike)
        {
            float height = Mathf.PerlinNoise((i+elapsed) / Mathf.Max(terrainGrid.x, terrainGrid.y), (j+elapsed) / Mathf.Max(terrainGrid.x, terrainGrid.y));
            return Mathf.Ceil(Mathf.Clamp(height, 0, 1) * heightTemperature);
        }
        else
        {
            return heightTemperature * Mathf.PerlinNoise((i+elapsed) / Mathf.Max(terrainGrid.x, terrainGrid.y), (j+elapsed) / Mathf.Max(terrainGrid.x, terrainGrid.y));
        }
    }
  void Update()
  {
    if (started) {
        elapsed += Time.deltaTime;
        ChangePosition();
    }
  }
  private void ChangePosition() {
    foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube")) {
        Vector3 cubePos = cube.transform.position;
        float height = GetHeight(cubePos.x, cubePos.z);
        cube.transform.position = new(cubePos.x, height, cubePos.z);
    }
  }
}
