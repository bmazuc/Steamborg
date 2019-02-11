using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class PushableBloc : MonoBehaviour, IPausable
{
	[SerializeField]
	private float maxLeftPosition = 1f;
	[SerializeField]
	private float maxRightPosition = 1f;

	[SerializeField]
	private float moveSpeedModifier = 3f;

	[SerializeField]
	private ParticleSystem moveFX = null;

	private AudioSource moveSFX = null;

	private BoxCollider collid = null;
	private Vector3 extents = Vector3.zero;

	private int playerMask;
	private int groundMask;

	private float xStartPos;
	private int numberOfPlayer = 0;

	[SerializeField]
	private float fallSpeed = -10f;
	private bool fall = true;

    bool moveFXisPlaying = false;

	private void Awake()
	{
		collid = GetComponent<BoxCollider>();
		extents = collid.bounds.extents;
		extents.x *= transform.localScale.x;
		extents.y *= transform.localScale.y;
		extents.z *= transform.localScale.z;

		playerMask = 1 << LayerMask.NameToLayer("Player");
		groundMask = 1 << LayerMask.NameToLayer("Ground");

		xStartPos = transform.position.x;

		moveSFX = GetComponent<AudioSource>();

        GameManager.Instance.PauseManager.AddPausable(this);
	}

    void OnDestroy()
    {
        GameManager.Instance.PauseManager.RemovePausable(this);
    }

	private void FixedUpdate()
	{
		if (fall)
		{
			Vector3 newPos = transform.position;
			newPos.y += fallSpeed * Time.deltaTime;
			transform.position = newPos;

			if (Physics.BoxCast(collid.bounds.center, extents, Vector3.down, Quaternion.identity, 0.2f, groundMask))
				fall = false;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "Player")
		{
			++numberOfPlayer;
			
			if (Physics.BoxCast(collid.bounds.center, new Vector3(extents.x * 0.9f, extents.y * 0.9f, extents.z), Vector3.right, Quaternion.identity, 1f, playerMask)
				|| Physics.BoxCast(collid.bounds.center, new Vector3(extents.x * 0.9f, extents.y * 0.9f, extents.z), Vector3.left, Quaternion.identity, 1f, playerMask))
			{
				if (numberOfPlayer == 1)
				{
					moveFX.gameObject.SetActive(true);
					moveFX.Play();
                    moveFXisPlaying = true;

					moveSFX.Play();
				}
			}
		}
	}
	private void OnCollisionStay(Collision collision)
	{
		if (collision.collider.tag == "Player")
		{
			if (Physics.BoxCast(collid.bounds.center, new Vector3(extents.x * 0.9f, extents.y, extents.z), Vector3.right, Quaternion.identity, 1f, playerMask))
			{
				PushLeft(collision.gameObject.GetComponent<Player>().HorizontalMoveSpeed);
			}

			if (Physics.BoxCast(collid.bounds.center, new Vector3(extents.x * 0.9f, extents.y, extents.z), Vector3.left, Quaternion.identity, 1f, playerMask))
			{
				PushRight(collision.gameObject.GetComponent<Player>().HorizontalMoveSpeed);
			}
		}
	}
	private void OnCollisionExit(Collision collision)
	{
		if (collision.collider.tag == "Player")
		{
			--numberOfPlayer;

			if (numberOfPlayer == 0)
			{
				moveFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                moveFXisPlaying = false;

				moveSFX.Stop();
            }
		}
	}

	private void PushRight(float speed)
	{
		if (transform.position.x - xStartPos < maxRightPosition)
		{
			Vector3 newPos = transform.position;
			newPos.x += speed / moveSpeedModifier * Time.deltaTime;
			transform.position = newPos;
		}
	}
	private void PushLeft(float speed)
	{
		if (transform.position.x - xStartPos > maxLeftPosition)
		{
			Vector3 newPos = transform.position;
			newPos.x -= speed / moveSpeedModifier * Time.deltaTime;
			transform.position = newPos;
		}
	}

	private void OnDrawGizmos()
	{
		BoxCollider collider = GetComponent<BoxCollider>();

		Vector3 leftCubePos = new Vector3(transform.position.x + maxLeftPosition, transform.position.y, transform.position.z);
		Vector3 rightCubePos = new Vector3(transform.position.x + maxRightPosition, transform.position.y, transform.position.z);
		Vector3 cubeSize = new Vector3(collider.size.x * transform.localScale.x, collider.size.y * transform.localScale.y, collider.size.z * transform.localScale.z);

		Gizmos.color = Color.cyan;
		Gizmos.DrawCube(leftCubePos, cubeSize);
		Gizmos.DrawCube(rightCubePos, cubeSize);

		Gizmos.color = Color.white;
		Gizmos.DrawLine(transform.position, leftCubePos);
		Gizmos.DrawLine(transform.position, rightCubePos);
	}

    public void OnPause()
    {
        enabled = false;
        if (moveFX && moveFXisPlaying)
            moveFX.Pause();
    }

    public void OnResume()
    {
        enabled = true;
        if (moveFX && moveFXisPlaying)
            moveFX.Play();
    }
}
