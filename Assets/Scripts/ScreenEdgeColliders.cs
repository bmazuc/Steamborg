using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ScreenEdgeColliders : MonoBehaviour
{
    [SerializeField]
    private GameObject leftCollider;
    [SerializeField]
    private GameObject rightCollider;
    
    private Camera camera;
    private CinemachineBrain brain;

    void Start ()
	{
	    camera = GetComponent<Camera>();
	    brain = GetComponent<CinemachineBrain>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (GameManager.Instance.Player1 != null && brain.ActiveVirtualCamera != null)
	    {
	        CinemachineVirtualCamera virtualCamera = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();

	        if (virtualCamera != null)
	        {
	            float cameraDistance = GameManager.Instance.Player1.transform.position.z - virtualCamera.transform.position.z;

	            leftCollider.transform.position = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane + cameraDistance)) - new Vector3(leftCollider.transform.localScale.x / 2, 0, 0);
	            rightCollider.transform.position = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, camera.nearClipPlane + cameraDistance)) + new Vector3(rightCollider.transform.localScale.x / 2, 0, 0);

            }
        }
    }
}
