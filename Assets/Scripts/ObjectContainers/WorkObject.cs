using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVTG
{
    public class WorkObject
    {
        public List<Material> materials = new List<Material>();
        public Mesh mesh;
        private GameObject _previewObject;
        private List<Mesh> _subMeshes = new List<Mesh>();
        private List<GameObject> _colliderObjects = new List<GameObject>();
        public List<int> colliderIDs { get; }



        public void ToggleCollision(bool active)
        {
            foreach (GameObject gameObject in _colliderObjects)
            {
                gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Clean up after the preview object
        /// </summary>
        /// 
        public void Destroy()
        {
            Object.DestroyImmediate(_previewObject);
            for (int i = 0;i < _subMeshes.Count;i++)
            {
                Object.DestroyImmediate(_subMeshes[i]);
            }
            _subMeshes.Clear();
            materials.Clear();
            Object.DestroyImmediate(_previewObject);
            for (int i = 0;i < _colliderObjects.Count;i++)
            {
                Object.DestroyImmediate(_colliderObjects[i]);
            }
            _colliderObjects.Clear();
        }

        /// <summary>
        /// Create a new WorkObject for inspector preview.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="renderer"></param>
        /// <param name="parent"></param>
        public WorkObject(Mesh mesh, MeshRenderer renderer, Transform parent = null)
        {
            _previewObject = new GameObject();
            _previewObject.hideFlags = HideFlags.HideAndDontSave;
            _previewObject.transform.position = renderer.transform.position;
            _previewObject.transform.rotation = renderer.transform.rotation;
            _previewObject.transform.localScale = renderer.transform.localScale;
            _previewObject.transform.parent = parent;
            _previewObject.layer = 31;
            List<int> triangles = new List<int>();
            Dictionary<int, int> triangleDict = new Dictionary<int, int>();
            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);
            List<Vector3> subVertices = new List<Vector3>();
            List<int> subTriangles = new List<int>();
            colliderIDs = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
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
                GameObject colliderObject = new GameObject();
                colliderObject.hideFlags = HideFlags.HideAndDontSave;
                colliderObject.transform.position = _previewObject.transform.position;
                colliderObject.transform.rotation = _previewObject.transform.rotation;
                colliderObject.transform.localScale = _previewObject.transform.localScale;
                MeshCollider meshCollider = colliderObject.AddComponent<MeshCollider>();
                colliderIDs.Add(colliderObject.GetInstanceID());
                colliderObject.layer = 31;
                meshCollider.sharedMesh = subMesh;
                _colliderObjects.Add(colliderObject);
                colliderObject.SetActive(false);
                _subMeshes.Add(subMesh);
                triangleDict.Clear();
                subVertices.Clear();
                subTriangles.Clear();

                triangles.Clear();
            }
            

            this.mesh = mesh;
            renderer.GetSharedMaterials(materials);
            foreach (Material material in materials)
            {

                int propCount = material.shader.GetPropertyCount();
                for (int i = 0;i < propCount;i++)
                {
                    Debug.Log(material.shader.GetPropertyName(i));
                    Debug.Log(material.shader.GetPropertyType(i));
                }
            }
        }

        /// <summary>
        /// Render a preview of the meshes on the preview camera
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="layer"></param>
        public void Render(Camera camera, int layer)
        {
            if (_previewObject != null) {
                Matrix4x4 matrix = Matrix4x4.TRS(_previewObject.transform.position, _previewObject.transform.rotation, _previewObject.transform.lossyScale);
                for (int i = 0; i < materials.Count; i++)
                {
                    Graphics.DrawMesh(mesh, matrix, materials[i], layer, camera, i % mesh.subMeshCount);
                }
            }
        }




    }
}