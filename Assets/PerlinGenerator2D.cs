using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise3DGenerator : MonoBehaviour
{
    const int Size = 256;

    [SerializeField] int textureWidth = 256;
    [SerializeField] int textureHeight = 256;
    [SerializeField] float scale = 8f;
    [SerializeField] float zOffset = 0f;
    [Serializable]
    public struct CaveInfo {
        public GameObject cube;
        [Range(0, 1)]
        public float threshold;
        public int caveDepth;
    }
    [SerializeField] private CaveInfo caveInfo;

    int[] permutation;
    private List<Matrix4x4> persistentMatrices = new List<Matrix4x4>();
    private Mesh instanceMesh;
    private Material instanceMaterial;
    void Start()
    {
        permutation = MakePermutation();
        Texture2D noiseTex = GenerateNoiseTexture(textureWidth, textureHeight);
        noiseTex.filterMode = FilterMode.Point;
        noiseTex.wrapMode = TextureWrapMode.Clamp;
        GetComponent<Renderer>().material.SetTexture("_BaseMap", noiseTex);
        instanceMesh = caveInfo.cube.GetComponent<MeshFilter>().sharedMesh;
        instanceMaterial = new Material(caveInfo.cube.GetComponent<Renderer>().sharedMaterial);
    }

    Texture2D GenerateNoiseTexture(int width, int height)
    {
        Dictionary<Vector2, Color> pixelData = new Dictionary<Vector2, Color>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (float)x / width;
                float ny = (float)y / height;
                float value = Noise3D(nx * scale, ny * scale, zOffset * scale) * 0.5f + 0.5f;
                Color color = new Color(value, value, value);
                pixelData[new Vector2(x, y)] = color;
            }
        }

        return Utils.CreateTexture(new Vector2(width, height), pixelData);
    }


    int[] MakePermutation()
    {
        int[] p = new int[512];
        int[] basePerm = new int[256];
        for (int i = 0; i < 256; i++)
            basePerm[i] = i;

        Shuffle(basePerm);

        for (int i = 0; i < 512; i++)
            p[i] = basePerm[i % 256];

        return p;
    }

    void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    Vector3 GetConstantVector(int v)
    {
        int h = v & 15;
        switch (h)
        {
            case 0: return new Vector3(1, 1, 0);
            case 1: return new Vector3(-1, 1, 0);
            case 2: return new Vector3(1, -1, 0);
            case 3: return new Vector3(-1, -1, 0);
            case 4: return new Vector3(1, 0, 1);
            case 5: return new Vector3(-1, 0, 1);
            case 6: return new Vector3(1, 0, -1);
            case 7: return new Vector3(-1, 0, -1);
            case 8: return new Vector3(0, 1, 1);
            case 9: return new Vector3(0, -1, 1);
            case 10: return new Vector3(0, 1, -1);
            case 11: return new Vector3(0, -1, -1);
            default: return new Vector3(1, 1, 0);
        }
    }

    float Noise3D(float x, float y, float z)
    {
        int X = Mathf.FloorToInt(x) & 255;
        int Y = Mathf.FloorToInt(y) & 255;
        int Z = Mathf.FloorToInt(z) & 255;

        float xf = x - Mathf.Floor(x);
        float yf = y - Mathf.Floor(y);
        float zf = z - Mathf.Floor(z);

        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        int A = permutation[X] + Y;
        int AA = permutation[A] + Z;
        int AB = permutation[A + 1] + Z;
        int B = permutation[X + 1] + Y;
        int BA = permutation[B] + Z;
        int BB = permutation[B + 1] + Z;

        float x1, x2, y1, y2;
        x1 = Lerp(u,
            Vector3.Dot(GetConstantVector(permutation[AA]), new Vector3(xf, yf, zf)),
            Vector3.Dot(GetConstantVector(permutation[BA]), new Vector3(xf - 1, yf, zf))
        );
        x2 = Lerp(u,
            Vector3.Dot(GetConstantVector(permutation[AB]), new Vector3(xf, yf - 1, zf)),
            Vector3.Dot(GetConstantVector(permutation[BB]), new Vector3(xf - 1, yf - 1, zf))
        );
        y1 = Lerp(v, x1, x2);

        x1 = Lerp(u,
            Vector3.Dot(GetConstantVector(permutation[AA + 1]), new Vector3(xf, yf, zf - 1)),
            Vector3.Dot(GetConstantVector(permutation[BA + 1]), new Vector3(xf - 1, yf, zf - 1))
        );
        x2 = Lerp(u,
            Vector3.Dot(GetConstantVector(permutation[AB + 1]), new Vector3(xf, yf - 1, zf - 1)),
            Vector3.Dot(GetConstantVector(permutation[BB + 1]), new Vector3(xf - 1, yf - 1, zf - 1))
        );
        y2 = Lerp(v, x1, x2);

        return Lerp(w, y1, y2);
    }

    [ContextMenu("GenerateCave")]
    public void GenerateCave() {
        StartCoroutine(Generate());
    }

    public IEnumerator Generate() {
        persistentMatrices.Clear(); // Limpa matrizes anteriores
        
        for (int z = 0; z < caveInfo.caveDepth; z++) {
            for (int y = 0; y < textureHeight; y++) {
                for (int x = 0; x < textureWidth; x++) {
                    float nx = (float)x / textureWidth;
                    float ny = (float)y / textureHeight;
                    float nz = (float)z / caveInfo.caveDepth;
                    
                    float value = Noise3D(nx * scale, ny * scale, nz * scale) * 0.5f + 0.5f;
                    
                    if (value > caveInfo.threshold) {
                        persistentMatrices.Add(Matrix4x4.TRS(
                            new Vector3(x, z, y), 
                            Quaternion.identity, 
                            Vector3.one
                        ));
                    }
                }
                yield return null; // Permite renderização a cada linha Y
            }
        }
    }
    private void DrawMeshCombined(List<Matrix4x4> matrices) {
        Mesh mesh = caveInfo.cube.GetComponent<MeshFilter>().sharedMesh;
        Material material = caveInfo.cube.GetComponent<Renderer>().sharedMaterial;
        
        // Divide em lotes de 1023 matrizes
        for (int i = 0; i < matrices.Count; i += 1023) {
            int count = Mathf.Min(1023, matrices.Count - i);
            Matrix4x4[] batch = new Matrix4x4[count];
            matrices.CopyTo(i, batch, 0, count);
            Graphics.DrawMeshInstanced(mesh, 0, material, batch);
        }
    }
    void Update()
    {
        // Renderiza continuamente a cada frame
        if (persistentMatrices.Count > 0)
        {
            for (int i = 0; i < persistentMatrices.Count; i += 1023)
            {
                int count = Mathf.Min(1023, persistentMatrices.Count - i);
                Matrix4x4[] batch = new Matrix4x4[count];
                persistentMatrices.CopyTo(i, batch, 0, count);
                Graphics.DrawMeshInstanced(instanceMesh, 0, instanceMaterial, batch);
            }
        }
    }
}
