using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
  public static Texture2D CreateTexture(Vector2 size, Dictionary<Vector2, Color> pixelData)
  {
    int width = Mathf.RoundToInt(size.x);
    int height = Mathf.RoundToInt(size.y);
    Texture2D texture = new Texture2D(width, height);

    foreach (var entry in pixelData)
    {
      int x = Mathf.RoundToInt(entry.Key.x);
      int y = Mathf.RoundToInt(entry.Key.y);

      if (x >= 0 && x < width && y >= 0 && y < height)
        texture.SetPixel(x, y, entry.Value);
    }

    texture.Apply();
    return texture;
  }

}