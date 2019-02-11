using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RespawnController : MonoBehaviour {

    private delegate void RespawnDelegate();
    private RespawnDelegate OnRespawn;

    // Use this for initialization
    void Awake () {
        IRespawnable respawnable = gameObject.GetComponent<IRespawnable>();

        if (respawnable != null)
            OnRespawn += () => respawnable.Respawn();
	}

    public void InvokeOnRespawn()
    {
        if (OnRespawn != null)
            OnRespawn();
    }
}
