using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
public class TexturizerGUI : Graphic
{
    private int indexCounter;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        indexCounter = 0;



        DrawProportionalWindow(new float4(0,0,1,1), color, vh);


        DrawWindow(new float2(0.7f,0.5f),new float2(2,1),new Color(56 / 255.0f, 56 / 255.0f, 56 / 255.0f),vh);
    }


    private void DrawWindow(float2 position, float2 size, Color windowColor, VertexHelper vh)
    {
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        float minSize = math.min(width,height) * 0.1f;
        float2 rePosition = position * new float2(width,height) - new float2(width, height) * 0.5f;

        float left = -minSize * size.x + rePosition.x;
        float right = minSize * size.x + rePosition.x;
        float top = -minSize * size.y + rePosition.y;
        float bottom = minSize * size.y + rePosition.y;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = windowColor;
        vertex.position = new Vector3(left, bottom);
        vh.AddVert(vertex);
        vertex.position = new Vector3(left, top);
        vh.AddVert(vertex);
        vertex.position = new Vector3(right, bottom);
        vh.AddVert(vertex);
        vertex.position = new Vector3(right, top);
        vh.AddVert(vertex);
        
        vh.AddTriangle(indexCounter, 1 + indexCounter, 2 + indexCounter);
        vh.AddTriangle(1 + indexCounter, 2 + indexCounter, 3 + indexCounter);
        indexCounter += 4;
    }


    private void DrawProportionalWindow(float4 rect, Color windowColor, VertexHelper vh)
    {
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        float left = rect.x * width - width * 0.5f;
        float right = rect.z * width - width * 0.5f;
        float top = rect.y * height - height * 0.5f;
        float bottom = rect.w * height - height * 0.5f;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = windowColor;
        vertex.position = new Vector3(left, bottom);
        vh.AddVert(vertex);
        vertex.position = new Vector3(left, top);
        vh.AddVert(vertex);
        vertex.position = new Vector3(right, bottom);
        vh.AddVert(vertex);
        vertex.position = new Vector3(right, top);
        vh.AddVert(vertex);

        vh.AddTriangle(indexCounter, 1 + indexCounter, 2 + indexCounter);
        vh.AddTriangle(1 + indexCounter, 2 + indexCounter, 3 + indexCounter);

        indexCounter += 4;


    }



}
