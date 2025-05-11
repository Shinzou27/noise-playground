using System.Collections.Generic;
using UnityEngine;

public class PerlinTerrainInstanced : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private Vector2 terrainGrid;
    [SerializeField] private bool minecraftLike = false;
    [SerializeField] private float heightTemperature = 1f;
    [SerializeField] private float speed = 1f;

    public Matrix4x4[] matrices;
    public Vector3[] basePositions;
    private float elapsed;

    private const int INSTANCE_LIMIT = 10000; // Limite do Unity por chamada

    [ContextMenu("Generate")]
    public void Spawn()
    {
        int width = Mathf.FloorToInt(terrainGrid.x);
        int height = Mathf.FloorToInt(terrainGrid.y);
        int total = width * height;

        matrices = new Matrix4x4[total];
        basePositions = new Vector3[total];

        int index = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 pos = new Vector3(i, 0f, j);
                basePositions[index] = pos;
                matrices[index] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
                index++;
            }
        }
    }

    void Update()
    {
        if (matrices == null) return;

        elapsed += speed*Time.deltaTime;
        UpdateMatrices();
        DrawCubes();
    }

    private void UpdateMatrices()
    {
        int count = matrices.Length;
        float gridMax = Mathf.Max(terrainGrid.x, terrainGrid.y);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = basePositions[i];
            float height = GetHeight(pos.x, pos.z, gridMax);
            pos.y = height;
            matrices[i] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
        }
    }

    private float GetHeight(float i, float j, float scale)
    {
        float noise = Mathf.PerlinNoise((i + elapsed) / scale, (j + elapsed) / scale);
        if (minecraftLike)
            return Mathf.Ceil(Mathf.Clamp(noise, 0, 1) * heightTemperature);
        else
            return heightTemperature * noise;
    }

    private void DrawCubes()
    {
        int total = matrices.Length;
        int drawn = 0;

        while (drawn < total)
        {
            int batchCount = Mathf.Min(INSTANCE_LIMIT, total - drawn);
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices, batchCount, null, UnityEngine.Rendering.ShadowCastingMode.Off, false);
            drawn += batchCount;
        }
    }
}
