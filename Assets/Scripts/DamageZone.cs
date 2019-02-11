using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DamageZone : MonoBehaviour, IRespawnable, IPausable {

    [SerializeField]
    private bool enableOnAwake = false;

    [SerializeField]
    private int damage = 100;

    [SerializeField]
    private bool isKillZone = false;

    [SerializeField]
    private bool useDeathTimer = false;

    [SerializeField]
    private bool triggerOnlyOnTop = false;

    private float timer = 0f;
    [SerializeField]
    private float timeThreshold = 1f;

    private bool dealDamage = false;
    private bool isActive;

    [SerializeField]
    private bool isLooping = false;

    [SerializeField]
    private float activationTimer = 2f;
    [SerializeField]
    private float desactivationTimer = 2f;

    [SerializeField]
    private GameObject fx = null;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    private int playerMask;
    private int enemyMask;
    private BoxCollider collid = null;

    private void Start()
    {
        if (enableOnAwake)
            Activate();
        else if (fx)
            fx.SetActive(false);

        particleSystems.AddRange(GetComponents<ParticleSystem>());
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>(true));

        playerMask = 1 << LayerMask.NameToLayer("Player");
        enemyMask = 1 << LayerMask.NameToLayer("Enemy");
        collid = GetComponent<BoxCollider>();
    }

    public void Activate()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.AddPausable(this);

        if (isLooping)
            StartCoroutine(ActivationLoop());

        if (fx)
            fx.SetActive(true);
    }

    public void Deactivate()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.RemovePausable(this);

        if (isLooping)
            StopCoroutine(ActivationLoop());
        else
            gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        Deactivate();
    }

    private void FixedUpdate()
    {
        if (isKillZone)
            return;

        if (dealDamage)
            dealDamage = false;

        if (timer >= timeThreshold)
        {
            timer = 0f;
            dealDamage = true;
        }
        else
            timer += Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isKillZone)
        {
            if (triggerOnlyOnTop && !Physics.BoxCast(collid.bounds.center, new Vector3(collid.bounds.extents.x, collid.bounds.extents.y / 2f, collid.bounds.extents.z), Vector3.up, Quaternion.identity, 5f, playerMask | enemyMask))
                return;

            Entity entity = other.gameObject.GetComponent<Entity>();

            if (entity)
            {
                entity.Kill();
                GameManager.Instance.UseLoseWaitTimeNextGameOver = useDeathTimer;
            }
        }
        else
        {
            Entity entity = other.gameObject.GetComponent<Entity>();

            if (entity)
                entity.TakeDamage(damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isKillZone && dealDamage)
        {
            Entity entity = other.gameObject.GetComponent<Entity>();

            if (entity)
                entity.TakeDamage(damage);
        }
    }

    public void Respawn()
    {
        if (!enableOnAwake)
            gameObject.SetActive(false);
    }

    IEnumerator ActivationLoop()
    {
        float currentTimer = activationTimer;
        while (true)
        {
            yield return new WaitForSecondsPausable(activationTimer);
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            BoxCollider collider = GetComponent<BoxCollider>();

            if (renderer)
                renderer.enabled = isActive;
            if (collider)
                collider.enabled = isActive;
            if (fx)
                fx.SetActive(isActive);


            this.enabled = isActive;
            isActive = !isActive;

            currentTimer = isActive ? activationTimer : desactivationTimer;
        }
    }

    public void OnPause()
    {
        enabled = false;
        foreach (ParticleSystem sys in particleSystems)
            sys.Pause();
    }

    public void OnResume()
    {
        enabled = true;
        foreach (ParticleSystem sys in particleSystems)
            sys.Play();
    }
}
