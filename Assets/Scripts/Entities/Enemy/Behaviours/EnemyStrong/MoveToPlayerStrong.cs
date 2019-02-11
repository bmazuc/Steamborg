using UnityEngine;


public class MoveToPlayerStrong : StateMachineBehaviour
{
	[SerializeField, Range(0.1f, 1f)]
	private float zImportance = 1f;

	[SerializeField, Range(0.1f, 1f)]
	private float zForce = 1f;

	protected EnemyStrong parent = null;


	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		if (parent == null)
			parent = animator.GetComponentInParent<EnemyStrong>();
	}
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!parent.IsCC())
		{
			if (animator.GetBool("playerFound"))
			{
				if (parent.IsInRange())
				{
					animator.SetBool("inAttackRange", true);
					parent.Move(Vector3.zero);
				}
				else
				{
					MoveTo();
					animator.SetBool("playerFound", false);
				}
			}
		}
		else
			parent.Move(Vector3.zero);
	}

	private void MoveTo()
	{
		Player player = parent.nearestPlayer;
		Vector3 direction = (player.Collid.bounds.center - parent.transform.position).normalized;

		if (TryAvoidObject(direction))
			return;

		float xDistance = Mathf.Abs(parent.transform.position.x - player.Collid.bounds.center.x);
		float minXPos = parent.Extents.x + player.Extents.x;

		if (xDistance < minXPos)
		{
			float newXPos = player.Collid.bounds.center.x + (minXPos * Mathf.Sign(xDistance));

			if (Mathf.Abs(player.Collid.bounds.center.z - parent.transform.position.z) > parent.ZAcceptance)
			{
				direction = (new Vector3(newXPos, player.Collid.bounds.center.y, player.Collid.bounds.center.z) - parent.transform.position).normalized;

				direction.y = 0f;
			}
			else
				direction = new Vector3(newXPos, 0f, 0f).normalized;

			parent.Move(direction);
			return;
		}

		if (1f - Mathf.Abs(direction.z) <= zImportance)
		{
			float angle;
			if (direction.z < 0f)
				angle = Vector3.Angle(direction, Vector3.back) * Mathf.Sign(direction.x);
			else
				angle = Vector3.Angle(direction, Vector3.forward) * -Mathf.Sign(direction.x);

			direction = (Quaternion.Euler(0f, angle * zForce, 0f) * direction).normalized;
		}

		direction.y = 0f;
		parent.Move(direction);
	}

	private bool TryAvoidObject(Vector3 direction)
	{
		Vector3 extents = new Vector3(parent.Collid.size.x * parent.transform.localScale.x / 2f * 0.9f, parent.Collid.size.y * parent.transform.localScale.y / 2f, parent.Collid.size.z * parent.transform.localScale.z / 2f * 0.9f);

		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
		{ // left or right
			if (direction.x > 0f)
			{ // right
				if (Physics.BoxCast(parent.Collid.bounds.center, extents, Vector3.right, Quaternion.identity, 3f, parent.PushableObjectMask))
				{
					parent.Move(Vector3.back);
					return true;
				}
			}
			else
			{ // left
				if (Physics.BoxCast(parent.Collid.bounds.center, extents, Vector3.left, Quaternion.identity, 3f, parent.PushableObjectMask))
				{
					parent.Move(Vector3.back);
					return true;
				}
			}
		}
		else
		{ // forward or back
			if (direction.z > 0f)
			{ // forward
				if (Physics.BoxCast(parent.Collid.bounds.center, extents, Vector3.forward, Quaternion.identity, 3f, parent.PushableObjectMask))
				{
					parent.Move((parent.transform.position.x < parent.nearestPlayer.transform.position.x) ? Vector3.right : Vector3.left);
					return true;
				}
			}
			else
			{ // back
				if (Physics.BoxCast(parent.Collid.bounds.center, extents, Vector3.back, Quaternion.identity, 3f, parent.PushableObjectMask))
				{
					parent.Move((parent.transform.position.x < parent.nearestPlayer.transform.position.x) ? Vector3.right : Vector3.left);
					return true;
				}
			}
		}

		return false;
	}
}
