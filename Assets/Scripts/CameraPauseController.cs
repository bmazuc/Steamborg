using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
public class CameraPauseController : MonoBehaviour, IPausable {

    Cinemachine.CinemachineVirtualCamera virtualCamera;

    // Use this for initialization
    void Start()
    {
        virtualCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        GameManager.Instance.PauseManager.AddPausable(this);
    }

    void OnDestroy()
    {
        GameManager.Instance.PauseManager.RemovePausable(this);
    }

    public void OnPause()
    {
        virtualCamera.enabled = false;
    }

    public void OnResume()
    {
        virtualCamera.enabled = true;
    }
}
