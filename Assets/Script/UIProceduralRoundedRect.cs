using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
public class UIProceduralRoundedRect : MaskableGraphic
{
    [Range(0, 100)] public float cornerRadius = 20f;
    [Range(4, 32)] public int cornerResolution = 8;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;
        Vector2 size = new Vector2(rect.width, rect.height);

        // Clamp radius to ensure it never exceeds half the width or height
        float radius = Mathf.Min(cornerRadius, Mathf.Min(size.x, size.y) * 0.5f);

        if (radius <= 0)
        {
            // Fallback to a standard rectangle if radius is 0
            AddQuad(vh, rect.min, rect.max);
            return;
        }

        // Define inner bounds representing the central core matrix
        Vector2 innerMin = rect.min + new Vector2(radius, radius);
        Vector2 innerMax = rect.max - new Vector2(radius, radius);

        // 1. Center and side quads (the cross shape)
        AddQuad(vh, new Vector2(rect.xMin, innerMin.y), new Vector2(rect.xMax, innerMax.y));
        AddQuad(vh, new Vector2(innerMin.x, rect.yMin), new Vector2(innerMax.x, innerMin.y));
        AddQuad(vh, new Vector2(innerMin.x, innerMax.y), new Vector2(innerMax.x, rect.yMax));

        // 2. Generate the 4 rounded corners using trigonometry
        AddCorner(vh, new Vector2(innerMax.x, innerMax.y), radius, 0);   // Top Right
        AddCorner(vh, new Vector2(innerMin.x, innerMax.y), radius, 90);  // Top Left
        AddCorner(vh, new Vector2(innerMin.x, innerMin.y), radius, 180); // Bottom Left
        AddCorner(vh, new Vector2(innerMax.x, innerMin.y), radius, 270); // Bottom Right
    }

    private void AddQuad(VertexHelper vh, Vector2 min, Vector2 max)
    {
        int startIndex = vh.currentVertCount;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        Vector2[] positions = {
            new Vector2(min.x, min.y),
            new Vector2(min.x, max.y),
            new Vector2(max.x, max.y),
            new Vector2(max.x, min.y)
        };

        for (int i = 0; i < 4; i++)
        {
            vertex.position = positions[i];
            vh.AddVert(vertex);
        }

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private void AddCorner(VertexHelper vh, Vector2 center, float radius, float startAngle)
    {
        int centerIndex = vh.currentVertCount;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = center;
        vh.AddVert(vertex);

        int prevIndex = -1;
        float angleStep = 90f / cornerResolution;

        for (int i = 0; i <= cornerResolution; i++)
        {
            float rad = Mathf.Deg2Rad * (startAngle + (i * angleStep));
            Vector2 pos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            vertex.position = pos;
            vh.AddVert(vertex);

            int currIndex = vh.currentVertCount - 1;
            if (i > 0)
            {
                vh.AddTriangle(centerIndex, prevIndex, currIndex);
            }
            prevIndex = currIndex;
        }
    }
}
