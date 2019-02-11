using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NodeEditorFramework.Utilities;

public class NodeBasedEditor : EditorWindow
{
    private String comboName;
    public String ComboName { get { return comboName; } set { comboName = value; } }

    private List<Node> nodes;
    public List<Node> Nodes { get { return nodes; } }


    public static GUISkin nodeSkin;
    private SaveManager saveManager;
    
    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;

    private static NodeBasedEditor window;

    private GUIStyle inPointStyle;
    private GUIStyle outPointStyle;
    private GUIStyle toolbarStyle;

    private float toolbarHeight = 35f;

    private Vector2 offset;
    private Vector2 drag;

    [MenuItem("Window/Node Based Editor")]
    private static void OpenWindow()
    {
        window = GetWindow<NodeBasedEditor>();
        window.titleContent = new GUIContent("Node Based Editor");
    }

    private void OnEnable()
    {
        GUIScaleUtility.CheckInit();
        nodeSkin = EditorGUIUtility.Load("ComboTool/Node.guiskin") as GUISkin;
        
        if (saveManager == null)
            saveManager = new SaveManager(this);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);

        toolbarStyle = new GUIStyle();
        toolbarStyle.normal.background = EditorGUIUtility.Load("ComboTool/FondToolBar.png") as Texture2D;
    }

    float zoomScale = 1.0f;

    private void OnGUI()
    {
        // ToolBar area
        GUILayout.BeginArea(new Rect(10f, 10f, 180f, toolbarHeight));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
            saveManager.SaveAllNodes();

        if (GUILayout.Button("Load"))
            saveManager.LoadNode();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(200f, 11f, position.width, toolbarHeight));
            GUILayout.BeginHorizontal();
                GUILayout.Label("Combo name : ", GUILayout.MaxWidth(88));
                EditorGUILayout.LabelField(comboName, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(new Vector2(0f, toolbarHeight), new Vector2(position.width, 2)), toolbarStyle);
        GUILayout.EndArea();
        // ToolBar area

        Matrix4x4 oldMatrix = GUI.matrix;

        DrawConnectionLine(Event.current);
        
        Rect graphRect = new Rect(0, toolbarHeight + 2, position.width, position.height);

        var center = graphRect.size / 2f;

        GUIScaleUtility.BeginScale(ref graphRect, center, zoomScale, true, false);

        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        GUI.skin = nodeSkin;
        
        DrawNodes();
        
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();

        GUIScaleUtility.EndScale();
        
        GUILayout.FlexibleSpace();
        zoomScale = EditorGUILayout.Slider("zoom", zoomScale, 1.0f / 25.0f, 2.0f);

        var e = Event.current;
        if (e.type == EventType.ScrollWheel)
        {
            var zoomDelta = 0.1f;
            zoomDelta = e.delta.y < 0 ? -zoomDelta : zoomDelta;
            zoomScale += zoomDelta;
            zoomScale = Mathf.Clamp(zoomScale, 0.25f, 2f);
            e.Use();
        }
        else if (e.type == EventType.MouseDown && e.button == 2)
        {
            zoomScale = 1f;
            e.Use();
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
        
        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                //if (e.button == 0)
                //{
                //    ClearConnectionSelection();
                //}

                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    private void OnClickAddNode(Vector2 mousePosition)
    {
        if (nodes == null)
        {
            nodes = new List<Node>();
        }

        System.Guid guid = System.Guid.NewGuid();
        String id = guid.ToString();

        nodes.Add(new Node(mousePosition, 350, 280, "DefaultNode", id, new DataClass(), inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
    }

    private void OnClickRemoveNode(Node node)
    {
        nodes.Remove(node);
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center + new Vector2(0f, 40f),
                e.mousePosition,
                selectedInPoint.rect.center + new Vector2(0f, 40f) + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center + new Vector2(0f, 40f),
                e.mousePosition,
                selectedOutPoint.rect.center + new Vector2(0f, 40f) - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private void CreateConnection()
    {
        selectedOutPoint.node.AddConnexion(selectedInPoint, selectedOutPoint);
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    #region public

    /// <summary>
    /// Use to cread node from a load
    /// </summary>
    public void AddNode(Vector2 nodePosition, string name, string id, DataClass data)
    {
        if (nodes == null)
            nodes = new List<Node>();

        nodes.Add(new Node(nodePosition, 350, 280, name, id, data, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
    }

    public void ResetNodes()
    {
        if (nodes != null)
            nodes.Clear();
    }

    public Node GetNodeByID(String nodeID)
    {
        return nodes.Find(x => x.id == nodeID);
    }

    #endregion // public
}