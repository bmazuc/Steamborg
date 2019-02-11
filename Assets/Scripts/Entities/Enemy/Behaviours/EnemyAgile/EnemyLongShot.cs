using UnityEngine;


public class EnemyLongShot : StateMachineBehaviour
{
	private EnemyAgile parent = null;


	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		if (parent == null)
			parent = animator.GetComponentInParent<EnemyAgile>();

		if (!parent.IsCC())
		{
			parent.ChangeFacingDirection(parent.nearestPlayer.Collid.bounds.center - parent.transform.position);
			parent.LongShot();
			animator.SetBool("lockOnReload", true);
		}
	}
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);

		animator.SetBool("inLongShotRange", false);
		animator.SetBool("playerFound", false);
	}
}
