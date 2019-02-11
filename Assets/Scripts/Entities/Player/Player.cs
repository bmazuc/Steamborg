using System.Collections;
using Anima2D;
using UnityEngine;
using System;


abstract public class Player : MovableEntity, IRespawnable
{
    public enum ARCHETYPE
    {
        Standard,
        Strong,
        Agile
    }

	[SerializeField]
	protected AnimationCurve jumpCurve;
	[SerializeField]
	protected float jumpHeight = 1f;
	[SerializeField]
	protected float jumpSpeed = 1f;
	protected float jumpYStart;
	protected float jumpTime;
	protected bool isJumping = false;
	public bool IsJumping
	{
		get { return isJumping; }
	}

	[SerializeField]
	protected float inAirAttackTimer = 0.5f;
	protected float inAirAttackCurrTime;
	protected bool inAirAttack = false;
	protected bool forceFallDown = false;

	[SerializeField]
	protected float dodgeSpeed = 1f;
	[SerializeField]
	protected float dodgeRange = 1f;
	[SerializeField]
	protected float dodgeInvulnerabilityDuration = 0.8f;
	private float currDodgeRange;
	protected bool isDodging = false;
	public bool IsDodging
	{
		get { return isDodging; }
	}
	protected Vector3 dodgeDirection = new Vector3(0f, 0f, 0f);
	[SerializeField]
	private float dodgeCooldown = 1f;
	private bool dodgeCooling = false;

	[SerializeField]
	protected ParticleSystem morphStrongFX = null;
	[SerializeField]
	protected ParticleSystem morphAgileFX = null;
	[SerializeField]
	protected ParticleSystem morphAbsorbEndFX = null;
	[SerializeField]
	protected float morphChannelTime = 1f;
	protected Coroutine morphChannelCoroutine = null;
	[SerializeField]
    protected float morphDuration = 1f;
	public float MorphDuration
	{
		get { return morphDuration; }
	}
	protected float morphTime;
	public float MorphTime
	{
		get { return morphTime; }
	}
	protected Enemy catchEnemy = null;

	[SerializeField]
	protected float knockDownDuration = 1f;

    [SerializeField]
    protected float respawnTime = 3f;

	[SerializeField]
	protected ParticleSystem pickUpCollectibleFX = null;
	[SerializeField]
	protected float pickUpCollectibleFXDuration = 1f;
	protected Coroutine pickUpCollectibleCoroutine = null;
	[SerializeField]
	protected ParticleSystem respawnFX = null;

	protected ARCHETYPE currArchetype = ARCHETYPE.Standard;

    [SerializeField]
    private bool showArchetypeDebug = false;

    [SerializeField]
    private string characterName;
    public string CharacterName { get { return characterName; } }

	public Action OnMorphUpdate;
    public Action OnTakeDamage;
    public Action OnHealth;

	private Vector2 shadowInitialScale = new Vector2(0f, 0f);

    private Controller controller;

	protected bool ignoreFalling = false;

	protected int ignoreEnemyAttack;

    protected bool isSpecialAttacking = false;


    #region MonoBehaviour
    protected new void Start()
    {
		alliedUnitMask = 1 << LayerMask.NameToLayer("Player");

        controller = GetComponent<Controller>();

		ignoreEnemyAttack = LayerMask.NameToLayer("IgnoreEnemyAttack");

		base.Start();
	}

