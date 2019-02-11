using System;
using System.Collections;
using BTMCombo;
using UnityEngine;


public class PlayerMelee : Player
{
	[SerializeField]
	private float knockUpForce = 1f;
	[SerializeField]
	private float knockBackForce = 1f;

	[SerializeField]
	private Collider grabCollider = null;
	[SerializeField]
	private float grabRange = 1f;
	[SerializeField]
	private float grabSpeed = 1f;
	[SerializeField]
	private float stunDurationAfterGrab = 1f;
    [SerializeField]
    private float dashAttackValue = 10f;

	[SerializeField]
	private float strongAttackStickDelay = 0.15f;

    protected BTMCombo.ComboBrain comboMgr = null;
    
	private int currDamage = 0;
	private BTMCombo.ComboBrain.InputType attackType;
	private int attackNumber = 0;
	private int strongDirection = 0;

    [SerializeField]
    private bool showArchetypeButton = false;


	protected new void Start()
	{
		base.Start();
		comboMgr = GetComponent<BTMCombo.ComboBrain>();
		comboMgr.OnComboReset += AttackFinished;

		PlayerHitBoxAttack[] attacks = GetComponentsInChildren<PlayerHitBoxAttack>(true);
		for (int idx = 0; idx < attacks.Length; ++idx)
		{
			attacks[idx].OnEnemyTouched += ExecuteAttack;
		}
	}

	public override void LightAttack()
	{
	    if (IsDodging)
	        return;

		base.LightAttack();
		if (isAttacking && !forceFallDown)
		{
			comboMgr.SendAction(BTMCombo.ComboBrain.InputType.Light, currArchetype);
		}
	}
	public override void StrongAttack()
	{
	    if (IsDodging)
	        return;

        base.StrongAttack();
		if (isAttacking && !forceFallDown)
		{
			comboMgr.SendAction(BTMCombo.ComboBrain.InputType.Strong, currArchetype);
		}
	}
	public override void SpecialAttack()
	{
	    if (IsDodging)
	        return;

        base.SpecialAttack();
		if (isAttacking && !forceFallDown)
		{
			comboMgr.SendAction(BTMCombo.ComboBrain.InputType.Special, currArchetype);

			if (currArchetype == ARCHETYPE.Agile)
			{
				InitGrab();
			}
		}
	}

	public void LightAttack_Archetype(int damage)
	{
		currDamage = damage;
		attackType = BTMCombo.ComboBrain.InputType.Light;

		LaunchAttackAnim();
		if (!isJumping)
			rigid.AddForce((facingRight ? Vector3.right : Vector3.left) * dashAttackValue, ForceMode.Impulse);
	}
	public void StrongAttack_Archetype(int damage)
	{
		currDamage = damage;
		attackType = BTMCombo.ComboBrain.InputType.Strong;

		StartCoroutine(StickCheckDelay());
    }
	public void SpecialAttack_Archetype(int damage)
	{
		currDamage = damage;
		attackType = BTMCombo.ComboBrain.InputType.Special;

		if (currArchetype != ARCHETYPE.Standard)
			gameObject.layer = ignoreEnemyAttack;

		LaunchAttackAnim();
    }
    
	protected override void AttackFinished()
	{
		if (attackType == ComboBrain.InputType.Special)
		{
			if (currArchetype == ARCHETYPE.Standard)
				return;
			else
			{
				gameObject.layer = playerLayer;
				ResetMorph();
			}
		}

		base.AttackFinished();
		anim.SetBool("lightAttack", false);

		if (isJumping && !isAttacking)
			forceFallDown = true;

		attackNumber = 0;
		strongDirection = 0;
	}

