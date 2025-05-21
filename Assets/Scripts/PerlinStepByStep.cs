using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinStepByStep : MonoBehaviour, ITypeManager {
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
  private List<List<float>> mappings;
  private List<List<float>> blendMappings;
  [SerializeField] private int gradientToDebug;
  [SerializeField] private Renderer[] renderers;

  void Start() {
    LoadVariables();
    PlaygroundManager.OnVariableChange += LoadVariables;
  }
  void OnDestroy()
  {
    PlaygroundManager.OnVariableChange -= LoadVariables;
  }

  private void LoadVariables(object sender, EventArgs e)
  {
    LoadVariables();
    GenerateImage();
  }

  [ContextMenu("GenerateInfluenceVectors")]
  private void GenerateInfluenceVectors() {
    List<GameObject> vectors = new();
    for (int j = 0; j < grid.y + 1; j++) {
      for (int i = 0; i < grid.x + 1; i++) {
        float random = wordMaxValue - UnityEngine.Random.Range(0, wordMaxValue + 1);
        float angle = 360 * (random / wordMaxValue);
        Vector2 generated = new(Mathf.Cos(Mathf.Deg2Rad * angle), -Mathf.Sin(Mathf.Deg2Rad * angle));
        GameObject line = Instantiate(LineDebug, Vector3.zero, Quaternion.Euler(new(0, angle, 0)));
        line.name = (i + j * (int)(grid.x + 1)).ToString();
        line.GetComponent<PixelData>().SetData(generated, angle);
        line.GetComponent<LineRenderer>().enabled = false;
        // Material material = new(line.GetComponent<LineRenderer>().material);
        // line.GetComponent<LineRenderer>().material = material;
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

  private float[] GeneratePerlinValue(Vector2 pixelPos) {
    Vector2 quadrant = new(
      (int)(pixelPos.x / (textureResolution.x / grid.x)),
      (int)(pixelPos.y / (textureResolution.y / grid.y))
    );

    GameObject[] vectors = GetQuadrantInfluenceVectors(quadrant);
    Vector2[] positions = GetQuadrantInfluenceVectorsPosition(quadrant);

    Vector2 pixelVirtual = GetPixelVirtualPosition(pixelPos);
    Vector2 pixelRel = new(
      (pixelVirtual.x - quadrant.x) / 1f,
      (pixelVirtual.y - quadrant.y) / 1f
    );

    Vector2 offsetUL = GetOffsetVector(positions[0], pixelVirtual);
    Vector2 offsetUR = GetOffsetVector(positions[1], pixelVirtual);
    Vector2 offsetLL = GetOffsetVector(positions[2], pixelVirtual);
    Vector2 offsetLR = GetOffsetVector(positions[3], pixelVirtual);

    float ul = Vector2.Dot(vectors[0].GetComponent<PixelData>().offset, offsetUL);
    float ur = Vector2.Dot(vectors[1].GetComponent<PixelData>().offset, offsetUR);
    float ll = Vector2.Dot(vectors[2].GetComponent<PixelData>().offset, offsetLL);
    float lr = Vector2.Dot(vectors[3].GetComponent<PixelData>().offset, offsetLR);

    float sx = Fade(pixelRel.x);
    float sy = Fade(pixelRel.y);

    float top = Mathf.Lerp(ul, ur, sx);
    float bottom = Mathf.Lerp(ll, lr, sx);
    float value = Mathf.Lerp(bottom, top, sy);

    float[] toReturn = new float[7] {
      RemapDotProduct(ul),
      RemapDotProduct(ur),
      RemapDotProduct(ll),
      RemapDotProduct(lr),
      RemapDotProduct(top),
      RemapDotProduct(bottom),
      RemapDotProduct(value)
    };
    return toReturn;
  }

  [ContextMenu("GenerateImage")]
  public void GenerateImage() {
    StartCoroutine(Draw());
  }
  private IEnumerator Draw() {
    Dictionary<Vector2, Color>[] pixelData = new Dictionary<Vector2, Color>[renderers.Length];
    for (int i = 0; i < pixelData.Length; i++) {
        pixelData[i] = new Dictionary<Vector2, Color>();
    }

    for (int i = 0; i < textureResolution.x; i++) {
      for (int j = 0; j < textureResolution.y; j++) {
        
        for(int q = 0; q <  renderers.Length; q++) {
          float perlin = GeneratePerlinValue(new(i, j))[q];
          Color pixelColor = new(perlin, perlin, perlin, 1);
          pixelData[q][new Vector2(i, j)] = pixelColor;
          if (drawLines && q == gradientToDebug) {
            Vector2 offsetVector = GetOffsetVector(GetPixelVirtualPosition(new(i, j)), new(i, j));
            float angle = Vector2.SignedAngle(offsetVector, Vector2.right);
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 pixelVirtualPos = new(GetPixelVirtualPosition(new(i, j)).x, 0, GetPixelVirtualPosition(new(i, j)).y);
            GameObject pixelLine = Instantiate(LineDebug, pixelVirtualPos, rotation);
            if (!showOffsetOriginalSize) pixelLine.transform.localScale *= 0.1f;
            else pixelLine.GetComponent<LineRenderer>().SetPosition(1, new(0, 0, offsetVector.magnitude));
            offsets.Add(pixelLine, q);
            pixelLine.GetComponent<PixelData>().SetData(offsetVector, perlin, perlin, angle, pixelColor, new(i, j));
          }
        }
      }
      for(int q = 0; q <  renderers.Length; q++) {
        Texture2D image = Utils.CreateTexture(textureResolution, pixelData[q]);
        image.filterMode = FilterMode.Point;
        image.wrapMode = TextureWrapMode.Clamp;

        Material material = new(renderers[q].material);
        material.SetTexture("_BaseMap", image);
        material.GetTexture("_BaseMap").filterMode = FilterMode.Point;
        material.GetTexture("_BaseMap").wrapMode = TextureWrapMode.Clamp;
        renderers[q].material = material;
        
      }
      // mappings[q].Add(perlin);
      yield return null;
    }
    for(int q = 0; q <  renderers.Length; q++) {
      Texture2D image = Utils.CreateTexture(textureResolution, pixelData[q]);
      image.filterMode = FilterMode.Point;
      image.wrapMode = TextureWrapMode.Clamp;

      Material material = new(renderers[q].material);
      material.SetTexture("_BaseMap", image);
      material.GetTexture("_BaseMap").filterMode = FilterMode.Point;
      material.GetTexture("_BaseMap").wrapMode = TextureWrapMode.Clamp;
      renderers[q].material = material;
      
    }
  }
  

  private GameObject[] GetQuadrantInfluenceVectors(Vector2 quadrant) {
    int width = (int)(grid.x + 1);
    int ul = (int)(quadrant.x + (quadrant.y + 1) * width);
    int ur = (int)(quadrant.x + 1 + (quadrant.y + 1) * width);
    int ll = (int)(quadrant.x + quadrant.y * width);
    int lr = (int)(quadrant.x + 1 + quadrant.y * width);
    return new GameObject[] {
      influenceVectors[ul],
      influenceVectors[ur],
      influenceVectors[ll],
      influenceVectors[lr],
    };
  }

  private Vector2[] GetQuadrantInfluenceVectorsPosition(Vector2 quadrant) {
    return new Vector2[] {
      new(quadrant.x, quadrant.y + 1),
      new(quadrant.x + 1, quadrant.y + 1),
      new(quadrant.x, quadrant.y),
      new(quadrant.x + 1, quadrant.y),
    };
  }

  private Vector2 GetOffsetVector(Vector2 influenceVectorPosition, Vector2 pixel) {
    return influenceVectorPosition - pixel;
  }

  private Vector2 GetPixelVirtualPosition(Vector2 pixel) {
    return new(
      grid.x * pixel.x / textureResolution.x,
      grid.y * pixel.y / textureResolution.y
    );
  }

  [ContextMenu("ToggleOffsetLineView")]
  private void ToggleOffsetLineView() {
    foreach (KeyValuePair<GameObject, int> pair in offsets) {
      pair.Key.SetActive(pair.Value == gradientToDebug);
    }
    for (int i = 0; i < renderers.Length; i++) {
      renderers[i].gameObject.SetActive(i == gradientToDebug);
    }
  }

  private float Fade(float t) {
    return t * t * t * (t * (t * 6 - 15) + 10);
  }

  private float RemapDotProduct(float x) {
    return (x + 1) / 2;
  }

  // [ContextMenu("BlendHorizontally")]
  // private void BlendHorizontally() {
  //   Texture2D upper = Blend(mappings[0], mappings[1]);
  //   Texture2D lower = Blend(mappings[2], mappings[3]);
  //   Texture2D final = Blend(blendMappings[0], blendMappings[1]);

  //   Material upperMaterial = new(blends[0].material);
  //   upperMaterial.SetTexture("_BaseMap", upper);
  //   Material lowerMaterial = new(blends[1].material);
  //   lowerMaterial.SetTexture("_BaseMap", lower);
  //   Material finalMaterial = new(blends[0].material);
  //   finalMaterial.SetTexture("_BaseMap", final);

  //   blends[0].material = upperMaterial;
  //   blends[0].enabled = true;
  //   blends[1].material = lowerMaterial;
  //   blends[1].enabled = true;
  //   blends[2].material = finalMaterial;
  //   blends[2].enabled = true;
  // }

  private Texture2D Blend(List<float> l, List<float> r) {
    Texture2D image = new((int)textureResolution.x, (int)textureResolution.y, TextureFormat.RGBA32, false);
    List<float> blended = new();
    Color[] allPixels = new Color[(int)(textureResolution.x * textureResolution.y)];
    for (int i = 0; i < textureResolution.x; i++) {
      for (int j = 0; j < textureResolution.y; j++) {
        int index = j + i * (int)textureResolution.y;

        float pixelL = l[index];
        float pixelR = r[index];

        float lerpValue = j / (textureResolution.y - 1);
        float smoothValue = Mathf.SmoothStep(0f, 1f, lerpValue);
        float value = Mathf.Lerp(pixelL, pixelR, smoothValue);

        Color pixelColor = new(value, value, value, 1);
        blended.Add(value);
        allPixels[index] = pixelColor;
      }
    }
    blendMappings.Add(blended);
    image.SetPixels(allPixels);
    image.filterMode = FilterMode.Point;
    image.wrapMode = TextureWrapMode.Clamp;
    image.Apply();
    return image;
  }

  public void LoadVariables()
  {
    PlaygroundManager.integerVariables.TryGetValue("STEP_BY_STEP_SIZE", out int size);
    PlaygroundManager.stringVariables.TryGetValue("STEP_BY_STEP_SEED", out string _seed);
    PlaygroundManager.integerVariables.TryGetValue("STEP_BY_STEP_RESOLUTION", out int _resolution);
    grid = new(size, size);
    int.TryParse(_seed, out int result);
    seed = result;
    textureResolution = new(_resolution * size, _resolution * size);
        offsets = new();
    blendMappings = new();
    mappings = new();
    for (int i = 0; i < perlinPlaneRenderers.Length; i++) mappings.Add(new());

    UnityEngine.Random.InitState(seed);
    GenerateInfluenceVectors();

    foreach (MeshSet renderer in perlinPlaneRenderers) {
      if (renderer.generate) {
        renderer.renderer.transform.localScale = new(4 * 0.1f, 1, 4 * 0.1f);
        renderer.renderer.transform.position = new(4 / 2, -0.01f, 4 / 2);
      } else {
        renderer.renderer.gameObject.SetActive(false);
      }
    }

    for (int i = 0; i < renderers.Length; i++) {
      renderers[i].gameObject.transform.localScale = new(4 * 0.1f, 1, 4 * 0.1f);
      renderers[i].gameObject.transform.position = new((i%4*5)+4 / 2, -0.01f, -i/4*5+4 / 2);
    }
  }
}
