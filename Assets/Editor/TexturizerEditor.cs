using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace PVTG.Editor {
    public class TexturizerEditor : EditorWindow
    {

        public static TexturizerEditor instance;

        /*string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;*/

        private List<WorkObject> _workObjects = new List<WorkObject>();
       
        private TexturePreviewWindow _previewElement;
        private Vector2 _lastScreenDimensions;

        private Vector2 _lastMouse;
        private Vector2 _mouseDelta;
        private TexturizerGraphView _graphView;
        private Camera _previewCam;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Procedural/PVTG Texturizer")]
        public static void ShowWindow()
        {
            
            //Show existing window instance. If one doesn't exist, make one.
            instance = (TexturizerEditor)EditorWindow.GetWindow(typeof(TexturizerEditor));
            

        }

        private void OnEnable()
        {

        }




        private void OnGUI()
        {


           



           // float width = EditorGUIUtility.currentViewWidth;

            if (instance == null)
            {
                instance = this;
                instance._graphView = null;
            }

          


            Rect screenDims = new Rect(0, 20, EditorGUIUtility.currentViewWidth - 1, Screen.height - Screen.height * 0.2f - 41);

            if (_graphView == null)
            {

                TexturizerNode.FindNodes();
                _graphView = new TexturizerGraphView { name = "New Graph" };
                rootVisualElement.Add(_graphView);
                _graphView.StretchToParentSize();



              
                Button importer = new Button();
                importer.text = "Import";
                rootVisualElement.Add(importer);
                importer.style.marginTop = 0;
                importer.style.marginLeft = 0;
                importer.style.height = 20;
                importer.style.width = 100;
                importer.clicked += OnImport;
                importer.style.borderBottomLeftRadius = 0;
                importer.style.borderBottomRightRadius = 0;
                importer.style.borderTopLeftRadius = 0;
                importer.style.borderTopRightRadius = 0;
                Label label = new Label();
                rootVisualElement.Add(label);
                label.StretchToParentWidth();
                label.style.height = 20;
                label.style.marginLeft = 0;
                label.style.marginTop = 0;
                label.style.position = Position.Absolute;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.backgroundColor = (Color)new Color32(88,88,88,255);
                label.BringToFront();
                importer.BringToFront();


                //TexturizerNode.patterns


                VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("TexturizerGraph");
                VisualElement ui = uiAsset.CloneTree("");

                foreach (VisualElement ve in ui.Children())
                {
                    if (ve.name == "PreviewWindow")
                    {
                        ResizableElement resizer = new ResizableElement();
                        ve.Add(resizer);
                        resizer.BringToFront();
                        ve.AddManipulator(new Dragger());

                        foreach (VisualElement sve in ve.Children())
                        {
                            if (sve.name == "PreviewImage")
                            {
                                _previewElement = new TexturePreviewWindow();
                                sve.Add(_previewElement);
                                _previewElement.image = _previewElement.image;
                                _previewElement.scaleMode = ScaleMode.StretchToFill;
                                _previewElement.StretchToParentSize();
                            }
                        }
                    }
                }
                



                rootVisualElement.Add(ui);


            }
            

            titleContent.text = "PVTG Editor";

            float minScale = Mathf.Min(Screen.width, Screen.height);


        }


        /*  bool MoveCamera()
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

     */



        public void DrawWorkObjects()
        {
            if (_previewElement.cameraHolder.camera != null)
            {
                for (int i = 0; i < _workObjects.Count; i++)
                {
                    WorkObject workObject = _workObjects[i];
                    workObject.Render(_previewElement.cameraHolder.camera, 31);
                }


            }


            _previewElement.cameraHolder.camera.Render();
            rootVisualElement.MarkDirtyRepaint();
        }


        void OnImport()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject obj = Selection.activeGameObject;
                foreach (WorkObject workObject in _workObjects)
                {
                    workObject.Destroy();
                }
                _workObjects.Clear();
                GetRenderersCascade(obj.transform, _workObjects);
            }
            DrawWorkObjects();
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