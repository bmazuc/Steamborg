using System.Collections;
using UnityEngine;

abstract public class Enemy : MovableEntity, IRespawnable
{
	public enum ENEMY_TYPE
	{
		Small,
		Medium,
		Big
	}

	[SerializeField]
	protected ENEMY_TYPE enemyType;
	public ENEMY_TYPE EnemyType
	{
		get { return enemyType; }
	}

	protected float zAcceptance = 3.5f;
	public float ZAcceptance
	{
		get { return zAcceptance; }
	}

	[HideInInspector]
	public Player nearestPlayer = null;

	[SerializeField]
	protected GameObject brain = null;

    [SerializeField]
    private float deathTime = 0.2f;

    [SerializeField]
    private bool disableOnAwake = false;

    //temp for respawn before spawner
    private Vector3 initialPos;
	public System.Action OnMoveToPointFinished;

	protected GameObject detectionZone = null;
	public GameObject DetectionZone
	{
		get { return detectionZone; }
	}

	private Vector3 pointToMove;
	private float acceptanceRadius = 1f;
	private bool moveTo = false;

    [SerializeField]
    private GameObject collectiblePrefab = null;

	private int ignorePlayerAttack;

    [SerializeField]
    private bool isDeathAnimationDone = false;

	#region MonoBehaviour
	protected new void Awake()
	{
		base.Awake();

		detectionZone = GetComponentInChildren<DetectionZone>().gameObject;

		ignorePlayerAttack = LayerMask.NameToLayer("IgnorePlayerAttack");
	}

	protected new void Start()
	{
        initialPos = transform.position;
		alliedUnitMask = 1 << LayerMask.NameToLayer("Enemy");
		base.Start();
	    int camCollMask = 1 << LayerMask.NameToLayer("CameraCollider");
	    moveMask &= ~camCollMask;

	    if (disableOnAwake)
            Anim.Play("FadeOut");

    }

	protected new void FixedUpdate()
	{
		if (!moveTo)
			base.FixedUpdate();
		else
		{
			Vector3 direction = (pointToMove - transform.position).normalized;

			inputDirection = direction;
			moveDirection = new Vector3(direction.x * horizontalMoveSpeed, direction.y, direction.z * verticalMoveSpeed);

			ChangeFacingDirection(moveDirection);

			Vector3 newPos = moveDirection * Time.fixedDeltaTime;
			transform.position += newPos;

			anim.SetBool("isWalking", true);

			if (Vector3.Distance(pointToMove, transform.position) < acceptanceRadius)
			{
				moveTo = false;
				rigid.useGravity = true;
				collid.enabled = true;
				detectionZone.SetActive(true);

				if (OnMoveToPointFinished != null)
					OnMoveToPointFinished();
			}
		}

        if (isFalling && (ccMgr.CurrCC != CrowdControl.CONTROL_TYPE.KnockUp || rigid.velocity.y < -4f) && rigid.useGravity)
            Fall();
	}
    #endregion

    protected virtual void OnCollisionExit(Collision collision)
    {
        if ((collision.collider.tag == "Ground" || collision.collider.tag == "PushableObject")
			&& ccMgr.CurrCC != CrowdControl.CONTROL_TYPE.KnockBack)
        {
            IsFalling = true;
			landOnGroundFX.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public override void Move(Vector3 direction)
	{
		Vector3 rayPositionOffset = new Vector3(collid.size.x * transform.localScale.x / 2f, 0f, 0f);
		Ray rightRay = new Ray(collid.bounds.center + rayPositionOffset, Vector3.down);
		Ray leftRay = new Ray(collid.bounds.center - rayPositionOffset, Vector3.down);

		float rayDistance = collid.size.y * transform.localScale.y / 2f + 1f;
		if ((!Physics.Raycast(rightRay, rayDistance, groundMask) && direction.x > 0f)
			|| (!Physics.Raycast(leftRay, rayDistance, groundMask) && direction.x < 0f)
			|| (Physics.Raycast(leftRay, 1f, cameraColliderMask) && direction.x < 0f && collid.bounds.center.x < Camera.main.transform.position.x)
			|| (Physics.Raycast(rightRay, 1f, cameraColliderMask) && direction.x > 0f && collid.bounds.center.x > Camera.main.transform.position.x))
			direction.x = 0f;

		base.Move(direction);
	}

	public void MoveToPoint(Vector3 point)
	{
		pointToMove = point;
		moveTo = true;
	}

	protected override void Die()
    {
        base.Die();

		brain.SetActive(false);
		gameObject.layer = ignorePlayerAttack;

		MarkSystem markSystem = GetComponentInChildren<MarkSystem>();
		if (markSystem != null)
			markSystem.MarkExpire();

        StartCoroutine(DeathCoroutine());
    }

	public virtual void Attack()
	{
		inputDirection = Vector3.zero;
		moveDirection = Vector3.zero;
	}

    #region Debug
    protected new void OnDrawGizmos()
	{
		base.OnDrawGizmos();

		if (nearestPlayer != null)
		{
			Vector3 dir = nearestPlayer.Collid.bounds.center - transform.position;
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(transform.position, dir);
		}
	}
	#endregion

    public void Respawn()
    {
        transform.position = initialPos;

        isDead = false;
        IsAttacking = false;
        currLife = MaxLife;
        brain.SetActive(true);
        Animator brainController = brain.GetComponent<Animator>();
        foreach (AnimatorControllerParameter param in brainController.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
                brainController.SetBool(param.name, false);
        }

        ccMgr.ResetCC();

        GetComponent<SpriteRenderer>().color = Color.white;
        GetComponent<FMODUnity.StudioEventEmitter>().Play();
    }

	private void StartFadeOut()
	{
		brain.SetActive(false);
	}
	private void EndFadeIn()
	{
		brain.SetActive(true);
	}

    public void SetDeathAnimationDone()
    {
        isDeathAnimationDone = true;
    }

    private IEnumerator DeathCoroutine()
    {
//        yield return new WaitForSecondsPausable(deathTime);

        while (isDeathAnimationDone == false)
            yield return null;

        if (collectiblePrefab)
            Instantiate(collectiblePrefab, transform.position, collectiblePrefab.transform.rotation);

        Destroy(gameObject);
    }
}
