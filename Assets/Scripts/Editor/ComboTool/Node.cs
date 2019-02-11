using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class Node
{
    public Rect rect;
    public string title;
    public bool isDragged;
    public bool isSelected;
    public String id;

    private bool textFiled = false;

    public List<Connection> connections;
    
    public ConnectionPoint nodeInPoint;

    public ConnectionPoint lightOutPoint;
    public ConnectionPoint strongOutPoint;
    public ConnectionPoint specialOutPoint;
    public ConnectionPoint jumpOutPoint;

    public GUIStyle style;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;
    
    private string[] inputNames = { "None", "Light", "Strong", "Special", "Jump" };
    
    public Texture nodeVariableBackgroundTexture;

    private List<NodeVariable> nodeVariableList;
    public List<NodeVariable> NodeVariableList { get { return nodeVariableList; } }

    public NodeVariablePopup inputDropdown;
    public NodeVariableIntSlider stdDamageSlider;
    public NodeVariableIntSlider stgDamageSlider;
    public NodeVariableIntSlider aglDamageSlider;
    public NodeVariableFloatSlider lifeTimeSlider;
    public NodeVariableFloatSlider recoveryTimeSlider;
    
    public NodeVariableNextCombo lightVaraible;
    public NodeVariableNextCombo strongVaraible;
    public NodeVariableNextCombo specialVaraible;
    public NodeVariableNextCombo jumpVaraible;

    private List<Type> typeList;
    private List<String> nameList;

    public Action<Node> OnRemoveNode;

    public DataClass dataClass;

    GUIContent content;
    
    public Node(Vector2 position, float width, float height, string name, string id, DataClass dataClass, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode)
    {
        title = name;
        rect = new Rect(position.x, position.y, width, height);
        this.id = id;
        style = NodeBasedEditor.nodeSkin.box;
        nodeInPoint = new ConnectionPoint(this, new Vector2(0f, 150f), ConnectionPointType.In, inPointStyle, OnClickInPoint);
        nodeInPoint.isActivated = true;
        lightOutPoint = new ConnectionPoint(this, new Vector2(0f, 264f), ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        strongOutPoint = new ConnectionPoint(this, new Vector2(0f, 297f), ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        specialOutPoint = new ConnectionPoint(this, new Vector2(0f, 330f), ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        jumpOutPoint = new ConnectionPoint(this, new Vector2(0f, 363f), ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        defaultNodeStyle = NodeBasedEditor.nodeSkin.box;
        selectedNodeStyle = NodeBasedEditor.nodeSkin.customStyles[0];
        OnRemoveNode = OnClickRemoveNode;

        nodeVariableList = new List<NodeVariable>();
        
        inputDropdown = new NodeVariablePopup("Controlls", inputNames, (int)dataClass.input);
        nodeVariableList.Add(inputDropdown);

        stdDamageSlider = new NodeVariableIntSlider("Standard Damage", dataClass.standardDamage);
        nodeVariableList.Add(stdDamageSlider);
        stgDamageSlider = new NodeVariableIntSlider("Strong Damage", dataClass.strongDamage);
        nodeVariableList.Add(stgDamageSlider);
        aglDamageSlider = new NodeVariableIntSlider("Agile Damage", dataClass.agileDamage);
        nodeVariableList.Add(aglDamageSlider);

        lifeTimeSlider = new NodeVariableFloatSlider("Life Time", dataClass.lifeTime);
        nodeVariableList.Add(lifeTimeSlider);
        recoveryTimeSlider = new NodeVariableFloatSlider("Recovery Time", dataClass.recoveryTime);
        nodeVariableList.Add(recoveryTimeSlider);
        
        lightVaraible = new NodeVariableNextCombo("Light Combo", lightOutPoint);
        nodeVariableList.Add(lightVaraible);
        strongVaraible = new NodeVariableNextCombo("Strong Combo", strongOutPoint);
        nodeVariableList.Add(strongVaraible);
        specialVaraible = new NodeVariableNextCombo("Special Combo", specialOutPoint);
        nodeVariableList.Add(specialVaraible);
        jumpVaraible = new NodeVariableNextCombo("Jump Combo", jumpOutPoint);
        nodeVariableList.Add(jumpVaraible);


        nodeVariableBackgroundTexture = (Texture)EditorGUIUtility.Load("ComboTool/FondNodeVariable.png");
    }
    
    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }
    
    public void Draw()
    {
        nodeInPoint.Draw();
        lightOutPoint.Draw();
        strongOutPoint.Draw();
        specialOutPoint.Draw();
        jumpOutPoint.Draw();
        DrawConnections();

        // Main Rect
        GUI.Box(new Rect(rect.x, rect.y, rect.width, 60f), "", !isSelected ? NodeBasedEditor.nodeSkin.customStyles[3] : NodeBasedEditor.nodeSkin.customStyles[4]);
        GUI.Box(new Rect(rect.x, rect.y + 60f, rect.width, IMGUIToolsBox.layoutHeight * nodeVariableList.Count), "", !isSelected ? NodeBasedEditor.nodeSkin.customStyles[5] : NodeBasedEditor.nodeSkin.customStyles[6]);
        GUI.Box(new Rect(rect.x, rect.y +  33f * nodeVariableList.Count + 60f, rect.width, 30f), "", !isSelected ? NodeBasedEditor.nodeSkin.customStyles[7] : NodeBasedEditor.nodeSkin.customStyles[8]);

        //Node name
        if (!textFiled)
            IMGUIToolsBox.CenterLabel(new Rect(rect.x, rect.y + 30, rect.width, rect.height), title);
        
        //Node name toggle
        textFiled = GUI.Toggle(new Rect(rect.x + 28f, rect.y + 21f, 20f, 20f), textFiled, "");

        // Node name textfield
        if (textFiled)
            title = GUI.TextField(new Rect(rect.x + 44f, rect.y + 21f, 275f, 34f), title, NodeBasedEditor.nodeSkin.textField);

        // Start Content Area
        GUILayout.BeginArea(new Rect(rect.x + 27f, rect.y + 60f, rect.width - 54f, rect.height + 50f));
        
        foreach (var t1 in nodeVariableList)
            t1.Draw();

        // End Content Area
        GUILayout.EndArea();
    }
    
    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            OnRemoveNode(this);
        }
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
    }

    public void AddConnexion(ConnectionPoint selectedInPoint, ConnectionPoint selectedOutPoint)
    {
        if (connections == null)
            connections = new List<Connection>();

        connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
    }

}