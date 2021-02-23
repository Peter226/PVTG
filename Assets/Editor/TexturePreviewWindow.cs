using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;


namespace PVTG.Editor
{
    public class TexturePreviewWindow : Image
    {

        private Camera _previewCamera;
        private Camera _selectionCamera;
        private Vector3 _cameraPivot;
        private RenderTexture _previewTexture;
        private RenderTexture _selectionBuffer;
        private RenderTexture _selectionPing;
        private RenderTexture _selectionPong;
        private ComputeShader _selectionHighlight;
        private ComputeShader _selectionBlur;
        private ComputeShader _selectionAdd;

        public TexturePreviewWindow()
        {
            RegisterCallback<DetachFromPanelEvent>(e => { RemovePreview(); });
            RegisterCallback<AttachToPanelEvent>(e => { SetupPreview(); });
            RegisterCallback<MouseMoveEvent>(e => { MouseMoved(e); });
            RegisterCallback<GeometryChangedEvent>(e => { StyleChanged(); });
            RegisterCallback<WheelEvent>(e => { MouseScrolled(e); });
            RegisterCallback<MouseDownEvent>(e => { MouseDown(e); });
        }


        void ResizeTexture(RenderTexture texture)
        {
            texture.Release();
            texture.width = (int)this.contentRect.width;
            texture.height = (int)this.contentRect.height;
            texture.enableRandomWrite = true;
            texture.Create();
        }


        public void ReDraw()
        {
            if (TexturizerEditor.instance != null)
            {
                ResizeTexture(_previewTexture);
                ResizeTexture(_selectionBuffer);
                ResizeTexture(_selectionPing);
                ResizeTexture(_selectionPong);
                _previewCamera.targetTexture = _previewTexture;
                _selectionCamera.targetTexture = _selectionBuffer;
                TexturizerEditor.instance.DrawWorkObjects(_previewCamera);
                if(TexturizerEditor.instance.DrawSelectedObject(_selectionCamera)) {
                    int kernelHandle = _selectionHighlight.FindKernel("CSMain");
                    _selectionHighlight.SetTexture(kernelHandle, "Result", _selectionPing);
                    _selectionHighlight.SetTexture(kernelHandle, "Input", _selectionBuffer);
                    _selectionHighlight.Dispatch(0, _selectionBuffer.width / 8, _selectionBuffer.height / 8, 1);

                   

                    for (int i = 0;i < 3;i++)
                    {
                        int blurHandle = _selectionBlur.FindKernel("CSMain");
                        _selectionBlur.SetTexture(blurHandle, "Result", _selectionPong);
                        _selectionBlur.SetTexture(blurHandle, "Input", _selectionPing);
                        _selectionBlur.Dispatch(0, _selectionPong.width / 8, _selectionPong.height / 8, 1);

                        _selectionBlur.SetTexture(blurHandle, "Result", _selectionPing);
                        _selectionBlur.SetTexture(blurHandle, "Input", _selectionPong);
                        _selectionBlur.Dispatch(0, _selectionPong.width / 8, _selectionPong.height / 8, 1);
                    }

                    int addHandle = _selectionAdd.FindKernel("CSMain");
                    _selectionAdd.SetTexture(addHandle, "Result", _selectionPong);
                    _selectionAdd.SetTexture(addHandle, "Albedo", _previewTexture);
                    _selectionAdd.SetTexture(addHandle, "Solid", _selectionBuffer);
                    _selectionAdd.SetTexture(addHandle, "Input", _selectionPing);
                    _selectionAdd.Dispatch(0, _previewTexture.width / 8, _previewTexture.height / 8, 1);
                    this.image = _selectionPong;
                }
                else
                {
                    this.image = _previewTexture;
                }
                //this.image = _previewTexture;
                //DEBUG
                
            }
            this.MarkDirtyRepaint();
        }



        void StyleChanged()
        {
            ReDraw();
        }

        void MouseDown(MouseDownEvent e)
        {
            if (e.button == 1) {
                Vector3 mousePosition = new Vector3(e.mousePosition.x - worldBound.x, sourceRect.height - (e.mousePosition.y - worldBound.y),0);
                Ray ray = _previewCamera.ScreenPointToRay(mousePosition);
                RaycastHit hit;
                TexturizerEditor.instance.ToggleSelectorColliders(true);
                if (Physics.Raycast(ray,out hit,int.MaxValue,layerMask: -2147483648))
                {
                    TexturizerEditor.instance.SelectWorkObject(hit.collider.gameObject.GetInstanceID());
                }
                else
                {
                    TexturizerEditor.instance.DeselectWorkObject();
                }
                TexturizerEditor.instance.ToggleSelectorColliders(false);
            }
            ReDraw();
        }

