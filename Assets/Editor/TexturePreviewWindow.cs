using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;


namespace PVTG.Editor
{
    public class TexturePreviewWindow : Image
    {

        public CameraHolder cameraHolder = new CameraHolder();
        private Vector3 _cameraPivot;
        private RenderTexture _previewTexture;



        public TexturePreviewWindow()
        {
            RegisterCallback<DetachFromPanelEvent>(e => { RemovePreview(); });
            RegisterCallback<AttachToPanelEvent>(e => { SetupPreview(); });
            RegisterCallback<MouseMoveEvent>(e => { MouseMoved(e); });
            


        }




        void MouseMoved(MouseMoveEvent e)
        {
            TexturizerEditor.instance.DrawWorkObjects();

            bool changed = false;
            
            /*if (e.imguiEvent.type == EventType.ScrollWheel)
            {
                cameraHolder.camera.transform.position = _cameraPivot + (cameraHolder.camera.transform.position - _cameraPivot) * (1 + e.imguiEvent.delta.y * 0.01f * Mathf.Log(Vector3.Distance(_cameraPivot, cameraHolder.camera.transform.position)));
                changed = true;
            }*/
            

           /* if (Event.current.type == EventType.MouseDown)
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
                        _previewCamera.transform.RotateAround(_cameraPivot, Vector3.up, -_mouseDelta.x * 0.5f);
                        _previewCamera.transform.RotateAround(_cameraPivot, _previewCamera.transform.right, -_mouseDelta.y * 0.5f);
                    }

                    changed = true;
                }
            }*/
        }


        void SetupPreview()
        {
            GameObject cameraObject = new GameObject("previewCam");
            cameraObject.SetActive(false);
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            _cameraPivot = new Vector3(0, 0, 0);
            cameraObject.transform.position = new Vector3(0, 0, -10.0f);
            cameraHolder.camera = cameraObject.AddComponent<Camera>();
            cameraHolder.camera.clearFlags = CameraClearFlags.Color;
            cameraHolder.camera.cullingMask = -2147483648;
            cameraHolder.camera.backgroundColor = new Color(25 / 255.0f, 25 / 255.0f, 25 / 255.0f, 1.0f);
            _previewTexture = new RenderTexture(100, 100, 1);
            image = _previewTexture;
            cameraHolder.camera.targetTexture = _previewTexture;
            cameraHolder.camera.Render();
        }

        void RemovePreview()
        {
            cameraHolder.Destroy();

            Object.DestroyImmediate(_previewTexture);
        }


    }



    public class CameraHolder
    {
        public Camera camera;


        public void Destroy()
        {
            if (camera != null)
            {
                Object.DestroyImmediate(camera);
            }
        }
    }
}