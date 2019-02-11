using UnityEngine;


public class StunAttack : StateMachineBehaviour
{
	private EnemyStrongBig parent = null;


	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		if (parent == null)
			parent = animator.GetComponentInParent<EnemyStrongBig>();

		if (!parent.IsCC() && !parent.IsAttacking)
		{
			parent.ChangeFacingDirection(parent.nearestPlayer.Collid.bounds.center - parent.transform.position);
			parent.StunAttack();
		}
	}
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);

		animator.SetBool("inStunAttackRange", false);
	}
}
