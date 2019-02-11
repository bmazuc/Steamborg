using System.Collections;
using UnityEngine;


public class EnemyAgile : Enemy
{
	[SerializeField]
	private float longShotRangeMini = 1f;
	public float LongShotRangeMini
	{
		get { return longShotRangeMini; }
	}

	[SerializeField]
	private float longShotRangeMax = 1f;
	public float LongShotRangeMax
	{
		get { return longShotRangeMax; }
	}

	[SerializeField]
	private float shortShotRangeMini = 1f;
	public float ShortShotRangeMini
	{
		get { return shortShotRangeMini; }
	}

	[SerializeField]
	private float shortShotRangeMax = 1f;
	public float ShortShotRangeMax
	{
		get { return shortShotRangeMax; }
	}

	[SerializeField]
	private int projectileDamage = 1;
	[SerializeField]
	private float projectileSpeed = 1f;
	[SerializeField]
	private float projectileLifeTime = 1f;

	[SerializeField]
	private float reloadDuration = 1f;

	[SerializeField]
	private EnemyProjectile projectilePrefab = null;

	private Animator brainComponent = null;


	protected new void Start()
	{
		base.Start();

		brainComponent = brain.GetComponent<Animator>();
	}

	public void LongShot()
	{
	    //if (isStunned)
	    //    return;

		Attack();

		IsAttacking = true;

		anim.SetBool("longShot", true);
	}
	public void ShortShot()
	{
	    //if (isStunned)
	    //    return;

        Attack();

		IsAttacking = true;

		anim.SetBool("shortShot", true);
	}

	public bool IsXAlignWithNearestPlayer()
	{
		return (Mathf.Abs(transform.position.z - nearestPlayer.Collid.bounds.center.z) < zAcceptance);
	}

	public bool IsYAlignWithNearestPlayer()
	{
		if (((transform.position.y > nearestPlayer.Collid.bounds.center.y
				&& Mathf.Abs(transform.position.y + collid.center.y - nearestPlayer.Collid.bounds.center.y + nearestPlayer.Extents.y) < extents.y + nearestPlayer.Extents.y)
			|| (transform.position.y < nearestPlayer.Collid.bounds.center.y
				&& Mathf.Abs(transform.position.y - collid.center.y - nearestPlayer.Collid.bounds.center.y - nearestPlayer.Extents.y) < extents.y + nearestPlayer.Extents.y)))
			return true;

		return false;
	}

	// -1 = too close, 0 = in range, 1 = too far
	public int IsInLongShotRange()
	{
		float distance = Vector3.Distance(nearestPlayer.Collid.bounds.center, transform.position);
		if (distance > longShotRangeMax)
			return 1;
		else if (distance < longShotRangeMini)
			return -1;

		return 0;
	}
	// -1 = too close, 0 = in range, 1 = too far
	public int IsInShortShotRange()
	{
		float distance = Vector3.Distance(nearestPlayer.Collid.bounds.center, transform.position);
		if (distance > shortShotRangeMax)
			return 1;
		else if (distance < shortShotRangeMini)
			return -1;

		return 0;
	}

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (currLife > 0f && !isAttacking)
        {
            if (ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.None || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockBack)
                anim.Play("Hit", -1, 0f);
            else if (ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockUp || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.Prone)
                anim.Play("Hit_InAir", -1, 0f);
        }
    }

    protected override void AttackFinished()
	{
		base.AttackFinished();

		StartCoroutine(Reloading(reloadDuration));

		anim.SetBool("longShot", false);

		if (enemyType != ENEMY_TYPE.Small)
			anim.SetBool("shortShot", false);
	}

    protected override void CancelCurrentAction()
    {
        base.CancelCurrentAction();

        anim.SetBool("longShot", false);
        anim.SetBool("isReloading", false);
		brainComponent.SetBool("lockOnReload", false);

		//if (enemyType != ENEMY_TYPE.Small)
		//    anim.SetBool("shortShot", false);

	}

    private void Shoot()
	{
		if (isAttacking)
		{
			EnemyProjectile newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
			newProjectile.OnPlayerTouched += ExecuteShoot;

			newProjectile.Init(projectileSpeed, facingRight);

			newProjectile.DestroyPausable(projectileLifeTime);
		}
	}

	private void ExecuteShoot(Player player)
	{
		player.TakeDamage(projectileDamage);
	}

	private IEnumerator Reloading(float duration)
	{
		anim.SetBool("isReloading", true);

		yield return new WaitForSecondsPausable(duration);

		anim.SetBool("isReloading", false);

		brainComponent.SetBool("lockOnReload", false);

		//if (enemyType != ENEMY_TYPE.Small)
		//	brainComponent.SetBool("runAway", false);
	}
}
