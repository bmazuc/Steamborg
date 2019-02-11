using UnityEngine;


public class PlayerHitBoxAttack : HitBoxAttack
{
	public System.Action<Enemy> OnEnemyTouched;


	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Enemy")
		{
			Enemy enemy = other.gameObject.GetComponent<Enemy>();
			if (!touchedEntities.Contains(enemy))
			{
				if (!canHitOnKnockDown && enemy.CCMgr.CurrCC == CrowdControl.CONTROL_TYPE.KnockDown)
					return;

				if (!canTouchMultiple && touchedEntities.Count >= 1)
					return;

				touchedEntities.Add(enemy);

				if (OnEnemyTouched != null)
					OnEnemyTouched(enemy);
			}
		}
	}
}
