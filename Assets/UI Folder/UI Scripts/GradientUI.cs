using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Gradient UI")]
public class GradientUI : BaseMeshEffect
{
    public Color colorTop = Color.white;
    public Color colorBottom = Color.black;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        int count = vertexList.Count;
        float bottomY = vertexList[0].position.y;
        float topY = vertexList[0].position.y;

        for (int i = 1; i < count; i++)
        {
            float y = vertexList[i].position.y;
            if (y > topY) topY = y;
            else if (y < bottomY) bottomY = y;
        }

        float uiHeight = topY - bottomY;

        for (int i = 0; i < count; i++)
        {
            UIVertex uiVertex = vertexList[i];
            // 버텍스 위치에 따라 색상을 혼합(Lerp)합니다.
            uiVertex.color = Color.Lerp(colorBottom, colorTop, (uiVertex.position.y - bottomY) / uiHeight);
            vertexList[i] = uiVertex;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }
}