        void MouseScrolled(WheelEvent e)
        {
            _previewCamera.transform.position = _cameraPivot + (_previewCamera.transform.position - _cameraPivot) * (1 + e.delta.y * 0.01f * Mathf.Log(Vector3.Distance(_cameraPivot, _previewCamera.transform.position)));
            ReDraw();
        }

        void MouseMoved(MouseMoveEvent e)
        {
            float minScale = 1 / Mathf.Min(contentRect.width,contentRect.height) * 1000.0f;

            if ((e.pressedButtons & 4) > 0)
            {
                
                if (e.shiftKey)
                {
                    Vector3 move = _previewCamera.transform.right * e.mouseDelta.x * 0.01f * minScale + _previewCamera.transform.up * e.mouseDelta.y * -0.01f * minScale;
                    _cameraPivot -= move;
                    _previewCamera.transform.position -= move;
                }
                else
                {
                    _previewCamera.transform.RotateAround(_cameraPivot, Vector3.up, e.mouseDelta.x * minScale * 0.2f);
                    _previewCamera.transform.RotateAround(_cameraPivot, _previewCamera.transform.right, e.mouseDelta.y * minScale * 0.2f);
                }
                ReDraw();
            }
        }


        void SetupPreview()
        {
            _selectionHighlight = Resources.Load<ComputeShader>("selectionOutline");
            _selectionBlur = Resources.Load<ComputeShader>("selectionBlur");
            _selectionAdd = Resources.Load<ComputeShader>("selectionCombine");
            GameObject cameraObject = new GameObject("previewCam");
            GameObject selectionCameraObject = new GameObject("selectionCam");
            selectionCameraObject.transform.parent = cameraObject.transform;
            cameraObject.SetActive(false);
            selectionCameraObject.SetActive(false);
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            selectionCameraObject.hideFlags = HideFlags.HideAndDontSave;
            _cameraPivot = new Vector3(0, 0, 0);
            cameraObject.transform.position = new Vector3(0, 0, -10.0f);
            _previewCamera = cameraObject.AddComponent<Camera>();
            _selectionCamera = selectionCameraObject.AddComponent<Camera>();
            _previewCamera.clearFlags = CameraClearFlags.Color;
            _selectionCamera.clearFlags = CameraClearFlags.Color;
            _previewCamera.cullingMask = -2147483648;
            _selectionCamera.cullingMask = -2147483648;
            _previewCamera.backgroundColor = new Color(25 / 255.0f, 25 / 255.0f, 25 / 255.0f, 1.0f);
            _selectionCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            _previewTexture = new RenderTexture(100, 100, 1,RenderTextureFormat.DefaultHDR);
            _selectionPing = new RenderTexture(100, 100, 1, RenderTextureFormat.DefaultHDR);
            _selectionPong = new RenderTexture(100,100,1, RenderTextureFormat.DefaultHDR);
            _selectionBuffer = new RenderTexture(100,100,1, RenderTextureFormat.DefaultHDR);

            _selectionPing.enableRandomWrite = true;
            _selectionBuffer.enableRandomWrite = true;
            _previewTexture.enableRandomWrite = true;
            _selectionPong.enableRandomWrite = true;

            _selectionBuffer.useDynamicScale = true;
            _previewTexture.useDynamicScale = true;
            _selectionPing.useDynamicScale = true;
            _selectionPong.useDynamicScale = true;
            //image = _previewTexture;
            //DEBUG
            image = _selectionPing;
            _previewCamera.targetTexture = _previewTexture;
            _previewCamera.Render();
            _selectionCamera.targetTexture = _selectionPing;
            _selectionCamera.Render();
        }

        void RemovePreview()
        {
            Object.DestroyImmediate(_previewCamera.gameObject);
            Object.DestroyImmediate(_previewTexture);
            Object.DestroyImmediate(_selectionPing);
            Object.DestroyImmediate(_selectionPong);
            Object.DestroyImmediate(_selectionBuffer);
        }


    }

}