	protected override void CancelCurrentAction()
	{
		base.CancelCurrentAction();

		anim.SetBool("lightAttack", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("specialAttackStandard", false);

		if (isJumping && !isAttacking)
			forceFallDown = true;

		attackNumber = 0;
		strongDirection = 0;
	}

	private void ExecuteAttack(Enemy enemy)
	{
	    enemy.TakeDamage(currDamage);

        if (currArchetype == ARCHETYPE.Standard)
			ExecuteStandardAttack(enemy);
		else if (currArchetype == ARCHETYPE.Strong)
			ExecuteStrongAttack(enemy);
		else
			ExecuteAgileAttack(enemy);

		if (enemy.IsDead)
			catchEnemy = null;

		if (currArchetype == ARCHETYPE.Standard && attackType == ComboBrain.InputType.Special)
			return;
    }

    private void ExecuteStandardAttack(Enemy enemy)
	{
		InAirAttack();

		if (attackType == BTMCombo.ComboBrain.InputType.Light)
		{
			if (enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockUp || enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.Prone)
				enemy.CCMgr.Prone(floatingDuration, proneDuration);
			else
				enemy.CCMgr.KnockBack(((facingRight) ? Vector3.right : Vector3.left), dashAttackValue);
		}
		else if (attackType == BTMCombo.ComboBrain.InputType.Strong)
		{
			Projection(enemy);
		}
		else
		{
			if (isAttacking)
			{
				morphChannelCoroutine = StartCoroutine(MorphChannel(ARCHETYPE.Strong));

				enemy.CCMgr.Stun(morphChannelTime);

				catchEnemy = enemy;
			}
		}
	}
	private void ExecuteStrongAttack(Enemy enemy)
	{
		InAirAttack();

		if (attackType == BTMCombo.ComboBrain.InputType.Light)
		{
			if (enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockUp || enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.Prone)
				enemy.CCMgr.Prone(floatingDuration, proneDuration);
			else
				enemy.CCMgr.KnockBack(((facingRight) ? Vector3.right : Vector3.left), dashAttackValue);
		}
		else if (attackType == BTMCombo.ComboBrain.InputType.Strong)
		{
			Projection(enemy);
		}
		else
		{
			enemy.CCMgr.KnockUp(knockUpForce);
		}
	}
	private void ExecuteAgileAttack(Enemy enemy)
	{
		InAirAttack();

		if (attackType == BTMCombo.ComboBrain.InputType.Light)
		{
			if (enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockUp || enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.Prone)
				enemy.CCMgr.Prone(floatingDuration, proneDuration);
			else
				enemy.CCMgr.KnockBack(((facingRight) ? Vector3.right : Vector3.left), dashAttackValue);
		}
		else if (attackType == BTMCombo.ComboBrain.InputType.Strong)
		{
			Projection(enemy);
		}
		else
		{
			enemy.CCMgr.Grab(transform, extents, grabSpeed, stunDurationAfterGrab);
		}
	}

	private void InitGrab()
	{
		RaycastHit hit;
		if (Physics.BoxCast(collid.bounds.center, extents, ((facingRight) ? Vector3.right : Vector3.left), out hit, Quaternion.identity, grabRange, enemyMask))
		{
			grabCollider.transform.localPosition = new Vector3(hit.distance, 0f, 0f);
		}
		else
		{
			grabCollider.transform.localPosition = new Vector3(grabRange, 0f, 0f);
		}
	}

	private void Projection(Enemy enemy)
	{
		if (strongDirection == 1)
			enemy.CCMgr.KnockUp(knockUpForce);
		else if (strongDirection == -1)
			enemy.CCMgr.KnockDown(knockDownDuration);
		else
			enemy.CCMgr.KnockBack((facingRight) ? Vector3.right : Vector3.left, knockBackForce);
	}

	private void LaunchAttackAnim()
	{
		if (currArchetype == ARCHETYPE.Standard)
			LaunchStandardAttackAnim();
		else if (currArchetype == ARCHETYPE.Strong)
			LaunchStrongAttackAnim();
		else
			LaunchAgileAttackAnim();
	}
	private void LaunchStandardAttackAnim()
	{
		if (attackType == BTMCombo.ComboBrain.InputType.Light)
		{
			anim.SetBool("lightAttack", true);

			if (!isJumping)
				anim.Play("LightAttackStandard" + (attackNumber + 1));
			else
				anim.Play("LightJumpAttackStandard" + (attackNumber + 1));

			++attackNumber;
		}
		else if (attackType == BTMCombo.ComboBrain.InputType.Strong)
		{
			if (!isJumping)
			{
				if (strongDirection == 1)
					anim.Play("StrongAttackStandardUp");
				else if (strongDirection == -1)
					anim.Play("StrongAttackStandardDown");
				else
					anim.Play("StrongAttackStandardForward");
			}
			else
			{
				if (strongDirection == 1)
					anim.Play("StrongJumpAttackStandardUp");
				else if (strongDirection == -1)
					anim.Play("StrongJumpAttackStandardDown");
				else
					anim.Play("StrongJumpAttackStandardForward");
			}
		}
		else
			anim.SetBool("specialAttackStandard", true);
	}
	private void LaunchStrongAttackAnim()
	{
		if (attackType == BTMCombo.ComboBrain.InputType.Light)
		{
			anim.SetBool("lightAttack", true);

			if (!isJumping)
				anim.Play("LightAttackStrong" + (attackNumber + 1));
			else
				anim.Play("LightJumpAttackStrong" + (attackNumber + 1));

			++attackNumber;
		}
		else if (attackType == BTMCombo.ComboBrain.InputType.Strong)
		{
			if (!isJumping)
			{
				if (strongDirection == 1)
					anim.Play("StrongAttackStrongUp");
				else if (strongDirection == -1)
					anim.Play("StrongAttackStrongDown");
				else
					anim.Play("StrongAttackStrongForward");
			}
			else
			{
				if (strongDirection == 1)
					anim.Play("StrongJumpAttackStrongUp");
				else if (strongDirection == -1)
					anim.Play("StrongJumpAttackStrongDown");
				else
					anim.Play("StrongJumpAttackStrongForward");
			}
		}
		else
		{
		    SetSpecialAttack();
            anim.SetTrigger("specialAttackStrong");
        }
			
	}
	private void LaunchAgileAttackAnim()
	{
		if (attackType == BTMCombo.ComboBrain.InputType.Light)
		{
			anim.SetBool("lightAttack", true);

			if (!isJumping)
				anim.Play("LightAttackAgile" + (attackNumber + 1));
			else
				anim.Play("LightJumpAttackAgile" + (attackNumber + 1));

			++attackNumber;
		}
		else if (attackType == BTMCombo.ComboBrain.InputType.Strong)
		{
			if (!isJumping)
			{
				if (strongDirection == 1)
					anim.Play("StrongAttackAgileUp");
				else if (strongDirection == -1)
					anim.Play("StrongAttackAgileDown");
				else
					anim.Play("StrongAttackAgileForward");
			}
			else
			{
				if (strongDirection == 1)
					anim.Play("StrongJumpAttackAgileUp");
				else if (strongDirection == -1)
					anim.Play("StrongJumpAttackAgileDown");
				else
					anim.Play("StrongJumpAttackAgileForward");
			}
		}
		else
			anim.SetTrigger("specialAttackAgile");
	}

    private IEnumerator StickCheckDelay()
	{
		yield return new WaitForSeconds(0.15f);

		float upDown = inputDirection.z;
		float leftRight = inputDirection.x;

		if (Mathf.Abs(upDown) > Mathf.Abs(leftRight))
		{
			if (upDown > 0f)
				strongDirection = 1;
			else
				strongDirection = -1;
		}
		else
			strongDirection = 0;

		LaunchAttackAnim();
	}

#if UNITY_EDITOR
    void OnGUI()
    {
        if (showArchetypeButton)
        {
            if (GUI.Button(new Rect(10, 40, 150, 100), "Strong"))
                MorphingTo(ARCHETYPE.Strong);

            if (GUI.Button(new Rect(10, 180, 150, 100), "Agile"))
                MorphingTo(ARCHETYPE.Agile);
        }
    }
#endif
}
