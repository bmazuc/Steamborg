using UnityEngine;


public class MoveToPlayerSmall : StateMachineBehaviour
{
	[SerializeField, Range(0.1f, 1f)]
	private float zImportance = 1f;

	[SerializeField, Range(0.1f, 1f)]
	private float zForceMovingTo = 1f;

	[SerializeField, Range(0.1f, 1f)]
	private float zForceMovingAway = 1f;

	private EnemyAgile parent = null;


	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		if (parent == null)
			parent = animator.GetComponentInParent<EnemyAgile>();
	}
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!parent.IsCC() && !parent.IsAttacking)
		{
			if (animator.GetBool("playerFound"))
			{
				bool alignInX = parent.IsXAlignWithNearestPlayer();
				bool alignInY = parent.IsYAlignWithNearestPlayer();

				if (alignInX && alignInY)
				{
					switch (parent.IsInLongShotRange())
					{
						case -1:
							MoveTo(animator, false, true);
							break;
						case 0:
							animator.SetBool("inLongShotRange", true);
							parent.Move(Vector3.zero);
							break;
						case 1:
							MoveTo(animator, true, true);
							break;
						default:
							break;
					}
				}
				else
					MoveTo(animator, false, alignInX);
			}
		}
		else
			parent.Move(Vector3.zero);
	}

	private void MoveTo(Animator animator, bool moveCloserToPlayer, bool alignWithPlayer)
	{
		Player player = parent.nearestPlayer;
		Vector3 direction = (player.Collid.bounds.center - parent.transform.position).normalized;

		if (!moveCloserToPlayer)
		{
			float angle;
			if (direction.z < 0f)
				angle = Vector3.Angle(direction, Vector3.back) * 2f * Mathf.Sign(direction.x);
			else
				angle = Vector3.Angle(direction, Vector3.forward) * 2f * -Mathf.Sign(direction.x);

			direction = (Quaternion.Euler(0f, angle, 0f) * direction).normalized;

			Vector3 scaledSize = new Vector3(parent.Collid.size.x * parent.transform.localScale.x, parent.Collid.size.y * parent.transform.localScale.y, parent.Collid.size.z * parent.transform.localScale.z);
			if (Physics.BoxCast(parent.Collid.bounds.center, new Vector3(scaledSize.x / 2f * 0.9f, scaledSize.y / 2f, scaledSize.z / 2f), direction, Quaternion.identity, scaledSize.x / 2f, parent.GroundMask | parent.WallMask | parent.PushableObjectMask | parent.CameraColliderMask))
			{
				if (!alignWithPlayer)
				{
					direction = new Vector3(0f, 0f, (direction.z > 0f) ? 1f : -1f);
					animator.SetBool("playerFound", false);
				}
				else
				{
					direction = Vector3.zero;
					animator.SetBool("inLongShotRange", true);
				}
				
				parent.Move(direction);
				return;
			}
		}

		if (!alignWithPlayer && 1f - Mathf.Abs(direction.z) <= zImportance)
		{
			float angle;
			if (direction.z < 0f)
				angle = Vector3.Angle(direction, Vector3.back) * Mathf.Sign(direction.x);
			else
				angle = Vector3.Angle(direction, Vector3.forward) * -Mathf.Sign(direction.x);

			direction = (Quaternion.Euler(0f, angle * ((moveCloserToPlayer) ? zForceMovingTo : 1f - zForceMovingAway), 0f) * direction).normalized;
		}
		else
			direction.z = 0f;

		direction.y = 0f;

		parent.Move(direction);
		animator.SetBool("playerFound", false);
	}
}
