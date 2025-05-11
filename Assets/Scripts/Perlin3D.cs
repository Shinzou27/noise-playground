using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Perlin3D : MonoBehaviour
{
  [Serializable]
  public struct MeshSet {
    public Renderer renderer;
    public bool generate;
  }
  [SerializeField] private int seed;
  [SerializeField] private int wordMaxValue;
  [SerializeField] private Vector2 grid;
  [SerializeField] private GameObject LineDebug;
  [SerializeField] private Vector2 textureResolution;
  [SerializeField] private MeshSet[] perlinPlaneRenderers;
  [SerializeField] private bool showOffsetOriginalSize;
  [SerializeField] private bool drawLines;
  public List<Color> colorsAssigned;
  public GameObject[] influenceVectors;
  private Dictionary<GameObject, int> offsets;
  private List<float>[] mappings;
  private List<List<float>> blendMappings;
  [SerializeField] private int gradientToDebug;
  [SerializeField] private Renderer[] blends;
  void Start() {
    offsets = new();
    blendMappings = new();
    mappings = new List<float>[4] {
      new(),
      new(),
      new(),
      new(),
    };
    UnityEngine.Random.InitState(seed);
    GenerateInfluenceVectors();
    foreach (MeshSet renderer in perlinPlaneRenderers) {
      if (renderer.generate) {
        renderer.renderer.transform.localScale = new (grid.x*0.1f, 1, grid.y*0.1f);
        renderer.renderer.transform.position = new(grid.x/2, -0.01f, grid.y/2);
      } else {
        renderer.renderer.gameObject.SetActive(false);
      }
    }
    foreach (Renderer renderer in blends) {
      renderer.transform.localScale = new (grid.x*0.1f, 1, grid.y*0.1f);
      renderer.transform.position = new(grid.x/2, -0.01f, grid.y/2);
      renderer.enabled = false;
    }
  }

  [ContextMenu("GenerateInfluenceVectors")]
  private void GenerateInfluenceVectors() {
    List<GameObject> vectors = new();
    for (int j = 0; j < grid.y+1; j++) {
      for (int i = 0; i < grid.x+1; i++) {
        float random = wordMaxValue - UnityEngine.Random.Range(0, wordMaxValue+1);
        float angle = 360*(random/wordMaxValue);
        Vector2 generated = new (Mathf.Cos(Mathf.Deg2Rad * angle), -Mathf.Sin(Mathf.Deg2Rad * angle));
        GameObject line = Instantiate(LineDebug, new Vector3(0, 0, 0), Quaternion.Euler(new(0, angle, 0)));
        line.name = (i + j*(grid.y+1)).ToString();
        line.GetComponent<PixelData>().SetData(generated, angle);
        Material material = new(line.GetComponent<LineRenderer>().material);
        line.GetComponent<LineRenderer>().material = material;
        vectors.Add(line);
        PutOnGrid(line, i, j);
      }
    }
    influenceVectors = vectors.ToArray();
  }

  private void PutOnGrid(GameObject line, int x, int z) {
    line.transform.position = new(x, 0, z);
    if (!showOffsetOriginalSize) line.transform.localScale *= 0.5f;
  }
  private float GeneratePerlinValue(Vector2 pixelPos, int cornerIndex) {
    Vector2 quadrant = new(
      (int)(pixelPos.x / (textureResolution.x / grid.x)),
      (int)(pixelPos.y / (textureResolution.y / grid.y))
    );

    GameObject[] quadrantVectors = GetQuadrantInfluenceVectors(quadrant);
    Vector2[] vectorPositions = GetQuadrantInfluenceVectorsPosition(quadrant);

    Vector2 offset = GetOffsetVector(vectorPositions[cornerIndex], pixelPos);
    Vector2 influence = quadrantVectors[cornerIndex].GetComponent<PixelData>().offset;

    float dot = Vector2.Dot(influence, offset);
    return RemapDotProduct(dot);
  }


  [ContextMenu("GenerateImage")]
  private void GenerateImage() {
    for(int q = 0; q < perlinPlaneRenderers.Length; q++) {
      if (!perlinPlaneRenderers[q].generate) continue;

      Dictionary<Vector2, Color> pixelData = new();

      for (int i = 0; i < textureResolution.x; i++) {
        for (int j = 0; j < textureResolution.y; j++) {
          Vector2 quadrant = new ((int)(i / (textureResolution.x / grid.x)), (int)(j / (textureResolution.y / grid.y)));
          Vector2 offsetVector = GetOffsetVector(GetQuadrantInfluenceVectorsPosition(quadrant)[q], new(i, j));
          float perlin = GeneratePerlinValue(new(i, j), q);
          Color pixelColor = new(perlin, perlin, perlin, 1);
          pixelData[new Vector2(i, j)] = pixelColor;
          colorsAssigned.Add(pixelColor);
          mappings[q].Add(perlin);
          if (i == 8 && j == 15 && q == 0) {
            Debug.Log($"{q}: ({i}, {j}) Produto escalar mapeado: {perlin}, cor: <color=#{pixelColor.ToHexString()}>██████████████████ ou {pixelColor.ToHexString()}</color>");
          }
          if (drawLines) {
            float angle = Vector2.SignedAngle(offsetVector, Vector2.right);
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 pixelVirtualPos = GetPixelVirtualPositionOnQuadrant(new(i, j));
            pixelVirtualPos = new(pixelVirtualPos.x, 0, pixelVirtualPos.y);
            GameObject pixelLine = Instantiate(LineDebug, pixelVirtualPos, rotation);
            if (!showOffsetOriginalSize) {
              pixelLine.transform.localScale *= 0.1f;
            } else {
              pixelLine.GetComponent<LineRenderer>().SetPosition(1, new(0, 0, offsetVector.magnitude));
            }
            offsets.Add(pixelLine, q);
            pixelLine.GetComponent<PixelData>().SetData(offsetVector, perlin, perlin, angle, pixelColor, new(i, j));
          }
        }
      }

      Texture2D image = Utils.CreateTexture(textureResolution, pixelData);
      image.filterMode = FilterMode.Point;
      image.wrapMode = TextureWrapMode.Clamp;

      Material material = new(perlinPlaneRenderers[q].renderer.material);
      material.SetTexture("_BaseMap", image);
      material.GetTexture("_BaseMap").filterMode = FilterMode.Point;
      material.GetTexture("_BaseMap").wrapMode = TextureWrapMode.Clamp;
      perlinPlaneRenderers[q].renderer.material = material;
    }
  }


  private float GetDotProduct(Vector2 influence, Vector2 offsetVector) {
    // Debug.Log(influence);
    // Debug.Log(offsetVector);
    return Vector2.Dot(influence, offsetVector);
  }

  private GameObject[] GetQuadrantInfluenceVectors(Vector2 quadrant) {
    /*
    0,0
    0+(0+1)*2 = 2
    1+(0+1)*2 = 3
    0+0*2 = 0
    0+1+0*2=1
    
    */
    int ul = (int)(quadrant.x + (quadrant.y+1)*(grid.y+1));
    int ur = (int)(quadrant.x+1 + (quadrant.y+1)*(grid.y+1));
    int ll = (int)(quadrant.x + quadrant.y*(grid.y+1));
    int lr = (int)(quadrant.x+1 + quadrant.y*(grid.y+1));

    return new GameObject[] {
      influenceVectors[ul],
      influenceVectors[ur],
      influenceVectors[ll],
      influenceVectors[lr],
    };
  }
  private Vector2[] GetQuadrantInfluenceVectorsPosition(Vector2 quadrant) {
    return new Vector2[] {
      new(quadrant.x, quadrant.y+1),
      new(quadrant.x+1, quadrant.y+1),
      new(quadrant.x, quadrant.y),
      new(quadrant.x+1, quadrant.y),
    };
  }
  private Vector2 GetOffsetVector(Vector2 influenceVectorPosition, Vector2 pixel) {
    return 
    influenceVectorPosition
    - 
    GetPixelVirtualPositionOnQuadrant(pixel) 
    ;
  }
  private Vector2 GetPixelVirtualPositionOnQuadrant(Vector2 pixel) {
    return new(
    grid.x * pixel.x/textureResolution.x + GetPixelSize().x/2,
    grid.y * pixel.y/textureResolution.y + GetPixelSize().y/2
    );
  }
  // private Vector3 GetPixelWorldPos(Vector2 pixel) {

  // }
  [ContextMenu("ToggleOffsetLineView")]
  private void ToggleOffsetLineView() {
    foreach (KeyValuePair<GameObject, int> pair in offsets) {
      pair.Key.SetActive(pair.Value == gradientToDebug);
    }
    for (int i = 0; i < perlinPlaneRenderers.Length; i++) {
      perlinPlaneRenderers[i].renderer.gameObject.SetActive(i == gradientToDebug);
    }
  }
  private Vector2 GetPixelSize() {
    return new (grid.x/textureResolution.x, grid.y/textureResolution.y);
  }
  private float RemapDotProduct(float x) {
    return (x+1)/2;
  }

  [ContextMenu("BlendHorizontally")]
  private void BlendHorizontally() {
    Texture2D upper = Blend(mappings[0], mappings[1]);
    Texture2D lower = Blend(mappings[2], mappings[3]);
    Texture2D final = Blend(blendMappings[0], blendMappings[1]);
    Material upperMaterial = new(blends[0].material);
    upperMaterial.SetTexture("_BaseMap", upper);
    Material lowerMaterial = new(blends[1].material);
    lowerMaterial.SetTexture("_BaseMap", lower);
    Material finalMaterial = new(blends[0].material);
    finalMaterial.SetTexture("_BaseMap", final);
    blends[0].material = upperMaterial;
    blends[0].enabled = true;
    blends[1].material = lowerMaterial;
    blends[1].enabled = true;
    blends[2].material = finalMaterial;
    blends[2].enabled = true;
  }
  private void DebugGradient(Color[] colors) {
    string aux = "";
    foreach (Color color in colors) {
      aux += $"<color=#{color.ToHexString()}>█</color>";
    }
      Debug.Log(aux);
  }
  private Texture2D Blend( List<float> l, List<float> r) {
    Texture2D image = new ((int)textureResolution.x, (int)textureResolution.y, TextureFormat.RGBA32, false);
    List<float> blended = new();
    Color[] allPixels = new Color[(int)(textureResolution.x * textureResolution.y)];
    for (int i = 0; i < textureResolution.x; i++) {
      Color[] originalL = new Color[(int)textureResolution.y];
      Color[] originalR = new Color[(int)textureResolution.y];
      Color[] line = new Color[(int)textureResolution.y];
      string aux = "";
      for (int j = 0; j < textureResolution.y; j++) {
        int index = j + i * (int)textureResolution.y;

        float pixelL = l[index];
        float pixelR = r[index];

        float lerpValue = j / (textureResolution.y - 1);
        float smoothValue = Mathf.SmoothStep(0f, 1f, lerpValue);
        float value = Mathf.Lerp(pixelL, pixelR, smoothValue);

        Color pixelColor = new(value, value, value, 1);

        aux += $"({i}, {j}), ";
        originalL[j] = new(pixelL, pixelL, pixelL);
        originalR[j] = new(pixelR, pixelR, pixelR);
        line[j] = pixelColor;
        blended.Add(value);
        allPixels[index] = pixelColor;
      }
      Debug.Log(aux);
      DebugGradient(originalL);
      DebugGradient(originalR);
      DebugGradient(line);
    }
    image.SetPixels(allPixels);
    blendMappings.Add(blended);
    image.filterMode = FilterMode.Point;
    image.wrapMode = TextureWrapMode.Clamp;
    image.Apply();
    return image;
    // byte[] bytes = image.EncodeToPNG();
    // File.WriteAllBytes(Application.dataPath + "/Sprites/tex.png", bytes);
  }
}