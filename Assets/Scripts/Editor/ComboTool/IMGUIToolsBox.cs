using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class IMGUIToolsBox
{
    private static Texture testTex;
    public static float layoutHeight = 33f;

    public static Texture TesTexture {
        get
        {
            if (testTex == null)
                testTex = EditorGUIUtility.Load("ComboTool/FondNodeVariableTest.png") as Texture2D;
            return testTex;
        }
    }

    public static void CenterLayoutLabel(string text)
    {
        GUILayout.BeginHorizontal(NodeBasedEditor.nodeSkin.customStyles[2]);
            GUILayout.FlexibleSpace();
               GUILayout.Label(text, GUILayout.Height(layoutHeight));
            GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public static void CenterLabel(Rect rec, string text)
    {
        GUILayout.BeginArea(rec);
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                    GUILayout.Label(text);
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public static void IntSlider(string text, ref int val)
    {
        GUILayout.BeginHorizontal(NodeBasedEditor.nodeSkin.customStyles[2]);
        GUILayout.Label(text, GUILayout.Height(layoutHeight));

        val = EditorGUILayout.IntSlider(val, 0, 500, GUILayout.Height(15f));
        
        GUILayout.EndHorizontal();
    }

    public static void FloatSlider(string text, ref float val)
    {
        GUILayout.BeginHorizontal(NodeBasedEditor.nodeSkin.customStyles[2]);
        GUILayout.Label(text, GUILayout.Height(layoutHeight));

        val = EditorGUILayout.Slider(val, 0, 10, GUILayout.Height(15f));
        
        GUILayout.EndHorizontal();
    }

    public static void NextCombo(string text, ref bool val)
    {
        GUILayout.BeginHorizontal(NodeBasedEditor.nodeSkin.customStyles[2]);
        GUILayout.Label(text, GUILayout.Height(layoutHeight));

        if (!val)
        {
            if (GUILayout.Button("Active"))
            {
                val = !val;
            }
        }
        else
        {
            if (GUILayout.Button("x"))
            {
                val = !val;
            }
        }

        GUILayout.EndHorizontal();
    }

    public static void LayoutLabelAndPopup(string text, ref int val, string[] array)
    {
        GUILayout.BeginHorizontal(NodeBasedEditor.nodeSkin.customStyles[2]);
            GUILayout.Label(text, NodeBasedEditor.nodeSkin.label, GUILayout.Height(layoutHeight));
            GUILayout.Space(8f);
            val = EditorGUILayout.Popup(val, array, GUILayout.Height(layoutHeight));
        GUILayout.EndHorizontal();
    }
}
