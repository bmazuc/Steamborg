using System.Collections.Generic;
using UnityEngine;


public class PlayerRanged : Player
{
	[SerializeField]
	private int[] lightAttacksDamage = new int[3];
	[SerializeField]
	private int[] specialAttacksDamage = new int[3];

	[SerializeField]
	private float fireRate = 1f;
	[SerializeField]
	private float bulletSpeed = 1f;
	[SerializeField]
	private float bulletLifeTime = 1f;
	[SerializeField]
	private float bulletKnockBackForce = 1f;

	[SerializeField]
	private MarkSystem markSystemPrefab = null;
	[SerializeField]
	private Bullet bulletStandardPrefab = null;
	[SerializeField]
	private Bullet bulletStrongPrefab = null;
	[SerializeField]
	private Bullet bulletAgilePrefab = null;

    [SerializeField]
    private Transform bulletSpawn = null;

	private List<MarkSystem> markSystems = new List<MarkSystem>();

    [SerializeField]
    private Collider strongSpecialBox = null;

    [SerializeField]
    protected AnimationCurve propulseCurve;
    [SerializeField]
    protected float propulseHeight = 1f;
    [SerializeField]
    protected float propulseSpeed = 1f;
    protected float propulseYStart;
    protected float propulseTime;
    bool isPropulsed = false;

	[SerializeField]
	private bool showDebugButton = false;

	[SerializeField]
	protected float laserBeamStunDuration = 1f;

	private bool isSpecialAttackingStrong = false;
	private bool isSpecialAttackingAgile = false;
    Vector3 originPos;

    private AudioSource laserSound;


	protected new void Start()
	{
		base.Start();
		anim.speed = fireRate;

		PlayerHitBoxAttack[] attacks = GetComponentsInChildren<PlayerHitBoxAttack>(true);
		for (int idx = 0; idx < attacks.Length; ++idx)
		{
			attacks[idx].OnEnemyTouched += ExecuteSpecialAttack;
		}

        laserSound = GetComponent<AudioSource>();
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();

        if (isPropulsed)
        {
            if (propulseTime > 1f)
            {
                IsFalling = true;
                isPropulsed = false;
            }

            if (Physics.Raycast(new Ray(transform.position, Vector3.up), (Vector3.up * extents.y).magnitude + 0.1f, groundMask))
                propulseTime = GetPropulseTimeFallOff(propulseTime);

            float y = propulseCurve.Evaluate(propulseTime) * propulseHeight;

            Vector3 newPos = transform.position;
            newPos.y = y + propulseYStart;
            transform.position = newPos;

            if (!inAirAttack)
                propulseTime += Time.fixedDeltaTime * propulseSpeed;
        }

        if (isSpecialAttackingStrong && strongSpecialBox)
            strongSpecialBox.transform.position = originPos;
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (!laserSound)
            return;

        if (laserSound.isPlaying)
            laserSound.Stop();
    }

    protected float GetPropulseTimeFallOff(float time)
    {
        float currTime = propulseCurve.Evaluate(time);
        float fallOffTime = propulseCurve.Evaluate(time + Time.deltaTime);

        while (currTime < fallOffTime)
        {
            time += Time.deltaTime;
            fallOffTime = propulseCurve.Evaluate(time);
        }

        return time;
    }

    public override void LightAttack()
	{
	    if (IsDodging)
	        return;

        if (!isSpecialAttackingAgile)
		{
			base.LightAttack();
			if (isAttacking)
			{
				InAirAttack();
				anim.Play("Shoot");
			}
		}
	}
	public override void StrongAttack()
	{
	    if (IsDodging)
	        return;

        if (markSystems.Count > 0 && !isSpecialAttackingAgile)
		{
			base.StrongAttack();
			if (isAttacking)
			{
				InAirAttack();

				if (!isJumping)
					anim.SetTrigger("detonate");
				else
					anim.Play("Detonate");
			}
		}
	}
	public override void SpecialAttack()
	{
	    if (IsDodging)
	        return;

        if (!isSpecialAttackingAgile)
		{
			base.SpecialAttack();
			if (isAttacking)
			{
				if (currArchetype == ARCHETYPE.Standard)
				{
					anim.SetBool("specialAttackStandard", true);
				}
				else if (currArchetype == ARCHETYPE.Strong)
				{
					anim.SetTrigger("specialAttackStrong");

					isSpecialAttackingStrong = true;
					originPos = transform.position;

					morphStrongFX.gameObject.SetActive(false);
				}
				else
				{
					gameObject.layer = ignoreEnemyAttack;

					anim.SetTrigger("specialAttackAgile");
					isSpecialAttackingAgile = true;

					morphAgileFX.gameObject.SetActive(false);

					rigid.useGravity = false;
					ignoreFalling = true;

					GetComponent<Shadow>().Hide();
				}
			}
		}
	}

    public void PropulseCharacter()
    {
        //GetComponent<Rigidbody>().AddForce(new Vector3(0f, 100f, 0f), ForceMode.Impulse);
        //Jump();
        isPropulsed = true;
        anim.SetBool("isJumping", true);
        propulseYStart = transform.position.y;
        propulseTime = 0f;
    }

