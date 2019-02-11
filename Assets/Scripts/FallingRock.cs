using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRock : MonoBehaviour, IRespawnable, IPausable {
    [SerializeField]
    private float speed = 20f;

    [SerializeField]
    private GameObject rock = null;
    [SerializeField]
    private GameObject stompFx = null;
    [SerializeField]
    private GameObject warningFx = null;

    private bool isFalling = false;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    private AudioSource audioSource;

    [SerializeField]
    private float YDestination = 0f;

    [SerializeField]
    Color gizmosColor = Color.red;
    [SerializeField]
    Vector3 gizmosSize = new Vector3(1f, 1f, 1f);

    private void OnDrawGizmosSelected()
    {
        if (!rock)
            return;

        Gizmos.color = gizmosColor;
        Vector3 dest = new Vector3(rock.transform.position.x, YDestination, rock.transform.position.z);
        Gizmos.DrawLine(rock.transform.position, dest);
        Gizmos.DrawCube(dest, gizmosSize);
    }

    private void Start()
    {
        GameManager.Instance.PauseManager.AddPausable(this);

        particleSystems.AddRange(GetComponents<ParticleSystem>());
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>(true));

        audioSource = GetComponent<AudioSource>();

        if (stompFx)
        {
            Vector3 pos = stompFx.transform.position;
            pos.y = YDestination;
            stompFx.transform.position = pos;
        }
    }

    void Update()
    {
        if (isFalling && rock)
        {
            Vector3 rockPos = rock.transform.position;
            rockPos.y -= speed * Time.deltaTime;
            rock.transform.position = rockPos;

            if (rockPos.y <= YDestination)
                Stomp();
        }
    }

    public void startFalling()
    {
        isFalling = true;
        if (warningFx)
            warningFx.SetActive(true);
    }
    
    public void Respawn()
    {
        isFalling = false;
        if (rock)
            rock.SetActive(true);
        if (stompFx)
            stompFx.SetActive(false);
        if (warningFx)
        {
            Animator anim = warningFx.GetComponent<Animator>();
            if (anim)
                anim.enabled = false;
            warningFx.SetActive(false);
        }
    }

    private void Stomp()
    {
        isFalling = false;
        if (rock)
            rock.SetActive(false);
        if (stompFx)
            stompFx.SetActive(true);
        if (warningFx)
            warningFx.SetActive(false);
        if (audioSource)
            audioSource.Play();
    }

    public void OnPause()
    {
        enabled = false;
        if (warningFx)
        {
            Animator anim = warningFx.GetComponent<Animator>();
            if (anim)
                anim.enabled = false;
        }

        foreach (ParticleSystem sys in particleSystems)
            sys.Pause();
    }

    public void OnResume()
    {
        enabled = true;

        if (warningFx)
        {
            Animator anim = warningFx.GetComponent<Animator>();
            if (anim)
                anim.enabled = true;
        }

        foreach (ParticleSystem sys in particleSystems)
            sys.Play();
    }
}
