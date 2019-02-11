﻿using UnityEngine;


public class FindNearestPlayerAgile : StateMachineBehaviour
{
	private Enemy parent = null;


	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		if (parent == null)
			parent = animator.GetComponentInParent<Enemy>();

		parent.nearestPlayer = FindPlayer();

		if (parent.nearestPlayer != null)
			animator.SetBool("playerFound", true);
		else
			animator.SetBool("playerFound", false);
	}

	private Player FindPlayer()
	{
		GameManager gameMgr = GameManager.Instance;
		if (!gameMgr.HasTwoPlayer)
		{
			if (!gameMgr.Player1.IsDead)
				return gameMgr.Player1;
			else
				return null;
		}

		float nearest = Mathf.Abs(parent.transform.position.z - gameMgr.Player1.transform.position.z);

		if (Mathf.Abs(parent.transform.position.z - gameMgr.Player2.transform.position.z) < nearest || gameMgr.Player1.IsDead)
		{
			if (!gameMgr.Player2.IsDead)
				return gameMgr.Player2;
		}

		if (!gameMgr.Player1.IsDead)
			return gameMgr.Player1;

		return null;
	}
}