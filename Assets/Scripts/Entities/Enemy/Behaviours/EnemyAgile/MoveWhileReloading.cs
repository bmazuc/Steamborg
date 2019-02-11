using UnityEngine;


public class MoveWhileReloading : StateMachineBehaviour
{
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

		Vector3 direction = new Vector3(parent.transform.position.x - parent.nearestPlayer.Collid.bounds.center.x, 0f, 0f).normalized;
		parent.Move(direction);
	}
}
