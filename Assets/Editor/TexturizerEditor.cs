using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
namespace PVTG.Editor {
    public class TexturizerEditor : EditorWindow
    {

        static TexturizerEditor instance;

        /*string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;*/

        private List<MeshFilter> _currentMeshes = new List<MeshFilter>();
        private List<MeshRenderer> _currentRenderers = new List<MeshRenderer>();
        private List<WorkObject> _workObjects = new List<WorkObject>();
        private Camera _previewCamera;
        private Vector3 _cameraPivot;
        private RenderTexture _previewTexture;
        private Vector2 _lastScreenDimensions;

        private Vector2 _lastMouse;
        private Vector2 _mouseDelta;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Procedural/PVTG Texturizer")]
        public static void ShowWindow()
        {

            //Show existing window instance. If one doesn't exist, make one.
            instance = (TexturizerEditor)EditorWindow.GetWindow(typeof(TexturizerEditor));
        }

        void OnGUI()
        {

           


            if (instance == null)
            {
                instance = this;
            }

            if (instance._previewCamera == null)
            {
                GameObject cameraObject = new GameObject();
                cameraObject.SetActive(false);
                cameraObject.hideFlags = HideFlags.HideAndDontSave;
                _cameraPivot = new Vector3(0,0,0);
                cameraObject.transform.position = new Vector3(0,0,-10.0f);
                instance._previewCamera = cameraObject.AddComponent<Camera>();
                instance._previewCamera.clearFlags = CameraClearFlags.Color;
                instance._previewCamera.cullingMask = -2147483648;
                instance._previewCamera.backgroundColor = new Color(25 / 255.0f, 25 / 255.0f, 25 / 255.0f, 1.0f);
            }

            //Camera Controls
            bool needsRepaint = MoveCamera();




            titleContent.text = "PVTG Editor";

            float minScale = Mathf.Min(Screen.width, Screen.height);

            if (_previewTexture == null) _previewTexture = new RenderTexture(Screen.width, Screen.height, 1);
            if (new Vector2(Screen.width, Screen.height) != _lastScreenDimensions)
            {
                _previewCamera.targetTexture = null;
                DestroyImmediate(_previewTexture);
                _previewTexture = new RenderTexture(Screen.width, Screen.height, 1);
            }
            _lastScreenDimensions = new Vector2(Screen.width, Screen.height);
            _previewCamera.targetTexture = _previewTexture;



            /*GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            myString = EditorGUILayout.TextField("Text Field", myString);

            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle("Toggle", myBool);
            myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            EditorGUILayout.ColorField(Color.red);
            EditorGUILayout.EndToggleGroup();*/

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _previewTexture, ScaleMode.StretchToFill);

            if (GUI.Button(new Rect(0, 0, minScale * 0.2f, minScale * 0.05f), "Import", EditorStyles.toolbarButton))
            {
                if (Selection.activeGameObject != null) {
                    GameObject obj = Selection.activeGameObject;
                    _currentRenderers.Clear();
                    _currentMeshes.Clear();
                    GetRenderersCascade(obj.transform, _currentRenderers, _currentMeshes);

                    for (int f = 0;f < _currentMeshes.Count;f++)
                    {
                        _workObjects.Add(new WorkObject(_currentMeshes[f].sharedMesh,_currentRenderers[f]));
                    }
                }
            }


            if (GUI.Button(new Rect(minScale * 0.2f, 0, minScale * 0.2f, minScale * 0.05f), "Settings", EditorStyles.toolbarButton))
            {

            }

            GUI.Label(new Rect(minScale * 0.4f, 0, Screen.width - minScale * 0.4f, minScale * 0.05f), "", EditorStyles.toolbarButton);


                for (int i = 0; i < _currentMeshes.Count; i++)
            {
                if (_currentRenderers[i] == null)
                {
                    _currentRenderers.Clear();
                    _currentMeshes.Clear();
                    break;
                }
                List<Material> materials = new List<Material>();

                _currentRenderers[i].GetSharedMaterials(materials);
                for (int m = 0; m < materials.Count; m++)
                {
                    Graphics.DrawMesh(_currentMeshes[i].sharedMesh, _currentMeshes[i].transform.position, _currentMeshes[i].transform.rotation, materials[m], 31, _previewCamera);
                }
            }
            _previewCamera.Render();
            if (needsRepaint) {
                this.Repaint();
            }
        }


        bool MoveCamera()
        {
            bool changed = false;
            if (Event.current.type == EventType.ScrollWheel)
            {
                _previewCamera.transform.position = _cameraPivot + (_previewCamera.transform.position - _cameraPivot) * (1 + Event.current.delta.y * 0.01f * Mathf.Log(Vector3.Distance(_cameraPivot, _previewCamera.transform.position)));
                changed = true;
            }


            if (Event.current.type == EventType.MouseDown)
            {
                _mouseDelta = Vector2.zero;
                _lastMouse = Event.current.mousePosition;
            }

            if (Event.current.type == EventType.MouseDrag)
            {
                if (Event.current.button == 2)
                {
                    if (Event.current.shift)
                    {
                        _mouseDelta = _lastMouse - Event.current.mousePosition;
                        _lastMouse = Event.current.mousePosition;
                        Vector3 move = _previewCamera.transform.right * _mouseDelta.x * 0.01f + _previewCamera.transform.up * _mouseDelta.y * -0.01f;
                        _cameraPivot += move;
                        _previewCamera.transform.position += move;
                    }
                    else
                    {
                        _mouseDelta = _lastMouse - Event.current.mousePosition;
                        _lastMouse = Event.current.mousePosition;
                        _previewCamera.transform.RotateAround(_cameraPivot,Vector3.up,-_mouseDelta.x * 0.5f);
                        _previewCamera.transform.RotateAround(_cameraPivot, _previewCamera.transform.right, -_mouseDelta.y * 0.5f);
                    }

                    changed = true;
                }
            }

            return changed;
        }

   



        void GetRenderersCascade(Transform transform, List<MeshRenderer> renderers, List<MeshFilter> filters)
        {

            Component component;
            if (transform.TryGetComponent(typeof(MeshRenderer), out component)) {

                renderers.Add((MeshRenderer)component);
                filters.Add(transform.GetComponent<MeshFilter>());
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                GetRenderersCascade(child, renderers, filters);
            }
        }
    }
}