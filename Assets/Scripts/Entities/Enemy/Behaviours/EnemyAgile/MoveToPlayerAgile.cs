using UnityEngine;


public class MoveToPlayerAgile : StateMachineBehaviour
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
							ManageShortRangeBehaviour(animator);
							break;
						case 0:
							animator.SetBool("inLongShotRange", true);
							parent.Move(Vector3.zero);
							break;
						case 1:
							MoveTo(true, true);
							animator.SetBool("playerFound", false);
							break;
						default:
							break;
					}
				}
				else
				{
					MoveTo(false, alignInX);
					animator.SetBool("playerFound", false);
				}
			}
		}
		else
			parent.Move(Vector3.zero);
	}

	private void MoveTo(bool moveCloserToPlayer, bool alignWithPlayer)
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
	}

	private void ManageShortRangeBehaviour(Animator animator)
	{
		switch (parent.IsInShortShotRange())
		{
			case -1:
				MoveTo(false, true);
				animator.SetBool("playerFound", false);
				break;
			case 0:
				animator.SetBool("inShortShotRange", true);
				parent.Move(Vector3.zero);
				break;
			case 1:
				MoveTo(true, true);
				animator.SetBool("playerFound", false);
				break;
			default:
				break;
		}
	}
}
