using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PixelData : MonoBehaviour
{
    public Vector2 offset = new();
    public Vector2 position = new();
    public float originalDotProduct = new();
    public float dotProduct = new();
    public float angle = new();
    public Color color = new();
    [SerializeField] private TextMeshProUGUI positionLabel;
    public void SetData(Vector2 offset, float angle) {
        this.offset = offset;
        this.angle = angle;
        // positionLabel.enabled = false;
    }
    public void SetData(Vector2 offset, float originalDotProduct, float dotProduct, float angle, Color color, Vector2 position) {
        this.offset = offset;
        this.originalDotProduct = originalDotProduct;
        this.dotProduct = dotProduct;
        this.angle = angle;
        this.color = color;
        this.position = position;
        positionLabel.text = $"({position.x},{position.y})";
    }
}
