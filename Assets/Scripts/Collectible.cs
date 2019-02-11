using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Shadow))]
public class Collectible : MonoBehaviour, IPausable {
    [SerializeField]
    private int heal = 100;

    [SerializeField]
    private float speed = -5f;

    [SerializeField]
    private float collectibleLifeTime = 5f;
    [SerializeField]
    private float timeBeforeBlink = 3f;
    [SerializeField]
    private float blinkTime = 0.4f;

    private int pushableObjectMask;
    private int groundMask;

    private BoxCollider collid = null;
    private Vector3 extents;

    private SpriteRenderer spriteRenderer = null;
    private GameObject shadow = null;

    private bool isOnGround = false;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    private List<Animation> animationList = new List<Animation>();

    private void Start()
    {
        GameManager.Instance.PauseManager.AddPausable(this);

        collid = GetComponent<BoxCollider>();
        extents = collid.bounds.extents;

        spriteRenderer = GetComponent<SpriteRenderer>();

        pushableObjectMask = 1 << LayerMask.NameToLayer("PushableObject");
        groundMask = 1 << LayerMask.NameToLayer("Ground");

        particleSystems.AddRange(GetComponents<ParticleSystem>());
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>(true));

        animationList.AddRange(GetComponents<Animation>());
        animationList.AddRange(GetComponentsInChildren<Animation>(true));

        StartCoroutine(DestroyPausable());
    }

    private void FixedUpdate()
    {
       if (!isOnGround)
        {
            Vector3 position = transform.position;
            position.y += speed * Time.deltaTime;
            transform.position = position;

            isOnGround = Physics.BoxCast(collid.bounds.center, new Vector3(collid.size.x * transform.localScale.x / 2f, collid.size.y * transform.localScale.y / 2f * 0.1f, collid.size.z * transform.localScale.z / 2f), Vector3.down, Quaternion.identity, collid.size.y * transform.localScale.y / 2f, groundMask | pushableObjectMask);
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.PauseManager.RemovePausable(this);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            Player player = collider.GetComponent<Player>();
            if (player)
                player.Heal(heal);

            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyPausable()
    {
        yield return new WaitForSecondsPausable(timeBeforeBlink);
        shadow = transform.GetChild(0).gameObject;
        StartCoroutine(Blink());
        float timeBeforeDestroy = collectibleLifeTime - timeBeforeBlink;
        if (timeBeforeDestroy < 0f)
            timeBeforeDestroy = 0f;
        yield return new WaitForSecondsPausable(timeBeforeDestroy);
        Destroy(gameObject);
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            yield return new WaitForSecondsPausable(blinkTime);
            spriteRenderer.enabled = !spriteRenderer.enabled;
            shadow.SetActive(!shadow.activeSelf);
        }
    }

    public void OnPause()
    {
        foreach (ParticleSystem sys in particleSystems)
            sys.Pause();

        foreach (Animation anim in animationList)
            anim.enabled = false;
    }

    public void OnResume()
    {
        foreach (ParticleSystem sys in particleSystems)
            sys.Play();

        foreach (Animation anim in animationList)
            anim.enabled = true;
    }
}
