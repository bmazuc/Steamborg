using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
abstract public class HitBoxAttack : MonoBehaviour
{
	[SerializeField]
	protected bool canTouchMultiple = true;

	[SerializeField]
	protected bool canHitOnKnockDown = false;

	[SerializeField]
	protected bool multipleHit = false;
	[SerializeField]
	protected float hitRate = 1f;

	protected List<Entity> touchedEntities = new List<Entity>();

	protected BoxCollider collid = null;


	private void Start()
	{
		collid = GetComponent<BoxCollider>();
	}

	protected virtual void OnEnable()
	{
		touchedEntities.Clear();

		if (multipleHit)
			StartCoroutine(RefreshHit());
	}

	private IEnumerator RefreshHit()
	{
		while (gameObject.activeSelf)
		{
			yield return new WaitForSecondsPausable(hitRate);

			collid.enabled = false;
			touchedEntities.Clear();

			yield return new WaitForSecondsPausable(0.1f);

			collid.enabled = true;
		}
	}
}
