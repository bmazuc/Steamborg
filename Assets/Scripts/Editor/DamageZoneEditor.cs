using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(DamageZone)), CanEditMultipleObjects]
public class DamageZoneEditor : Editor {
    SerializedProperty  enableOnAwake;
    SerializedProperty  isKillZone;
    SerializedProperty  useDeathTimer;
    SerializedProperty  triggerOnlyOnTop;
    SerializedProperty  damage;
    SerializedProperty  timeThreshold;
    SerializedProperty  isLooping;
    SerializedProperty  activationTimer;
    SerializedProperty  desactivationTimer;
    SerializedProperty  fx;

    void OnEnable()
    {
        enableOnAwake       = serializedObject.FindProperty("enableOnAwake");
        isKillZone          = serializedObject.FindProperty("isKillZone");
        useDeathTimer       = serializedObject.FindProperty("useDeathTimer");
        triggerOnlyOnTop    = serializedObject.FindProperty("triggerOnlyOnTop");
        damage              = serializedObject.FindProperty("damage");
        timeThreshold       = serializedObject.FindProperty("timeThreshold");
        isLooping           = serializedObject.FindProperty("isLooping");
        activationTimer     = serializedObject.FindProperty("activationTimer");
        desactivationTimer  = serializedObject.FindProperty("desactivationTimer");
        fx                  = serializedObject.FindProperty("fx");
    }

    public override void OnInspectorGUI()
    {
        enableOnAwake.boolValue = EditorGUILayout.Toggle("Enable On Awake", enableOnAwake.boolValue);
        isKillZone.boolValue    = EditorGUILayout.Toggle("Is Kill Zone ?", isKillZone.boolValue);

        if (!isKillZone.boolValue)
        {
            damage.intValue = EditorGUILayout.IntField("Damage", damage.intValue);
            timeThreshold.floatValue = EditorGUILayout.FloatField("Time Threshold", timeThreshold.floatValue);

            isLooping.boolValue = EditorGUILayout.Toggle("Is Looping ?", isLooping.boolValue);

            if (isLooping.boolValue)
            {
                activationTimer.floatValue = EditorGUILayout.FloatField("Activation Timer", activationTimer.floatValue);
                desactivationTimer.floatValue = EditorGUILayout.FloatField("Desactivation Timer", desactivationTimer.floatValue);
            }
        }
        else
        {
            useDeathTimer.boolValue     = EditorGUILayout.Toggle(new GUIContent("Use Death Timer?", "Use the death timer in gamemanager? Useful for hole."), useDeathTimer.boolValue);
            triggerOnlyOnTop.boolValue  = EditorGUILayout.Toggle("Trigger Only On Top ?", triggerOnlyOnTop.boolValue);
        }

        fx.objectReferenceValue = EditorGUILayout.ObjectField("Fx", fx.objectReferenceValue, typeof(GameObject), true);

        serializedObject.ApplyModifiedProperties();
    }
}
