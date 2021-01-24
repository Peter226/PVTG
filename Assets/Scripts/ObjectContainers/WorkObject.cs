using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVTG
{
    public class WorkObject
    {
        private Mesh _mesh;
        private List<Material> _materials;
        private GameObject _previewObject;
        private List<Mesh> _subMeshes = new List<Mesh>();

        public WorkObject(Mesh mesh, MeshRenderer renderer)
        {
            List<int> triangles = new List<int>();
            Dictionary<int, int> triangleDict = new Dictionary<int, int>();
            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            List<Vector3> subVertices = new List<Vector3>();
            List<int> subTriangles = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                Debug.Log("Detected Submesh " + i.ToString());
                mesh.GetTriangles(triangles,i,false);

                int index = 0;
                for (int t = 0;t < triangles.Count;t++)
                {
                    int triangle = triangles[t];
                    if(triangleDict.ContainsKey(triangle))
                    {
                        subTriangles.Add(triangleDict[triangle]);
                    }
                    else
                    {
                        triangleDict.Add(triangle,index);
                        subTriangles.Add(index);
                        subVertices.Add(vertices[triangle]);
                        index++;
                    }
                }

                Mesh subMesh = new Mesh();
                subMesh.SetVertices(subVertices);
                subMesh.SetTriangles(subTriangles,0);
                subMesh.RecalculateBounds();
                GameObject test = new GameObject();
                test.AddComponent<MeshCollider>();
                test.GetComponent<MeshCollider>().sharedMesh = subMesh;
                _subMeshes.Add(subMesh);
                triangleDict.Clear();
                subVertices.Clear();
                subTriangles.Clear();

                triangles.Clear();
            }
            

            _mesh = mesh;
            renderer.GetSharedMaterials(_materials);
        }
    }
}