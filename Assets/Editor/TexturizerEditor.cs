using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;
namespace PVTG.Editor {
    public class TexturizerEditor : EditorWindow
    {

        public static TexturizerEditor instance;

        /*string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;*/

        private List<WorkObject> _workObjects = new List<WorkObject>();
        Dictionary<int, WorkObject> _selectables = new Dictionary<int, WorkObject>();
        private TexturePreviewWindow _previewElement;
        private TexturizerGraphView _graphView;
        private WorkObject _selectedObject;




        // Add menu item named "My Window" to the Window menu
        [MenuItem("Procedural/PVTG Editor")]
        public static void ShowWindow()
        {
            
            //Show existing window instance. If one doesn't exist, make one.
            instance = (TexturizerEditor)EditorWindow.GetWindow(typeof(TexturizerEditor));
            

        }

        public void ToggleSelectorColliders(bool active)
        {
            foreach (WorkObject workObject in _workObjects)
            {
                workObject.ToggleCollision(active);
            }   
        }


        private void OnDisable()
        {
            foreach (WorkObject workObject in _workObjects)
            {
                workObject.Destroy();
            }
            _workObjects.Clear();

        }


        public void SelectWorkObject(int colliderID)
        {
            if (_selectables.ContainsKey(colliderID))
            {
                if (_selectedObject != null && _selectedObject.colliderIDs.Contains(colliderID))
                {
                    _selectedObject = null;
                }
                else
                {
                    _selectedObject = _selectables[colliderID];
                }
            }
            else
            {
                DeselectWorkObject();
            }
        }
        public void DeselectWorkObject() {
            _selectedObject = null;
         }

        private void OnEnable()
        {

            if (instance == null)
            {
                instance = this;
                instance._graphView = null;
            }

            titleContent.text = "PVTG Editor";


            if (_graphView == null)
            {

                TexturizerNode.FindNodes();
                _graphView = new TexturizerGraphView { name = "New Graph" };
                rootVisualElement.Add(_graphView);
                _graphView.StretchToParentSize();

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
                int idk = 0;
                Toolbar ToolBar = new Toolbar();
                rootVisualElement.Add(ToolBar);
                ToolbarMenu ToolBarMenu = new ToolbarMenu();
                ToolBarMenu.text = "Import";
                ToolBarMenu.menu.AppendAction("From Selection", a => { idk++; OnImport(); }, a => DropdownMenuAction.Status.Normal);
                ToolBar.Add(ToolBarMenu);

            }
        }




        public void DrawWorkObjects(Camera camera)
        {
            if (camera != null)
            {
                for (int i = 0; i < _workObjects.Count; i++)
                {
                    WorkObject workObject = _workObjects[i];
                    workObject.Render(camera, 31);
                }
            }
            camera.Render();
        }
        public bool DrawSelectedObject(Camera camera)
        {
            if (camera != null)
            {
                if (_selectedObject != null)
                {
                    _selectedObject.Render(camera, 31);
                    camera.Render();
                    return true;
                }
            }
            return false;
        }


        void OnImport()
        {
            
            if (Selection.activeGameObject != null)
            {

                foreach (WorkObject workObject in _workObjects)
                {
                    workObject.Destroy();
                }
                _workObjects.Clear();
                _selectables.Clear();
                foreach (GameObject selectedObject in Selection.gameObjects)
                {
                    GetRenderersCascade(selectedObject.transform, _workObjects);
                }
                foreach (WorkObject workObject in _workObjects)
                {
                    foreach (int id in workObject.colliderIDs)
                    {
                        _selectables.Add(id, workObject);
                    }
                }
            }
            _previewElement.ReDraw();
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