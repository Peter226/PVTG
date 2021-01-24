using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using PVTG.Graph;
public class Texturizer : MonoBehaviour
{
    public UVRenderer uvRenderer;
    public List<Mesh> testMeshes = new List<Mesh>();
    public List<RenderTexture> renderedTextures = new List<RenderTexture>();
    public Material testMaterial;

    public TextureGraph currentGraph = new TextureGraph("TestGraph");



    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            print("ran");
            renderedTextures = uvRenderer.RenderMeshes(testMeshes,new int2(256, 256));
            testMaterial.SetTexture("_BaseMap",renderedTextures[1]);
        }
    }
}

