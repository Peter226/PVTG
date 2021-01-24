using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
[RequireComponent(typeof(Camera))]
public class UVRenderer : MonoBehaviour
{
    
    public List<Material> materials = new List<Material>();
    public ComputeShader edgeFixer;
    private Camera _camera;
    void Start()
    {
        _camera = GetComponent<Camera>();
    }
    public List<RenderTexture> RenderMeshes(List<Mesh> meshesToRender, int2 resolution, int uv = 0)
    {
        List<RenderTexture> textures = new List<RenderTexture>();
        List<Mesh> convertedMeshes = new List<Mesh>();


        foreach (Mesh mesh in meshesToRender) { 
            convertedMeshes.Add(ConvertMesh(mesh, uv));
        }

        foreach (Material material in materials)
        {
            RenderTexture renderTexture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGBFloat);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
            _camera.targetTexture = renderTexture;
            foreach (Mesh mesh in convertedMeshes)
            {
                Graphics.DrawMesh(mesh, new Vector3(-0.5f, -0.5f, 0.5f), transform.rotation, material, 6, _camera);
            }
            _camera.Render();

            RenderTexture outputTexture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGBFloat);
            outputTexture.enableRandomWrite = true;
            outputTexture.Create();
            int kernelHandle = edgeFixer.FindKernel("CSMain");
            edgeFixer.SetTexture(kernelHandle, "Result", outputTexture);
            edgeFixer.SetTexture(kernelHandle, "Input", renderTexture);
            edgeFixer.Dispatch(0,renderTexture.width / 8,renderTexture.height / 8, 1);
            edgeFixer.SetTexture(kernelHandle, "Result", renderTexture);
            edgeFixer.SetTexture(kernelHandle, "Input", outputTexture);
            edgeFixer.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
            textures.Add(renderTexture);
        }

        return textures;
    }
    private Mesh ConvertMesh(Mesh source, int uv)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        source.GetVertices(vertices);
        List<Vector2> uvs = new List<Vector2>();
        source.GetUVs(uv, uvs);
        List<Vector2> uvs2 = new List<Vector2>();
        List<Vector2> uvs3 = new List<Vector2>();
        try
        {
            for (int v = 0; v < vertices.Count; v++)
            {
                Vector3 pos = new Vector3(uvs[v].x, uvs[v].y, 0.5f);
                Vector3 vertex = vertices[v];
                uvs2.Add(new Vector2(vertex.x, vertex.y));
                uvs3.Add(new Vector2(vertex.z, 0.0f));
                vertices[v] = pos;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(1, uvs2);
            mesh.SetUVs(2, uvs3);

            mesh.SetTriangles(source.triangles, 0);
            mesh.SetNormals(source.normals);
            mesh.SetColors(source.colors);
            mesh.RecalculateBounds();

        }
        catch (Exception e)
        {
            Debug.LogError("Error: could not convert mesh\n" + e.ToString());
        }

        return mesh;
    }


}
