using System.Collections;
using UnityEngine;


[RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(Rigidbody))]
abstract public class Entity : MonoBehaviour
{
	[SerializeField]
	protected int maxLife = 1;
    public int MaxLife { get { return maxLife; } }
	protected int currLife = 1;
    public int CurrLife { get { return currLife; } }

	[SerializeField]
	private float hitFeedbackDuration = 1f; // to remove

	protected bool facingRight = true;
	public bool FacingRight
	{
		get { return facingRight; }
	}

	protected bool isDead = false;
	public bool IsDead
	{
		get { return isDead; }
	}

	protected CrowdControl ccMgr = null;
	public CrowdControl CCMgr
	{
		get { return ccMgr; }
	}

	protected Rigidbody rigid = null;
	public Rigidbody Rigid
	{
		get { return rigid; }
	}
	protected BoxCollider collid = null;
	public BoxCollider Collid
	{
		get { return collid; }
	}
	protected Vector3 extents = new Vector3(0f, 0f, 0f);
	public Vector3 Extents
	{
		get { return extents; }
	}

	protected int groundMask;
	public int GroundMask
	{
		get { return groundMask; }
	}
	protected int wallMask;
	public int WallMask
	{
		get { return wallMask; }
	}
	protected int cameraColliderMask;
	public int CameraColliderMask
	{
		get { return cameraColliderMask; }
	}

    // For debug text
    protected GUIStyle guiStyle = new GUIStyle();


	#region MonoBehaviour
	protected void Awake()
	{
		rigid = GetComponent<Rigidbody>();
		collid = GetComponent<BoxCollider>();
		extents = collid.bounds.extents;
		extents.x *= transform.localScale.x;
		extents.y *= transform.localScale.y;
		extents.z *= transform.localScale.z;

		currLife = maxLife;
	}

	protected void Start()
	{
        // For debug text
	    guiStyle.fontSize = 24;
	    guiStyle.normal.textColor = Color.white;

		ccMgr = GetComponent<CrowdControl>();

		groundMask = 1 << LayerMask.NameToLayer("Ground");
		wallMask = 1 << LayerMask.NameToLayer("Wall");
		cameraColliderMask = 1 << LayerMask.NameToLayer("CameraCollider");
	}
	#endregion

	public virtual void Kill()
    {
        if (isDead)
            return;

        currLife = 0;
        Die();
    }

    public virtual void Heal(int health)
    {
        currLife += health;

        if (currLife > maxLife)
            currLife = maxLife;
    }

	public virtual void TakeDamage(int damage)
	{
        if (isDead)
            return;

		currLife -= damage;
        if (currLife <= 0)
            Die();

		//if (currLife > 0f) // to remove
		//{
		//	if (this is Player)
		//		((Player)this).ChangeColor(Color.red);
		//	else
		//		GetComponent<SpriteRenderer>().color = Color.red;
		//	StartCoroutine(FeedbackHit());
		//}
	}

    protected virtual void Die()
    {
        isDead = true;
    }

    public void TeleportTo(Transform target)
    {
        transform.position = target.position;
        Vector3 targetRotation = target.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(targetRotation);

        if (targetRotation.y == 0)
            facingRight = true;
        else if (targetRotation.y == 180)
            facingRight = false;
    }

	public void ChangeFacingDirection(Vector3 direction)
	{
		if (direction.x > 0f && !facingRight)
		{
			facingRight = true;
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else if (direction.x < 0f && facingRight)
		{
			facingRight = false;
			transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		}
	}

	public bool IsCC()
	{
		return ccMgr.CurrCC != CrowdControl.CONTROL_TYPE.None;
	}

	private IEnumerator FeedbackHit() // to remove
	{
		yield return new WaitForSecondsPausable(hitFeedbackDuration);

		if (this is PlayerMelee)
			((PlayerMelee)this).ChangeColor(Color.white);
		else
			GetComponent<SpriteRenderer>().color = Color.white;
	}
}
