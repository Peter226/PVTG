using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace PVTG
{
    public class TexturizerGraphView : GraphView
    {
        public TexturizerGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("TexturizerGraph"));

            SetupZoom(ContentZoomer.DefaultMinScale * 0.03f, ContentZoomer.DefaultMaxScale * 10.0f);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            

            var grid = new GridBackground();
            
            Insert(0, grid);
            grid.StretchToParentSize();



            TexturizerNode node = new TexturizerNode(TexturizerNode.patterns["cs_multiply"]);
            node.SetPosition(new Rect(10, 10, 500, 500));
            AddElement(node);

            TexturizerNode node2 = new TexturizerNode(TexturizerNode.patterns["cs_multiply"]);
            node2.SetPosition(new Rect(10, 30, 500, 500));
            AddElement(node2);

            TexturizerNode node3 = new TexturizerNode(TexturizerNode.patterns["cs_multiply"]);
            node3.SetPosition(new Rect(10, 60, 500, 500));
            AddElement(node3);

            TexturizerNode node4 = new TexturizerNode(TexturizerNode.patterns["cs_multiply"]);
            node4.SetPosition(new Rect(10, 60, 500, 500));
            AddElement(node4);

            TexturizerNode node5 = new TexturizerNode(TexturizerNode.patterns["cs_multiply"]);
            node5.SetPosition(new Rect(10, 80, 500, 500));
            AddElement(node5);
        }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach( port => {
                if (port.node != startPort.node && port.portType == startPort.portType && port.direction != startPort.direction)
                {
                    bool recursion = false;
                    ((TexturizerNode)port.node).HuntRecursion(startPort.node, ref recursion, startPort.direction);
                    if (!recursion) compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }





    }
}