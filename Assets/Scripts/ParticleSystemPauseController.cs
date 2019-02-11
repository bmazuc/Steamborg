using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemPauseController : MonoBehaviour, IPausable {

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    // Use this for initialization
    void Start () {
        particleSystems.AddRange(GetComponents<ParticleSystem>());
        GameManager.Instance.PauseManager.AddPausable(this);
    }

    void OnDestroy()
    {
        GameManager.Instance.PauseManager.RemovePausable(this);
    }

    public void OnPause()
    {
        foreach (ParticleSystem sys in particleSystems)
            sys.Pause();
    }

    public void OnResume()
    {
        foreach (ParticleSystem sys in particleSystems)
            sys.Play();
    }
}
