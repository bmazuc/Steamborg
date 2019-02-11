using UnityEngine;


public class EnemyHitBoxAttack : HitBoxAttack
{
	public System.Action<Player> OnPlayerTouched;


	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Player player = other.gameObject.GetComponent<Player>();
			if (!touchedEntities.Contains(player))
			{
				if (!canHitOnKnockDown && player.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockDown)
					return;

				if (!canTouchMultiple && touchedEntities.Count >= 1)
					return;

				touchedEntities.Add(player);

				if (OnPlayerTouched != null)
					OnPlayerTouched(player);
			}
		}
	}
}
