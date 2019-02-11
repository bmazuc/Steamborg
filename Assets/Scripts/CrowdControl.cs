using System.Collections;
using UnityEngine;


[RequireComponent(typeof(BoxCollider)), RequireComponent(typeof(Rigidbody))]
public class CrowdControl : MonoBehaviour, IPausable
{
	public enum CONTROL_TYPE
	{
		None,
		KnockUp,
		KnockDown,
		KnockBack,
		Grab,
		Stun,
		Prone
	}

	private CONTROL_TYPE crowdControl = CONTROL_TYPE.None;
	public CONTROL_TYPE CurrCC
	{
		get { return crowdControl; }
	}

	private Entity self;

	private Vector3 grabPosition;
	private float grabSpeed;
	private float stunDurationAfterGrab;

	private Vector3 knockBackDir;

	private float knockDownDurationAfterProne = 1f;

	private bool hitInAir = false;

    // For debug text
    [SerializeField]
    private bool showDebugTest = false;
    private GUIStyle guiStyle = new GUIStyle();
    private float stunDurationnLeft = 0f;

    private IEnumerator currCoroutine = null;
	private Coroutine knockBackCoroutine = null;


	protected void Awake()
	{
	    // For debug text
        guiStyle.fontSize = 24;
	    guiStyle.normal.textColor = Color.green;

        self = GetComponent<Entity>();

        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.AddPausable(this);
	}

