using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace PVTG.Editor {
    public class TexturizerEditor : EditorWindow
    {

        static TexturizerEditor instance;

        /*string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;*/

        private List<WorkObject> _workObjects = new List<WorkObject>();
        private Camera _previewCamera;
        private Vector3 _cameraPivot;
        private RenderTexture _previewTexture;
        private Vector2 _lastScreenDimensions;

        private Vector2 _lastMouse;
        private Vector2 _mouseDelta;
        private TexturizerGraphView _graphView;


        // Add menu item named "My Window" to the Window menu
        [MenuItem("Procedural/PVTG Texturizer")]
        public static void ShowWindow()
        {

            //Show existing window instance. If one doesn't exist, make one.
            instance = (TexturizerEditor)EditorWindow.GetWindow(typeof(TexturizerEditor));
            

        }

        void OnGUI()
        {


           



           // float width = EditorGUIUtility.currentViewWidth;

            if (instance == null)
            {
                instance = this;
                instance._graphView = null;
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

            Rect screenDims = new Rect(0, 20, EditorGUIUtility.currentViewWidth - 1, Screen.height - Screen.height * 0.2f - 41);
            if (_graphView == null)
            {
                _graphView = new TexturizerGraphView { name = "New Graph" };
                rootVisualElement.Add(_graphView);
                _graphView.StretchToParentSize();

                Image image = new Image();
                image.image = _previewTexture;
               // rootVisualElement.Add(image);
               
               
                image.BringToFront();
            }


            titleContent.text = "PVTG Editor";

            float minScale = Mathf.Min(Screen.width, Screen.height);

            if (_previewTexture == null) _previewTexture = new RenderTexture((int)(Mathf.RoundToInt(screenDims.width)), Mathf.RoundToInt(screenDims.height), 1);
            if (new Vector2(Screen.width, Screen.height) != _lastScreenDimensions)
            {
                _previewCamera.targetTexture = null;
                DestroyImmediate(_previewTexture);
                _previewTexture = new RenderTexture((int)(Mathf.RoundToInt(screenDims.width)), Mathf.RoundToInt(screenDims.height), 1);
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

            
            GUI.DrawTexture(new Rect(screenDims.x, screenDims.y, screenDims.width * 0.5f, screenDims.height), _previewTexture, ScaleMode.StretchToFill,false,0,Color.white,0,20.0f);

            if (GUI.Button(new Rect(0, 0, minScale * 0.2f, minScale * 0.05f), "Import", EditorStyles.toolbarButton))
            {
                if (Selection.activeGameObject != null) {
                    GameObject obj = Selection.activeGameObject;
                    foreach (WorkObject workObject in _workObjects)
                    {
                        workObject.Destroy();
                    }
                    _workObjects.Clear();
                    GetRenderersCascade(obj.transform, _workObjects);
                }
            }


            if (GUI.Button(new Rect(minScale * 0.2f, 0, minScale * 0.2f, minScale * 0.05f), "Settings", EditorStyles.toolbarButton))
            {

            }

            GUI.Label(new Rect(minScale * 0.4f, 0, Screen.width - minScale * 0.4f, minScale * 0.05f), "", EditorStyles.toolbarButton);




            for (int i = 0;i < _workObjects.Count;i++)
            {
                WorkObject workObject = _workObjects[i];
                workObject.Render(_previewCamera,31);
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

   



        void GetRenderersCascade(Transform transform, List<WorkObject> workObjects)
        {
           
            Component component;
            if (transform.TryGetComponent(typeof(MeshRenderer), out component)) {

                workObjects.Add(new WorkObject(transform.GetComponent<MeshFilter>().sharedMesh, (MeshRenderer)component));
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                GetRenderersCascade(child, workObjects);
            }
        }
    }
}