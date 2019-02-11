using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineTargetGroup cameraTargetGroup;
    public CinemachineTargetGroup CameraTargetGroup {
        get
        {
            if (cameraTargetGroup == null)
                cameraTargetGroup = GameObject.FindObjectOfType<CinemachineTargetGroup>();

            return cameraTargetGroup;
        }
    }

    private Transform forcusTarger;
    private Transform deadPlayer;
    [SerializeField]
    private float focusTime = 3f;

    [SerializeField]
    private float deathPlayerLossFocusTime = 1.5f;

    //private List<CinemachineVirtualCamera> virtualCamerasList = new List<CinemachineVirtualCamera>();

    void OnEnable()
    {
        //SceneManager.sceneLoaded += OnLevelWasLoaded;
    }

    //private void OnLevelWasLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    virtualCamerasList.Clear();
    //    virtualCamerasList = GameObject.FindObjectsOfType<CinemachineVirtualCamera>().ToList();
    //    Debug.LogWarning("OnLevelWasLoaded " + virtualCamerasList.Count);
    //}

    public void SetActiveCamera(CinemachineVirtualCamera camera)
    {
        //Debug.LogWarning("CountSetActiveCamera : " + virtualCamerasList.Count);
        foreach (CinemachineVirtualCamera virtualCamera in GameObject.FindObjectsOfType<CinemachineVirtualCamera>())
        {
            virtualCamera.Priority = 0;
        }

        camera.Priority = 100;
    }

    public void AddTransformToCameraGroup(Transform transform, int index)
    {
        if (transform != CameraTargetGroup.m_Targets[index].target)
        {
            CinemachineTargetGroup.Target targ = new CinemachineTargetGroup.Target { target = transform, weight = 1, radius = 0 };
            CameraTargetGroup.m_Targets[index] = targ;
        }
    }

    public void RemoveTransformFromCameraGroup(Transform targetTransform)
    {
        for (int i = 0; i < CameraTargetGroup.m_Targets.Length; i++)
        {
            if (CameraTargetGroup.m_Targets[i].target == targetTransform)
            {
                CameraTargetGroup.m_Targets[i].target = null;
            }
        }
    }

    public void AddCameraTemporaryFocusPoint(Transform transform)
    {
        forcusTarger = transform;

        StartCoroutine("LaunchFocus");
    }

    private IEnumerator LaunchFocus()
    {
        AddTransformToCameraGroup(forcusTarger, 2);

        yield return new WaitForSecondsPausable(focusTime);

        RemoveTransformFromCameraGroup(forcusTarger);
    }

    public void PlayerDeathLossFocus(Player player)
    {
        deadPlayer = player.transform;
        RemoveTransformFromCameraGroup(deadPlayer);
        //deadPlayer = player.transform;
        //StartCoroutine("PlayerLossFocus");
    }

    private IEnumerator PlayerLossFocus()
    {
        yield return new WaitForSecondsPausable(deathPlayerLossFocusTime);
        RemoveTransformFromCameraGroup(deadPlayer);
    }
    
    public void ChangeActiveCamera(CinemachineVirtualCamera camera)
    {
        camera.Priority = 100;
    }

    public void ResetActiveCamera(CinemachineVirtualCamera camera)
    {
        camera.Priority = 0;
    }

}
