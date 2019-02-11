using UnityEngine;


public class EnemyStrong : Enemy
{
	[SerializeField]
	private EnemyHitBoxAttack normalBoxAttack = null;

	[SerializeField]
	private float attackRange = 1f;
	public float AttackRange
	{
		get { return attackRange; }
	}

	[SerializeField]
	private int meleeDamage = 1;

	[SerializeField]
	private int maxComboChain = 1;

	private bool canChainAttack = false;
	public bool CanChainAttack
	{
		get { return canChainAttack; }
	}

	private int attackNumber = 0;


	protected new void Start()
	{
		base.Start();

		normalBoxAttack.OnPlayerTouched += ExecuteAttack;
	}

	public override void Attack()
	{
		if (attackNumber < maxComboChain)
		{
		    //if (isStunned)
		    //    return;

            base.Attack();

			IsAttacking = true;

			++attackNumber;
			anim.Play("MeleeAttack" + attackNumber);

			if (attackNumber > 1)
				canChainAttack = false;
		}
	}

	public bool IsInRange()
	{
		if (nearestPlayer != null
			&& Vector3.Distance(transform.position, nearestPlayer.Collid.bounds.center) <= attackRange
			&& Mathf.Abs(transform.position.x - nearestPlayer.Collid.bounds.center.x) > nearestPlayer.Extents.x
			&& Mathf.Abs(transform.position.z - nearestPlayer.Collid.bounds.center.z) < zAcceptance)
			return true;

		return false;
	}

	protected override void AttackFinished()
	{
		base.AttackFinished();

		canChainAttack = false;
		attackNumber = 0;
	}

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (currLife > 0f && !isAttacking)
        {
            if (ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.None || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockBack || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockUp || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.Prone)
                anim.Play("Hit", -1, 0f);
        }
    }

    protected override void CancelCurrentAction()
	{
		base.CancelCurrentAction();

		canChainAttack = false;
		attackNumber = 0;
	}

	private void ExecuteAttack(Player player)
	{
		player.TakeDamage(meleeDamage);

		if (player.IsJumping)
			player.CCMgr.Prone(floatingDuration, proneDuration);
	}

    private void CanCombo()
	{
		canChainAttack = true;
	}
}
