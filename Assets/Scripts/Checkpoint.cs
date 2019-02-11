using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class Checkpoint : MonoBehaviour {

    [SerializeField]
    private List<RespawnController> respawnControllers;

    private bool isTriggered;

    public delegate void CheckpointDelegate(Checkpoint checkPoint);
    public event CheckpointDelegate OnTriggered;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (UnityEditor.ArrayUtility.Contains(UnityEditor.Selection.objects, gameObject))
        {
            if (respawnControllers != null && respawnControllers.Count > 0)
            {
                foreach (RespawnController respawnable in respawnControllers)
                {
                    if (respawnable == null)
                        continue;

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, respawnable.transform.position);
                    Gizmos.DrawIcon(respawnable.transform.position, "testIcon");
                }
            }
        }
    }
#endif
    private void Start()
    {
        isTriggered = false;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    public void InvokeRespawnable()
    {
        foreach (RespawnController controller in respawnControllers)
            controller.InvokeOnRespawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered)
        {
            if (other.tag == "Player")
            {
                OnTriggered(this);
                isTriggered = true;

                GameManager gameManager = GameManager.Instance;

                if (gameManager.HasTwoPlayer)
                {
                    if (gameManager.Player1.IsDead)
                        gameManager.Player1.Respawn();
                    if (gameManager.Player2.IsDead)
                        gameManager.Player2.Respawn();
                }
            }
        }
    }
}
