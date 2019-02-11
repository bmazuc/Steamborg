using System.Collections;
using UnityEngine;


public class EnemyStrongBig : EnemyStrong
{
	[SerializeField]
	private EnemyHitBoxAttack stunBoxAttack = null;

	[SerializeField]
	private float knockUpForce = 1f;

	[SerializeField]
	private float knockUpCooldown = 1f;

	[SerializeField]
	private int knockUpAttackDamage = 1;
	[SerializeField]
	private float knockUpAttackRange = 1f;

    [SerializeField]
    private float knockDownpDuration = 0.55f;

    private bool stunReady = true;
	public bool StunReady
	{
		get { return stunReady; }
	}
	private bool stunAttacking = false;
	public bool StunAttacking
	{
		get { return stunAttacking; }
		set { stunAttacking = value; anim.SetBool("stunAttack", value); }
	}


	protected new void Start()
	{
		base.Start();

		stunBoxAttack.OnPlayerTouched += ExecuteStunAttack;
	}

	public void StunAttack()
	{
	    //if (isStunned)
	    //    return;

        inputDirection = Vector3.zero;
		moveDirection = Vector3.zero;

		stunReady = false;

		StunAttacking = true;
		IsAttacking = true;

		anim.Play("Stomp");

		StartCoroutine(StunAttackCooldown(knockUpCooldown));
	}

	public bool IsInStunAttackRange()
	{
		if (nearestPlayer != null
			&& Vector3.Distance(transform.position, nearestPlayer.Collid.bounds.center) <= knockUpAttackRange)
			return true;

		return false;
	}

	protected override void AttackFinished()
	{
		base.AttackFinished();

		StunAttacking = false;
	}

	protected override void CancelCurrentAction()
	{
		base.CancelCurrentAction();

		StunAttacking = false;
	}

	private void ExecuteStunAttack(Player player)
	{
		player.TakeDamage(knockUpAttackDamage);
        player.CCMgr.KnockDown(knockDownpDuration);
	}

	private IEnumerator StunAttackCooldown(float cooldown)
	{
		yield return new WaitForSecondsPausable(cooldown);

		stunReady = true;
	}
}
