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
            RegisterCallback<GeometryChangedEvent>(e => { StyleChanged(); });
            RegisterCallback<WheelEvent>(e => { MouseScrolled(e); });
        }

        void ReDraw()
        {
            _previewTexture.Release();
            _previewTexture.width = (int)this.contentRect.width;
            _previewTexture.height = (int)this.contentRect.height;
            _previewTexture.Create();
            cameraHolder.camera.targetTexture = _previewTexture;
            this.image = _previewTexture;
            if (TexturizerEditor.instance != null) {
                TexturizerEditor.instance.DrawWorkObjects();
            }
        }



        void StyleChanged()
        {
            ReDraw();
        }
        void MouseScrolled(WheelEvent e)
        {
            cameraHolder.camera.transform.position = _cameraPivot + (cameraHolder.camera.transform.position - _cameraPivot) * (1 + e.delta.y * 0.01f * Mathf.Log(Vector3.Distance(_cameraPivot, cameraHolder.camera.transform.position)));
            ReDraw();
        }

        void MouseMoved(MouseMoveEvent e)
        {
            TexturizerEditor.instance.DrawWorkObjects();

            float minScale = 1 / Mathf.Min(contentRect.width,contentRect.height) * 1000.0f;

            if ((e.pressedButtons & 4) > 0)
            {

                if (e.shiftKey)
                {
                    Vector3 move = cameraHolder.camera.transform.right * e.mouseDelta.x * 0.01f * minScale + cameraHolder.camera.transform.up * e.mouseDelta.y * -0.01f * minScale;
                    _cameraPivot -= move;
                    cameraHolder.camera.transform.position -= move;
                }
                else
                {
                    cameraHolder.camera.transform.RotateAround(_cameraPivot, Vector3.up, e.mouseDelta.x * minScale * 0.2f);
                    cameraHolder.camera.transform.RotateAround(_cameraPivot, cameraHolder.camera.transform.right, e.mouseDelta.y * minScale * 0.2f);
                }
                ReDraw();
            }
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
            _previewTexture.useDynamicScale = true;
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