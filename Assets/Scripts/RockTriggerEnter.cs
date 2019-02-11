using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockTriggerEnter : MonoBehaviour {
    [SerializeField]
    private int damage = 50;
    [SerializeField]
    private float knockDownDuration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Entity entity = other.gameObject.GetComponent<Entity>();

        if (entity)
        {
            entity.TakeDamage(damage);
            entity.CCMgr.KnockDown(knockDownDuration);
        }
    }
}
