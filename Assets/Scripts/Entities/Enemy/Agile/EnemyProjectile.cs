using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class EnemyProjectile : MonoBehaviour, IPausable
{
	private BoxCollider collid = null;

	public System.Action<Player> OnPlayerTouched;

	private float speed;
	private float right;
	private bool move = false;
    private bool waitForDestroy = false;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    private void Awake()
	{
		collid = GetComponent<BoxCollider>();

        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.AddPausable(this);

        particleSystems.AddRange(GetComponents<ParticleSystem>());
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>());
    }

	private void Update()
	{
		if (move)
		{
			Vector3 newPos = transform.position;
			newPos.x += speed * Time.deltaTime * right;
			transform.position = newPos;
		}
	}

    private void OnDestroy()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.RemovePausable(this);
    }

    private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			if (OnPlayerTouched != null)
				OnPlayerTouched(other.gameObject.GetComponent<Player>());

			Destroy(gameObject);
		}
		else if (other.tag == "Ground" || other.tag == "Wall" || other.tag == "PushableObject")
			Destroy(gameObject);
	}

	public void Init(float _speed, bool facingRight)
	{
		speed = _speed;
		right = (facingRight) ? 1f : -1f;
		move = true;
	}

    public void DestroyPausable(float waitTime)
    {
        if (!waitForDestroy)
        {
            waitForDestroy = true;
            StartCoroutine(DestroyCoroutine(waitTime));
        }
    }

    private IEnumerator DestroyCoroutine(float waitTime)
    {
        yield return new WaitForSecondsPausable(waitTime);
        Destroy(gameObject);
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
