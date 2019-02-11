using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NodeVariable
{
    
    public abstract void Draw();
}

public class NodeVariableText : NodeVariable
{
    private string value;
    public string Value { get { return value; } }

    public NodeVariableText(string text)
    {
        value = text;
    }

    public override void Draw()
    {
        IMGUIToolsBox.CenterLayoutLabel(value);
    }
}

public class NodeVariableIntSlider : NodeVariable
{
    private String name;
    private int value;
    public int Value { get { return value; } }

    public NodeVariableIntSlider(String propertyName, int defaultValue)
    {
        name = propertyName;
        value = defaultValue;
    }

    public override void Draw()
    {
        IMGUIToolsBox.IntSlider(name, ref value);
    }
}

public class NodeVariableFloatSlider : NodeVariable
{
    private String name;
    private float value;
    public float Value { get { return value; } }

    public NodeVariableFloatSlider(String propertyName, float defaultValue)
    {
        name = propertyName;
        value = defaultValue;
    }

    public override void Draw()
    {
        IMGUIToolsBox.FloatSlider(name, ref value);
    }
}

public class NodeVariablePopup : NodeVariable
{
    private string name;
    private int value;
    public int Value { get { return value; } }
    private string[] array;

    public NodeVariablePopup(string nameVar, string[] arrayVar, int defaultValue)
    {
        name = nameVar;
        value = defaultValue;
        array = arrayVar;
    }

    public override void Draw()
    {
        IMGUIToolsBox.LayoutLabelAndPopup(name, ref value, array);
    }
}

public class NodeVariableNextCombo : NodeVariable
{
    public bool isActive = false;
    private String text;
    
    public ConnectionPoint outPoint;

    public NodeVariableNextCombo(String text, ConnectionPoint outPoint)
    {
        this.outPoint = outPoint;
        this.text = text;
    }

    public override void Draw()
    {
        if (isActive)
        {
            IMGUIToolsBox.NextCombo(text, ref isActive);

            if (!isActive)
            {
                outPoint.isActivated = false;
                outPoint.DeleteParentConnexion();
            }
                
        }
        else
        {
            IMGUIToolsBox.NextCombo(text, ref isActive);

            if (isActive)
                outPoint.isActivated = true;
        }
    }

    public void ShowOutPoint()
    {
        outPoint.isActivated = true;
    }
}