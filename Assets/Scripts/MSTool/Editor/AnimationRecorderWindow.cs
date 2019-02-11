using UnityEditor;
using UnityEngine;
using UnityEngine.Recorder;


/*
 * Variables & functions passed from private to protected in RecorderWindow script:
 *      - m_WindowSettingsAsset
 *      - void StartRecording(bool autoExitPlayMode)        -> also set to virtual
 *      - void StopRecording()                              -> also set to virtual
*/
public class AnimationRecorderWindow : UnityEditor.Recorder.RecorderWindow
{
    Object prefab;
    AnimationClip clip;
    string clipToOverrideName;
    int recordNthTimeAnim = 1;
    Vector3 prefabOffset = new Vector3(0f, 0f, 10f);

    GameObject instantiateObject;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Tools/AnimationRecorder")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AnimationRecorderWindow window = (AnimationRecorderWindow)EditorWindow.GetWindow(typeof(AnimationRecorderWindow));
        window.Show();
    }

    private new void OnGUI()
    {
        GUILayout.Label("Animation Override :", EditorStyles.boldLabel);
        prefab = EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        prefabOffset = EditorGUILayout.Vector3Field("Prefab offset with camera", prefabOffset);
        clipToOverrideName = EditorGUILayout.TextField("Animation to override", clipToOverrideName);
        clip = (AnimationClip)EditorGUILayout.ObjectField("Animation to play", clip, typeof(AnimationClip), false);
        recordNthTimeAnim = EditorGUILayout.IntField("Record Nth time animation", recordNthTimeAnim);

        GUILayout.Label("Recorder :", EditorStyles.boldLabel);
        base.OnGUI();
    }

    void CreateAnimationOverrider()
    {
        if (!prefab || !clip)
            return;

        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        if (!mainCamera)
            return;

        instantiateObject = new GameObject("AnimationOverrider");
        instantiateObject.transform.position = mainCamera.transform.position + prefabOffset;

        AnimationOverrider animationOverrider = instantiateObject.AddComponent<AnimationOverrider>();
        animationOverrider.Prefab = (GameObject)prefab;
        animationOverrider.Clip = clip;
        animationOverrider.ClipToOverrideName = clipToOverrideName;

        m_WindowSettingsAsset.m_Settings.m_DurationMode = DurationMode.TimeInterval;
        m_WindowSettingsAsset.m_Settings.m_EndTime = clip.length * recordNthTimeAnim;
    }

    protected override void StartRecording(bool autoExitPlayMode)
    {
        CreateAnimationOverrider();
        base.StartRecording(autoExitPlayMode);
    }

    protected override void StopRecording()
    {
        if (instantiateObject)
            DestroyImmediate(instantiateObject);

        base.StopRecording();
    }
}
