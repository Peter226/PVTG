using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace PVTG {
    public class TexturizerNode : Node
    {
        public static Dictionary<string,ComputePattern> patterns = new Dictionary<string,ComputePattern>();
        private ComputePattern _pattern;
        List<Port> ports = new List<Port>();
        RenderTexture previewTexture = new RenderTexture(128,128,1);

        public static void FindNodes()
        {
            patterns.Clear();
            TextAsset[] texts = Resources.LoadAll<TextAsset>("Nodes");
            for (int t = 0; t < texts.Length; t++)
            {
                TextAsset text = texts[t];
                ComputePattern computePattern = new ComputePattern(text.text.Substring(0, text.text.IndexOf('\n') - 1), text.text);
                patterns.Add(computePattern.shader.name,computePattern);
                Resources.UnloadAsset(text);
            }
        }

        public TexturizerNode(ComputePattern pattern)
        {
            this.RegisterCallback<DetachFromPanelEvent>(e => { RemovedNode(); });
            
            title = pattern.shader.name;
            _pattern = pattern;
            foreach (ComputeProperty computeProperty in _pattern.computeProperties)
            {
                Direction direction = Direction.Input;
                Port.Capacity capacity = Port.Capacity.Single;
                VisualElement container = inputContainer;
                if (computeProperty.isOutput) {
                    direction = Direction.Output;
                    capacity = Port.Capacity.Multi;
                    container = outputContainer;
                }
                Port p = InstantiatePort(Orientation.Horizontal, direction, capacity, computeProperty.type);
                p.portName = computeProperty.name;
                container.Add(p);
                ports.Add(p);
            }
            RefreshPorts();
            Image image = new Image();
            image.image = previewTexture;
            this.extensionContainer.Add(image);
            RefreshExpandedState();
            
        }

        void RemovedNode()
        {
            Debug.Log("Removed");
            previewTexture.Release();
        }


    }
}