	protected new void FixedUpdate()
	{
		if (!isDodging)
			base.FixedUpdate();
		else
		{
			Vector3 newPos = new Vector3(dodgeDirection.x * horizontalMoveSpeed, 0f, dodgeDirection.z * verticalMoveSpeed) * Time.fixedDeltaTime;
			newPos *= dodgeSpeed;

			currDodgeRange -= newPos.magnitude;

			if (currDodgeRange <= 0f)
			{
				isDodging = false;
				StartCoroutine(DodgeCooldown());
				anim.SetBool("isDodging", false);
				//Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
			}

			transform.position += newPos;
		}

        if (isFalling)
            Fall();

        if (isJumping)
        {
            if (jumpTime > 1f)
            {
                IsFalling = true;
                isJumping = false;
            }

            if (Physics.Raycast(new Ray(transform.position, Vector3.up), (Vector3.up * extents.y).magnitude + 0.1f, groundMask | pushableObjectMask))
                jumpTime = GetJumpTimeFallOff(jumpTime);

            float y = jumpCurve.Evaluate(jumpTime) * jumpHeight;

            Vector3 newPos = transform.position;
            newPos.y = y + jumpYStart;
            transform.position = newPos;

            if (!inAirAttack)
                jumpTime += Time.fixedDeltaTime * jumpSpeed;
        }

		if (inAirAttack)
		{
			inAirAttackCurrTime -= Time.fixedDeltaTime;
			if (inAirAttackCurrTime <= 0f)
				inAirAttack = false;
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
        if ((collision.collider.tag == "Ground" || collision.collider.tag == "PushableObject")
            && Physics.BoxCast(collid.bounds.center, new Vector3(collid.size.x * transform.localScale.x / 2f * 0.5f, collid.size.y / 2f * 0.1f, collid.size.z / 2f), Vector3.down, Quaternion.identity, collid.size.y / 2f, groundMask | pushableObjectMask))
		{
			isJumping = false;
			anim.SetBool("isJumping", false);
			forceFallDown = false;
			landOnGroundFX.Play();
		}
	}

    protected virtual void OnCollisionExit(Collision collision)
    {
        if ((collision.collider.tag == "Ground" || collision.collider.tag == "PushableObject") && !isJumping && !ignoreFalling)
        {
            IsFalling = true;
			landOnGroundFX.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}
	#endregion

	public void Jump()
	{
		if (CanJump())
		{
			isJumping = true;
			anim.SetBool("isJumping", true);
			jumpYStart = transform.position.y;
			jumpTime = 0f;
			landOnGroundFX.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public void Dodge()
	{
		if (CanDodge())
		{
			if (this is PlayerRanged)
				((PlayerRanged)this).HardFixSpecialAttackAgile();

			if (inputDirection.x == 0f && inputDirection.z == 0f)
			{
				inputDirection = (facingRight) ? Vector3.right : Vector3.left;
			}

			Vector3 direction = inputDirection;
			direction.x *= extents.x;
			direction.z *= extents.z;

			currDodgeRange = dodgeRange;

			RaycastHit hit;
			if (Physics.Raycast(new Ray(transform.position, direction), out hit, direction.magnitude + dodgeRange, wallMask | pushableObjectMask | groundMask))
			{
				//Vector3 distanceToPass = (hit.point - transform.position);
				//distanceToPass.x *= hit.collider.bounds.extents.x;
				//distanceToPass.z *= hit.collider.bounds.extents.z;

				//if (distanceToPass.magnitude > dodgeRange)
					//currDodgeRange = hit.distance - direction.magnitude;

				currDodgeRange = hit.distance - direction.magnitude;
			}

			if (currDodgeRange > 0.1f)
			{
				dodgeDirection = inputDirection.normalized;
				isDodging = true;
				anim.SetBool("isDodging", true);

				//Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);
				StartCoroutine(DodgeInvulnerability());
			}
		}
	}

	public virtual void LightAttack()
	{
		if (!forceFallDown)
			IsAttacking = CanAttack();
	}
	public virtual void StrongAttack()
	{
		if (!forceFallDown)
			IsAttacking = CanAttack();
	}
	public virtual void SpecialAttack()
	{
		if (!forceFallDown)
			IsAttacking = CanAttack() && !isJumping;
	}

	public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (currLife > 0f)
        {
            if (ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.None || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockBack || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockUp || ccMgr.CurrCC == CrowdControl.CONTROL_TYPE.Prone)
                anim.Play("Hit", -1, 0f);
        }

        if (OnTakeDamage != null)
            OnTakeDamage();
    }

    public override void Heal(int health)
    {
        base.Heal(health);

        if (OnHealth != null)
            OnHealth();

		if (pickUpCollectibleCoroutine != null)
		{
			StopCoroutine(pickUpCollectibleCoroutine);
			pickUpCollectibleCoroutine = null;
		}

		pickUpCollectibleCoroutine = StartCoroutine(PickUpCollectibleFXTimer());
    }

    public void StopMorphAttack()
	{
		if (currArchetype == ARCHETYPE.Standard)
		{
			IsAttacking = false;

			if (catchEnemy != null)
			{
				catchEnemy.CCMgr.ResetCC();
				catchEnemy = null;
			}

			anim.SetBool("specialAttackStandard", false);
			anim.SetBool("morphChannel", false);

			if (morphChannelCoroutine != null)
			{
				StopCoroutine(morphChannelCoroutine);
				morphChannelCoroutine = null;
			}
		}
	}
	public void ResetMorph(bool colorDebug = true)
	{
		if (currArchetype == ARCHETYPE.Strong)
		{
			morphStrongFX.Stop();
			morphStrongFX.gameObject.SetActive(false);
		}
		else
		{
			morphAgileFX.Stop();
			morphAgileFX.gameObject.SetActive(false);
		}

		morphTime = 0f;
		currArchetype = ARCHETYPE.Standard;

		if (OnMorphUpdate != null)
			OnMorphUpdate();

	    //if (colorDebug)
     //       ChangeColor(Color.white);
	}

	protected override void AttackFinished()
	{
		base.AttackFinished();
		if (inAirAttack)
		{
			inAirAttack = false;
			jumpTime = GetJumpTimeFallOff(jumpTime);
		}
	}

	protected void SpecialStandardFinished()
	{
		if (catchEnemy != null)
		{
			IsAttacking = false;
			anim.SetBool("specialAttackStandard", false);
		}
	}

    protected override void CancelCurrentAction()
    {
        base.CancelCurrentAction();
        ResetSpecialAttack();
    }

    protected override void Die()
    {
        base.Die();

        GameManager.Instance.CameraManager.PlayerDeathLossFocus(this);
        //GameManager.Instance.PauseManager.RemovePausable(this);

        if (GameManager.Instance.HasTwoPlayer)
            StartCoroutine(LaunchRespawn());

    }

    private IEnumerator LaunchRespawn()
    {
        yield return new WaitForSecondsPausable(respawnTime);
        
        GameManager gM = GameManager.Instance;
        bool isP1 = (gM.Player1 == this);

        if ((isP1 && !gM.Player2.isDead) || (!isP1 && !gM.Player1.isDead))
        {
            Vector3 position = (isP1) ? gM.Player2.transform.position : gM.Player1.transform.position;
            RespawnAt(position);
        }
    }

    protected void MorphingTo(ARCHETYPE morphType)
	{
		if (currArchetype == ARCHETYPE.Standard)
		{
			morphAbsorbEndFX.gameObject.SetActive(true);
			morphAbsorbEndFX.Play();

			currArchetype = morphType;

			if (currArchetype == ARCHETYPE.Strong)
			{
				morphStrongFX.gameObject.SetActive(true);
				morphStrongFX.Play();
			}
			else
			{
				morphAgileFX.gameObject.SetActive(true);
				morphAgileFX.Play();
			}

			StartCoroutine(Morph());
		}
	}

	protected void InAirAttack()
	{
		if (!forceFallDown)
		{
			inAirAttack = true;
			inAirAttackCurrTime = inAirAttackTimer;
		}
	}

	protected virtual bool CanAttack()
	{
		return !isDodging;
	}
	protected virtual bool CanDodge()
	{
		return /*!isAttacking &&*/ !isJumping && !isDodging && !dodgeCooling && !isSpecialAttacking;
	}
    protected virtual bool CanJump()
	{
		return !isDodging && !isJumping && !isAttacking;
	}

	protected float GetJumpTimeFallOff(float time)
	{
		float currTime = jumpCurve.Evaluate(time);
		float fallOffTime = jumpCurve.Evaluate(time + Time.deltaTime);

		while (currTime < fallOffTime)
		{
			time += Time.deltaTime;
			fallOffTime = jumpCurve.Evaluate(time);
		}

		return time;
	}

	protected IEnumerator MorphChannel(ARCHETYPE archetype)
	{
		anim.SetBool("morphChannel", true);

		yield return new WaitForSecondsPausable(morphChannelTime);

		if (catchEnemy != null)
		{
			MorphingTo(archetype);
			catchEnemy.CCMgr.KnockDown(knockDownDuration);

			IsAttacking = false;
			catchEnemy = null;
			morphChannelCoroutine = null;
		}

		anim.SetBool("specialAttackStandard", false);
		anim.SetBool("morphChannel", false);
	}

	private IEnumerator Morph()
	{
		morphTime = morphDuration;

		while (morphTime > 0f)
		{
			if (!GameManager.Instance.PauseManager.GameIsPaused)
			{
				morphTime -= Time.deltaTime;

				if (OnMorphUpdate != null)
					OnMorphUpdate();
			}

			yield return null;
		}

		if (currArchetype == ARCHETYPE.Strong)
		{
			morphStrongFX.Stop();
			morphStrongFX.gameObject.SetActive(false);
		}
		else
		{
			morphAgileFX.Stop();
			morphAgileFX.gameObject.SetActive(false);
		}

		currArchetype = ARCHETYPE.Standard;
        //ChangeColor(Color.white);
	}

	private IEnumerator DodgeCooldown()
	{
		dodgeCooling = true;
		yield return new WaitForSecondsPausable(dodgeCooldown);

		dodgeCooling = false;
	}
	private IEnumerator DodgeInvulnerability()
	{
		gameObject.layer = ignoreEnemyAttack;

		yield return new WaitForSecondsPausable(dodgeInvulnerabilityDuration);

		gameObject.layer = playerLayer;
	}

	private IEnumerator PickUpCollectibleFXTimer()
	{
		pickUpCollectibleFX.gameObject.SetActive(true);
		pickUpCollectibleFX.Play();

		yield return new WaitForSecondsPausable(pickUpCollectibleFXDuration);

		pickUpCollectibleFX.Stop();
		pickUpCollectibleFX.gameObject.SetActive(false);

		pickUpCollectibleCoroutine = null;
	}

    public void ResetSpecialAttack()
    {
        isSpecialAttacking = false;
    }

    public void SetSpecialAttack()
    {
        isSpecialAttacking = true;
    }

    #region Debug
    protected new void OnDrawGizmos()
	{
		base.OnDrawGizmos();

		Gizmos.color = Color.magenta;
		Gizmos.DrawRay(transform.position, (Vector3.up * extents.y));
        Gizmos.DrawWireCube(transform.position + collid.center, new Vector3(extents.x / 2f, extents.y / 3f, extents.z));
        Gizmos.DrawWireCube(transform.position + collid.center - (new Vector3(0f, Extents.y, 0f) / transform.localScale.y), new Vector3(extents.x / 2f, extents.y / 3f, extents.z));
        Vector3 direction = inputDirection.normalized;
		direction.x *= extents.x;
		direction.z *= extents.z;

		if (isDodging)
		{
			direction.x *= dodgeRange;
			direction.z *= dodgeRange;
			Gizmos.color = Color.red;
			Gizmos.DrawRay(transform.position, direction);
		}
	}

    protected void OnGUI()
    {
        if (showArchetypeDebug)
        {
            Vector3 screenpos = Camera.main.WorldToScreenPoint(transform.position);
            GUI.Label(new Rect(screenpos.x - 160f, Screen.height - (screenpos.y + 135f), 250f, 250f), "Archetype : " + currArchetype, guiStyle);
        }
    }
    #endregion

    public void Respawn()
    {
        Checkpoint curCheckpoint = GameManager.Instance.CheckpointManager.CurrentCheckpoint;

        if (!curCheckpoint)
            return;
        
        RespawnAt(curCheckpoint.transform.position);

        //transform.position = curCheckpoint.transform.position;
        //currLife = maxLife;
        //isDead = false;
        //if (anim)
        //    anim.SetBool("isDead", false);
        ////temp until real death anim
        //transform.rotation = Quaternion.identity;

        //GameManager gameManager = GameManager.Instance;
        //if (gameManager.HasTwoPlayer)
        //    gameManager.CameraManager.AddTransformToCameraGroup(transform, (gameManager.Player1 == this) ? 0 : 1);
        //else if (gameManager.Player1 == this)
        //    gameManager.CameraManager.AddTransformToCameraGroup(transform, 0);
    }

    private void RespawnAt(Vector3 position)
    {
        transform.position = position;
        currLife = maxLife;
        isDead = false;
        if (anim)
            anim.SetBool("isDead", false);

		if (OnHealth != null)
			OnHealth();

        transform.rotation = Quaternion.identity;

        GameManager gameManager = GameManager.Instance;
        if (gameManager.HasTwoPlayer)
            gameManager.CameraManager.AddTransformToCameraGroup(transform, (gameManager.Player1 == this) ? 0 : 1);
        else if (gameManager.Player1 == this)
            gameManager.CameraManager.AddTransformToCameraGroup(transform, 0);

		respawnFX.gameObject.SetActive(true);
		respawnFX.Play();
    }

    public override void OnPause()
    {
        base.OnPause();
        controller.enabled = false;
    }

    public override void OnResume()
    {
        base.OnResume();
        controller.enabled = true;
        controller.CheckState();
    }
}
