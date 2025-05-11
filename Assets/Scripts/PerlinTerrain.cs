using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    [SerializeField] private GameObject cube;
    [SerializeField] private Vector2 terrainGrid;
    [SerializeField] private bool minecraftLike = false;
    [SerializeField] private float heightTemperature;
    [SerializeField] private float speed = 1f;
    private bool started = false;
    private float elapsed;
    private Transform[] cubes;

    [ContextMenu("Generate")]
    public void Spawn()
    {
        started = true;
        foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube")) {
            Destroy(cube);
        }
        List<Transform> cubesList = new();
        for (int i = 0; i < terrainGrid.x; i++)
        {
            for (int j = 0; j < terrainGrid.y; j++)
            {
                float perlinHeight = GetHeight(i, j);
                Vector3 pos = new(i, perlinHeight, j);
                GameObject go = Instantiate(cube, pos, Quaternion.identity);
                cubesList.Add(go.transform);
            }
        }
        cubes = cubesList.ToArray();
    }
    private float GetHeight(float i, float j)
    {
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
        elapsed += speed*Time.deltaTime;
        ChangePosition();
    }
  }
  private void ChangePosition() {
    foreach (Transform cube in cubes) {
        Vector3 cubePos = cube.position;
        float height = GetHeight(cubePos.x, cubePos.z);
        cube.position = new(cubePos.x, height, cubePos.z);
    }
  }
}
