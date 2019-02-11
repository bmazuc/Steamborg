using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneButtonTool : EditorWindow {
    
    private static SceneButtonTool window;

    [MenuItem("Window/Scene Button Tool")]
    private static void OpenWindow()
    {
        window = GetWindow<SceneButtonTool>();
        window.titleContent = new GUIContent("Scene Button Tool");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 0f, position.width, position.height));
        GUILayout.BeginVertical();

        if (GUILayout.Button("Load MainMenu", GUILayout.Height(47f)))
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        if (GUILayout.Button("Load ReworkAnimationScene", GUILayout.Height(47f)))
            EditorSceneManager.OpenScene("Assets/Scenes/ReworkAnimationScene.unity");

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }


}
