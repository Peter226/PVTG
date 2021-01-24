using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class TexturizerEditor : EditorWindow
{

    static TexturizerEditor instance;

    /*string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;*/

    private List<MeshFilter> _currentMeshes = new List<MeshFilter>();
    private List<MeshRenderer> _currentRenderers = new List<MeshRenderer>();
    private Camera _previewCamera;
    private RenderTexture _previewTexture;
    private Vector2 _lastScreenDimensions;



    

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Procedural/PVTG Texturizer")]
    public static void ShowWindow()
    {
        
        //Show existing window instance. If one doesn't exist, make one.
        instance = (TexturizerEditor)EditorWindow.GetWindow(typeof(TexturizerEditor));
        if (instance._previewCamera == null)
        {
            GameObject cameraObject = new GameObject();
            cameraObject.SetActive(false);
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            instance._previewCamera = cameraObject.AddComponent<Camera>();
            instance._previewCamera.clearFlags = CameraClearFlags.Color;
            instance._previewCamera.cullingMask = 0;
            instance._previewCamera.backgroundColor = new Color(25 / 255.0f, 25 / 255.0f, 25 / 255.0f,1.0f );
        }

        
    }

    void OnGUI()
    {
        titleContent.text = "PVTG Editor";

        float minScale = Mathf.Min(Screen.width,Screen.height);

        if (_previewTexture == null) _previewTexture = new RenderTexture(Screen.width,Screen.height,1);
        if (new Vector2(Screen.width,Screen.height) != _lastScreenDimensions)
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

        GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height),_previewTexture,ScaleMode.StretchToFill);
        
        if (GUI.Button(new Rect(0, 0, minScale * 0.2f, minScale * 0.05f), "Import", EditorStyles.toolbarButton))
        {
            if (Selection.activeGameObject != null) {
                GameObject obj = Selection.activeGameObject;
                _currentRenderers.Clear();
                _currentMeshes.Clear();
                GetRenderersCascade(obj.transform, _currentRenderers, _currentMeshes);
            }
        }




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
                Graphics.DrawMesh(_currentMeshes[i].sharedMesh, _currentMeshes[i].transform.position, _currentMeshes[i].transform.rotation, materials[m], 1, _previewCamera);
                Debug.Log("Drawn Mesh");
            }
        }
        _previewCamera.Render();
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
