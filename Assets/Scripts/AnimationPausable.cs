using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class AnimationPausable : MonoBehaviour, IPausable
{
    Animation anim;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animation>();
        GameManager.Instance.PauseManager.AddPausable(this);
    }

    void OnDestroy()
    {
        GameManager.Instance.PauseManager.RemovePausable(this);
    }

    public void OnPause()
    {
        anim.enabled = false;
    }

    public void OnResume()
    {
        anim.enabled = true;
    }
}
