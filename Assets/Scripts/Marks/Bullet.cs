using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Bullet : MonoBehaviour, IPausable
{
	//[SerializeField]
	//private Vector3 strongBulletSize = new Vector3();

	private BoxCollider collid = null;

	public System.Action<Enemy> OnEnemyTouched;

	private float bulletSpeed;
	private float knockBack;
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

	private void LateUpdate()
	{
		if (move)
		{
			Vector3 newPos = transform.position;
			newPos.x += bulletSpeed * Time.deltaTime * right;
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
		if (other.tag == "Enemy")
		{
			Enemy enemy = other.gameObject.GetComponent<Enemy>();

			if (enemy.CCMgr.CurrCC != CrowdControl.CONTROL_TYPE.KnockDown)
			{
				if (OnEnemyTouched != null)
					OnEnemyTouched(enemy);

				Destroy(gameObject);
			}
		}
		else if (other.tag == "Ground" || other.tag == "Wall" || other.tag == "PushableObject")
			Destroy(gameObject);
	}

	public void Init(float speed, float knockBackForce, bool facingRight, Player.ARCHETYPE archetype = Player.ARCHETYPE.Standard)
	{
		//if (archetype == Player.ARCHETYPE.Strong)
		//{
		//	collid.size = strongBulletSize;
		//}

		bulletSpeed = speed;
		knockBack = knockBackForce;
		right = (facingRight) ? 1f : -1f;
		move = true;
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
}
