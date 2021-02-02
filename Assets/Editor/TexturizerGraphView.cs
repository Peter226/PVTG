using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


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
        Insert(0,grid);
        grid.StretchToParentSize();

        }





    }
