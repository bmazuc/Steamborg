using UnityEngine;
using Anima2D;
using System.Collections.Generic;
using System;

abstract public class MovableEntity : Entity, IPausable
{
    [FMODUnity.EventRef]
    [SerializeField]
    private string hitSoundEventName = "";
    [SerializeField]
    private FMOD.Studio.STOP_MODE hitSoundStopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT;
    FMOD.Studio.EventInstance hitSoundEventInstance;

    [SerializeField]
	protected float horizontalMoveSpeed = 1f;
	public float HorizontalMoveSpeed
	{
		get { return horizontalMoveSpeed; }
	}
	[SerializeField]
	protected float verticalMoveSpeed = 1f;

	[SerializeField]
	protected float floatingDuration = 1f;
	[SerializeField]
	protected float proneDuration = 1f;

	protected Vector3 inputDirection = new Vector3(0f, 0f, 0f);
	protected Vector3 moveDirection = new Vector3(0f, 0f, 0f);
	public Vector3 MoveDirection
	{
		get { return moveDirection; }
	}

	[HideInInspector]
	protected bool isAttacking = false;
	public bool IsAttacking
	{
		get { return isAttacking; }
		set { isAttacking = value; anim.SetBool("isAttacking", value); }
	}

	protected Animator anim = null;
	public Animator Anim
	{
		get { return anim; }
	}
	[SerializeField]
	protected ParticleSystem landOnGroundFX = null;

	protected int playerMask;
	protected int enemyMask;
	protected int ignorePlayerAttackMask;
	protected int ignoreEnemyAttackMask;
	private int playerAttackMask;
	protected int alliedUnitMask;
	private int ignoreRaycastMask;
	protected int moveMask;
	protected int pushableObjectMask;
	public int PushableObjectMask
	{
		get { return pushableObjectMask; }
	}

	protected int playerLayer;
	private int enemyLayer;
	protected int enemyAttackLayer;

    protected bool isFalling;
	public bool IsFalling
	{
		get { return isFalling; }
		set { isFalling = value; if (this is PlayerMelee) anim.SetBool("isFalling", value); }
	}

    public bool isStunned = false;
    [SerializeField]
    protected float fallSpeed = -50f;

    private Vector3 pauseVelocity;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    private List<Animation> animationList = new List<Animation>();
    private List<UnityChan.SpringManager> springManager = new List<UnityChan.SpringManager>();

    public Action OnDie;

    #region MonoBehaviour
    protected new void Start()
	{
		base.Start();
        GameManager.Instance.PauseManager.AddPausable(this);

		anim = GetComponent<Animator>();

		playerMask = 1 << LayerMask.NameToLayer("Player");
		enemyMask = 1 << LayerMask.NameToLayer("Enemy");
		ignorePlayerAttackMask = 1 << LayerMask.NameToLayer("IgnorePlayerAttack");
		ignoreEnemyAttackMask = 1 << LayerMask.NameToLayer("IgnoreEnemyAttack");
		playerAttackMask = 1 << LayerMask.NameToLayer("PlayerAttack");
		ignoreRaycastMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
		pushableObjectMask = 1 << LayerMask.NameToLayer("PushableObject");

		moveMask = ~enemyMask & ~playerAttackMask & ~alliedUnitMask & ~ignoreRaycastMask & ~playerMask & ~ignorePlayerAttackMask & ~ignoreEnemyAttackMask;

		playerLayer = LayerMask.NameToLayer("Player");
		enemyLayer = LayerMask.NameToLayer("Enemy");
		enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");

        if (hitSoundEventName != "")
            hitSoundEventInstance = FMODUnity.RuntimeManager.CreateInstance(hitSoundEventName);

        particleSystems.AddRange(GetComponents<ParticleSystem>());
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>(true));

        animationList.AddRange(GetComponents<Animation>());
        animationList.AddRange(GetComponentsInChildren<Animation>(true));

        springManager.AddRange(GetComponents<UnityChan.SpringManager>());
        springManager.AddRange(GetComponentsInChildren<UnityChan.SpringManager>());

