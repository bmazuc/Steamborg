using System.Collections;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class BreakablePlatform : MonoBehaviour
{
	[SerializeField]
	private float breakDelay = 1f;
	[SerializeField]
	private float breakTimer = 1f;

	private bool activated = false;

	private BoxCollider collid = null;

	private float fadeOutTime;

	private int groundMask;
	private int playerMask;


	private void Awake()
	{
		collid = GetComponent<BoxCollider>();

		fadeOutTime = breakTimer;

		groundMask = 1 << LayerMask.NameToLayer("Ground");
		playerMask = 1 << LayerMask.NameToLayer("Player");
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!activated && collision.collider.tag == "Player")
		{
			Player player = collision.gameObject.GetComponent<Player>();
			if (Physics.BoxCast(collid.bounds.center, new Vector3(collid.bounds.extents.x, collid.bounds.extents.y / 2f, collid.bounds.extents.z), Vector3.up, Quaternion.identity, 5f, playerMask))
			{
				activated = true;
                StartCoroutine(StartBreak());
			}
		}
	}

	private IEnumerator StartBreak()
	{
        yield return new WaitForSecondsPausable(breakDelay);
        StartCoroutine(Break());
	}

	private IEnumerator Break()
	{
        yield return new WaitForSecondsPausable(breakTimer);
		Destroy(gameObject);
	}
}