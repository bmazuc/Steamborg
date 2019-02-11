using UnityEngine;


public class MeleeAttackStrong : StateMachineBehaviour
{
	private EnemyStrong parent = null;

	private bool inRange = false;
	private bool comboTrigger = false;


	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		if (parent == null)
			parent = animator.GetComponentInParent<EnemyStrong>();

		comboTrigger = false;

		if (!parent.IsCC() && !parent.IsAttacking)
		{
			parent.ChangeFacingDirection(parent.nearestPlayer.Collid.bounds.center - parent.transform.position);
			parent.Attack();
		}
	}
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		inRange = (parent.IsInRange() || parent.IsAttacking) && !parent.nearestPlayer.IsDead;

		if (parent.EnemyType != Enemy.ENEMY_TYPE.Small 
			&& !comboTrigger 
			&& ((EnemyStrong)parent).CanChainAttack 
			&& inRange
			&& !parent.IsCC())
		{
			parent.ChangeFacingDirection(parent.nearestPlayer.Collid.bounds.center - parent.transform.position);
			parent.Attack();

			comboTrigger = true;
		}
	}
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);

		animator.SetBool("inAttackRange", inRange);
	}
}