    void OnDestroy()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.RemovePausable(this);
    }

	protected void Update()
	{
		if (crowdControl == CONTROL_TYPE.Grab)
		{
			Vector3 newPos = transform.localPosition;
			newPos.x -= grabSpeed * Time.deltaTime;
			transform.localPosition = newPos;

			if (newPos.x <= grabPosition.x)
			{
				crowdControl = CONTROL_TYPE.None;
				transform.parent = null;
				self.Rigid.useGravity = true;

				Stun(stunDurationAfterGrab);
			}
		}

		if (self.Rigid.velocity.x != 0f
			&& Physics.BoxCast(self.Collid.bounds.center, new Vector3(self.Collid.size.x * self.transform.localScale.x / 2f * 0.5f, self.Collid.size.y * self.transform.localScale.y / 2f, self.Collid.size.z * self.transform.localScale.z / 2f), knockBackDir, Quaternion.identity, self.Extents.x, self.GroundMask | self.WallMask | self.CameraColliderMask))
		{
			self.Rigid.velocity = new Vector3(0f, 0f, 0f);
			((MovableEntity)self).Move(Vector3.zero);
		}

        // For Debug text behaviour
	    stunDurationnLeft -= Time.deltaTime;

	}

	public void KnockUp(float force)
	{
		StopPreviousCC();
		self.Rigid.AddForce(Vector3.up * force, ForceMode.Impulse);
		crowdControl = CONTROL_TYPE.KnockUp;
		hitInAir = true;

	    ((MovableEntity)self).Anim.Play("Propulsed"); 
	    ((MovableEntity)self).Anim.SetBool("isPropulsed", true);

    }
	public void KnockDown(float duration)
	{
	    //if (self.IsDead)
	    //    return;

		StopPreviousCC();
		crowdControl = CONTROL_TYPE.KnockDown;
        //if (self is Enemy)
        //	GetComponent<SpriteRenderer>().color = Color.gray;
        //else if (self is Player)
        //	((Player)self).ChangeColor(Color.grey);
	    ((MovableEntity)self).Anim.SetBool("isKnockdown", true);
	    ((MovableEntity)self).Anim.SetBool("isPropulsed", false);
        currCoroutine = CrowdControlTimer(duration);
	    stunDurationnLeft = duration; // for debug

        StartCoroutine(currCoroutine);

		//if (self is PlayerMelee || self is EnemyStrong)
		((MovableEntity)self).Anim.Play("Knockdown");
	}
	public void KnockBack(Vector3 direction, float force)
	{
		StopPreviousCC();

		knockBackDir = direction;

		if (!Physics.BoxCast(self.Collid.bounds.center, new Vector3(self.Collid.size.x * self.transform.localScale.x / 2f * 0.9f, self.Collid.size.y * self.transform.localScale.y / 2f, self.Collid.size.z * self.transform.localScale.z / 2f), direction, Quaternion.identity, 2f, self.GroundMask | self.WallMask | self.CameraColliderMask))
		{
			direction *= force;
			direction.y += Mathf.Abs(Physics.gravity.y) / 15f;
			self.Rigid.AddForce(direction, ForceMode.Impulse);
			crowdControl = CONTROL_TYPE.KnockBack;

			knockBackCoroutine = StartCoroutine(KnockBackReset());
		}
	}
	public void Grab(Transform grabbedBy, Vector3 colliderExtents, float speed, float stunDurationAfter)
	{
		StopPreviousCC();
		crowdControl = CONTROL_TYPE.Grab;
		transform.parent = grabbedBy;
		self.Rigid.useGravity = false;

		grabPosition = new Vector3(colliderExtents.x + self.Collid.size.x / 2f, transform.localPosition.y, transform.localPosition.z);
		if (!self.FacingRight)
			grabPosition.x *= -1f;

		grabSpeed = speed;
		stunDurationAfterGrab = stunDurationAfter;
	}
	public void Stun(float duration)
	{
		StopPreviousCC();
		crowdControl = CONTROL_TYPE.Stun;
		currCoroutine = CrowdControlTimer(duration);
        stunDurationnLeft = duration; // for debug

        StartCoroutine(currCoroutine);
	}
	public void Prone(float duration, float knockDownDuration)
	{
		StopPreviousCC();
		crowdControl = CONTROL_TYPE.Prone;
		currCoroutine = CrowdControlTimer(duration);
		self.Rigid.useGravity = false;
		self.Rigid.velocity = Vector3.zero;
		knockDownDurationAfterProne = knockDownDuration;
		hitInAir = true;
		stunDurationnLeft = duration; // to remove

		StartCoroutine(currCoroutine);
	}

	public void ResetCC()
	{
		if (currCoroutine != null)
		{
			StopCoroutine(currCoroutine);
			currCoroutine = null;
		}

		if (crowdControl == CONTROL_TYPE.Prone)
			self.Rigid.useGravity = true;

		crowdControl = CONTROL_TYPE.None;
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "Ground")
		{
			if (hitInAir)
			{
				KnockDown(knockDownDurationAfterProne);
				hitInAir = false;
			}
			else if (crowdControl == CONTROL_TYPE.KnockBack)
			{
				crowdControl = CONTROL_TYPE.None;
			}
		}
	}

	public void StopPreviousCC()
	{
		if (currCoroutine != null)
			StopCoroutine(currCoroutine);

		if (knockBackCoroutine != null)
		{
			StopCoroutine(knockBackCoroutine);
			knockBackCoroutine = null;
		}

		if (crowdControl == CONTROL_TYPE.Prone)
			self.Rigid.useGravity = true;
	}

	private IEnumerator CrowdControlTimer(float duration)
	{
		yield return new WaitForSecondsPausable(duration);

		currCoroutine = null;

		if (crowdControl == CONTROL_TYPE.Prone)
			self.Rigid.useGravity = true;
		else
		{
		    crowdControl = CONTROL_TYPE.None;
            ((MovableEntity)self).Anim.SetBool("isPropulsed", false);
            bool knock = ((MovableEntity)self).Anim.GetBool("isKnockdown");
		    if (knock)
		        ((MovableEntity)self).Anim.SetBool("isKnockdown", false);
        }

		((MovableEntity)self).isStunned = false;
	}
	private IEnumerator KnockBackReset()
	{
		yield return new WaitForSecondsPausable(1f);

		if (crowdControl == CONTROL_TYPE.KnockBack)
			crowdControl = CONTROL_TYPE.None;

		knockBackCoroutine = null;
	}

    private void OnGUI()
    {
        if (showDebugTest)
        {
            Vector3 screenpos = Camera.main.WorldToScreenPoint(transform.position);

            if (crowdControl != CONTROL_TYPE.None)
            {
                string cTime = stunDurationnLeft > 0f ? stunDurationnLeft.ToString() : "0.0";
                GUI.Label(new Rect(screenpos.x - 160f, Screen.height - (screenpos.y + 155f), 250f, 250f), "Is on crowd control : " + crowdControl + " | Time : " + cTime, guiStyle);
            }

        }
    }

    public void OnPause()
    {
        enabled = false;
    }

    public void OnResume()
    {
        enabled = true;
    }
}
