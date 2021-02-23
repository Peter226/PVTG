using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace PVTG {
    public class TexturizerNode : Node, IEdgeConnectorListener
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
                p.AddManipulator(new EdgeConnector<Edge>(this));
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
            previewTexture.Release();
        }

        public void AlertChange()
        {
            Debug.Log("AlertChange");
            foreach (Port port in ports)
            {
                if (port.direction == Direction.Output)
                {
                    foreach (Edge edge in port.connections)
                    {
                        if (edge.output.direction == Direction.Input)
                            ((TexturizerNode)edge.output.node).AlertChange();
                        if (edge.input.direction == Direction.Input)
                            ((TexturizerNode)edge.input.node).AlertChange();
                    }
                }
            }
        }
            
        public void FindRecursion(Node origin, ref bool found, Direction direction)
        {
            if (this != origin)
            {
                foreach (Port port in ports)
                {
                    if (port.direction == direction)
                    {
                        foreach (Edge edge in port.connections)
                        {
                            if (edge.input.direction != direction)
                            {
                                ((TexturizerNode)edge.input.node).FindRecursion(origin, ref found, direction);
                            }
                            if (edge.output.direction != direction)
                            {
                                ((TexturizerNode)edge.output.node).FindRecursion(origin, ref found, direction);
                            }
                        }
                    }
                }
            }
            else
            {
                found = true;
            }
        }


        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            Debug.Log("Dropped Outside");
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            if (edge.input.direction == Direction.Input)
            {
                ((TexturizerNode)edge.input.node).AlertChange();
            }
            if (edge.output.direction == Direction.Input)
            {
                ((TexturizerNode)edge.output.node).AlertChange();
            }
        }
    }
}