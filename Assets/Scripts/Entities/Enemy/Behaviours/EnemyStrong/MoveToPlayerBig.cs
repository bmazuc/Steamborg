using UnityEngine;


public class MoveToPlayerBig : MoveToPlayerStrong
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!parent.IsCC() && animator.GetBool("playerFound") && !parent.CanChainAttack && !((EnemyStrongBig)parent).StunAttacking)
		{
			EnemyStrongBig parentAsBig = (EnemyStrongBig)parent;
			if (parentAsBig.StunReady && parentAsBig.IsInStunAttackRange())
				animator.SetBool("inStunAttackRange", true);
			else
				base.OnStateUpdate(animator, stateInfo, layerIndex);
		}
		else if (parent.IsCC())
			parent.Move(Vector3.zero);
	}
}
