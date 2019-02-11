using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOverrider : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    public GameObject Prefab { set { prefab = value; } }

    private Animator animator;
    protected AnimatorOverrideController animatorOverrideController;

    [SerializeField]
    private string clipToOverrideName;
    public string ClipToOverrideName { set { clipToOverrideName = value; } }

    [SerializeField]
    private AnimationClip clip;
    public AnimationClip Clip { set { clip = value; } }

    void Start()
    {

        if (prefab)
        {
            GameObject instance = GameObject.Instantiate(prefab, transform);
            animator = instance.GetComponent<Animator>();

            animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverrideController;
            animatorOverrideController[clipToOverrideName] = clip;
        }
    }

}