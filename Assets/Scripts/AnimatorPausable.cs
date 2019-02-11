using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorPausable : MonoBehaviour, IPausable {

    private Animator animator = null;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        GameManager.Instance.PauseManager.AddPausable(this);
	}

    void OnDestroy()
    {
        GameManager.Instance.PauseManager.RemovePausable(this);
    }

    public void OnPause()
    {
        animator.enabled = false;
    }

    public void OnResume()
    {
        animator.enabled = true;
    }
}