        IsFalling = true;
    }

	protected void FixedUpdate()
	{
		Vector3 direction = inputDirection.normalized;
		direction.x *= extents.x;
		direction.z *= extents.z;

		if (!Physics.BoxCast(collid.bounds.center, new Vector3(collid.size.x * transform.localScale.x / 2f * 0.9f, collid.size.y * transform.localScale.y / 2f * 0.9f, collid.size.z * transform.localScale.z / 2f * 0.9f), direction, Quaternion.identity, collid.size.x * transform.localScale.x * 0.05f, moveMask))
		{
			Vector3 newPos = moveDirection * Time.fixedDeltaTime;
			transform.position += newPos;
		}
	}

    void OnDestroy()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.RemovePausable(this);
    }
	#endregion

	public virtual void Move(Vector3 direction)
	{
		inputDirection = direction;

		if (!isAttacking)
		{
			Vector3 dir = new Vector3(direction.x * horizontalMoveSpeed, direction.y, direction.z * verticalMoveSpeed);
			moveDirection = dir;

			anim.SetBool("isWalking", moveDirection.x != 0f || moveDirection.z != 0f);

			ChangeFacingDirection(moveDirection);
		}
		else
			moveDirection = Vector3.zero;
	}

	protected virtual void AttackFinished()
	{
		IsAttacking = false;
	}

	protected virtual void CancelCurrentAction()
	{
		IsAttacking = false;
	}

	protected override void Die()
	{
		base.Die();

	    anim.SetBool("isDead", true);

        inputDirection = Vector3.zero;
		moveDirection = Vector3.zero;

        if (OnDie != null)
            OnDie();
	}

    protected void Fall()
    {
        Vector3 fall = transform.position;
        fall.y += fallSpeed * Time.deltaTime;
        transform.position = fall;

		if (Physics.BoxCast(collid.bounds.center, new Vector3(collid.size.x * transform.localScale.x / 2f, collid.size.y * transform.localScale.y / 2f * 0.1f, collid.size.z * transform.localScale.z / 2f), Vector3.down, Quaternion.identity, collid.size.y * transform.localScale.y / 2f, groundMask | pushableObjectMask))
		{
			IsFalling = false;
			landOnGroundFX.Play();
		}
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (currLife > 0f)
        {
            isStunned = true;

            if (hitSoundEventInstance.isValid())
            {
                hitSoundEventInstance.stop(hitSoundStopMode);
                hitSoundEventInstance.start();
            }
        }
    }

    public void ResetStunned()
    {
        isStunned = false;
    }

    public virtual void ChangeColor(Color color)
    {
        SpriteMeshInstance[] sMI = GetComponentsInChildren<SpriteMeshInstance>();

        foreach (SpriteMeshInstance meshRenderer in sMI)
            meshRenderer.color = color;
    }

    #region Debug
    protected virtual void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Vector3 direction = inputDirection.normalized;
		direction.x *= extents.x;
		direction.z *= extents.z;
		Gizmos.DrawRay(transform.position, direction);

		Gizmos.color = Color.red;
		BoxCollider collider = GetComponent<BoxCollider>();
		Gizmos.DrawWireCube(collider.bounds.center, new Vector3(collider.size.x * transform.localScale.x, collider.size.y * transform.localScale.y, collider.size.z * transform.localScale.z));
	}
	#endregion

    public virtual void OnPause()
    {
        enabled = false;
        anim.enabled = false;

        pauseVelocity = rigid.velocity;
        rigid.Sleep();
        foreach (ParticleSystem sys in particleSystems)
            sys.Pause();

        foreach (Animation anim in animationList)
            anim.enabled = false;

        foreach (UnityChan.SpringManager springManager in springManager)
            springManager.enabled = false;
    }

    public virtual void OnResume()
    {
        enabled = true;
        anim.enabled = true;

        rigid.WakeUp();
        rigid.velocity = pauseVelocity;
        foreach (ParticleSystem sys in particleSystems)
            sys.Play();

        foreach (Animation anim in animationList)
            anim.enabled = true;

        foreach (UnityChan.SpringManager springManager in springManager)
            springManager.enabled = true;
    }

    public void PlayFModEvent(string path)
    {
        FMOD.Studio.EventInstance fmodEvent = FMODUnity.RuntimeManager.CreateInstance(path);
        fmodEvent.start();
    }
}