	public void ApplyMark(Enemy enemy)
	{
	    if (enemy.IsDead)
	        return;

		MarkSystem enemyMarkSystem = enemy.GetComponentInChildren<MarkSystem>();
		if (enemyMarkSystem != null)
		{
		    if (enemyMarkSystem.IsDetonate)
		        CreateNewMarkSystem(enemy, enemyMarkSystem);
		    else
		        enemyMarkSystem.NextLevel();
		}
		else
		{
		    CreateNewMarkSystem(enemy, enemyMarkSystem);
		}

		enemy.TakeDamage(lightAttacksDamage[(int)currArchetype]);
		if (enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockUp)
			enemy.CCMgr.Prone(floatingDuration, proneDuration);
	}

    private void CreateNewMarkSystem(Enemy enemy, MarkSystem enemyMarkSystem)
    {
        enemyMarkSystem = Instantiate(markSystemPrefab);
        enemyMarkSystem.OnDestroyed += MarkSystemDestroyed;
        enemyMarkSystem.transform.parent = enemy.transform;
        enemyMarkSystem.transform.localPosition = Vector3.zero;

        markSystems.Add(enemyMarkSystem);
    }

    protected override void AttackFinished()
	{
		base.AttackFinished();

		if (currArchetype != ARCHETYPE.Standard && (isSpecialAttackingStrong || isSpecialAttackingAgile))
		{
			if (currArchetype == ARCHETYPE.Agile)
			{
				gameObject.layer = playerLayer;
				rigid.useGravity = true;
				ignoreFalling = false;
			}

			ResetMorph();
		}

		GetComponent<Shadow>().Show();
		isSpecialAttackingStrong = false;
		isSpecialAttackingAgile = false;
    }

	public void HardFixSpecialAttackAgile()
	{
		ResetSpecialAttack();

		isSpecialAttackingStrong = false;

		anim.SetBool("isAttacking", false);
		anim.SetBool("specialAttackStandard", false);
	}

	protected override void CancelCurrentAction()
	{
		if (!isSpecialAttackingAgile)
		{
			base.CancelCurrentAction();

			isSpecialAttackingStrong = false;

			anim.SetBool("isAttacking", false);
			anim.SetBool("specialAttackStandard", false);
		}
    }

	private void Shoot()
	{
		if (isAttacking)
		{
			Bullet prefab;
			if (currArchetype == ARCHETYPE.Standard)
				prefab = bulletStandardPrefab;
			else if (currArchetype == ARCHETYPE.Strong)
				prefab = bulletStrongPrefab;
			else
				prefab = bulletAgilePrefab;

			Bullet newBullet = Instantiate(prefab, bulletSpawn.position, Quaternion.Euler(0f, (facingRight) ? 0f : 180f, 0f));
			newBullet.OnEnemyTouched += ApplyMark;

			newBullet.Init(bulletSpeed, bulletKnockBackForce, facingRight, currArchetype);

            newBullet.DestroyPausable(bulletLifeTime);
		}
	}
	private void Detonate()
	{
		if (currArchetype == ARCHETYPE.Standard)
		{
		    while (markSystems.Count != 0)
		        DetonateFirstMark();

		}
		else
		{
			for (int idx = 0; idx < markSystems.Count; ++idx)
				markSystems[idx].DetonateMarks(currArchetype, this);
		}
	}

    private void DetonateFirstMark()
    {
        if (markSystems.Count != 0)
        {
            if (markSystems[0] == null)
            {
                markSystems.RemoveAt(0);
                DetonateFirstMark();
            }
            else
                markSystems[0].DetonateMarks(currArchetype, this);

        }

    }

    private void ExecuteSpecialAttack(Enemy enemy)
	{
		if (currArchetype == ARCHETYPE.Standard)
			SpecialAttackStandard(enemy);
		else if (currArchetype == ARCHETYPE.Strong)
			SpecialAttackStrong(enemy);
		else
			SpecialAttackAgile(enemy);
	}

	private void SpecialAttackStandard(Enemy enemy)
	{
		if (isAttacking)
		{
			morphChannelCoroutine = StartCoroutine(MorphChannel(ARCHETYPE.Agile));

			enemy.CCMgr.Stun(morphChannelTime);

			catchEnemy = (enemy.IsDead) ? null : enemy;
		}
	}
	private void SpecialAttackStrong(Enemy enemy)
	{
		enemy.TakeDamage(specialAttacksDamage[(int)currArchetype]);
        enemy.CCMgr.KnockDown(knockDownDuration);
    }
	private void SpecialAttackAgile(Enemy enemy)
	{
		enemy.TakeDamage(specialAttacksDamage[(int)currArchetype]);
		enemy.CCMgr.Stun(laserBeamStunDuration);
    }

    public void PlayLaserSound()
    {
        if (!laserSound)
            return;

        if (laserSound.isPlaying)
            laserSound.Stop();

        laserSound.Play();
    }

	private void MarkSystemDestroyed(MarkSystem destroyed)
	{
		markSystems.Remove(destroyed);
	}

    #region Debug

    protected override bool CanAttack()
    {
        return base.CanAttack() && !isPropulsed;
    }
    protected override bool CanDodge()
    {
        return base.CanDodge() && !isPropulsed;
    }
    protected override bool CanJump()
    {
        return base.CanJump() && !isPropulsed;
    }

    void OnGUI()
    {
		if (showDebugButton) 
		{
			if (GUI.Button(new Rect(10, 40, 150, 100), "Strong"))
				MorphingTo(ARCHETYPE.Strong);

			if (GUI.Button(new Rect(10, 180, 150, 100), "Agile"))
				MorphingTo(ARCHETYPE.Agile);
		}
    }

    #endregion // Debug
}
