using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouvablePlatform : MonoBehaviour, IPausable
{
    private int playerOnElevator = 0;

    private BoxCollider collid;
    private Vector3 extents;
    private int enemyMask;
    private int playerMask;

    private Vector3 previousPosition;
    private List<Entity> entities = new List<Entity>();

    // Use this for initialization
    void Start ()
    {
        GameManager.Instance.PauseManager.AddPausable(this);
        collid = GetComponent<BoxCollider>();
        extents = collid.bounds.extents;
        previousPosition = transform.position;

        enemyMask = 1 << LayerMask.NameToLayer("Enemy");
        playerMask = 1 << LayerMask.NameToLayer("Player");
    }

    void FixedUpdate()
    {
        Vector3 move = (transform.position - previousPosition);
        previousPosition = transform.position;

        int count = entities.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; ++i)
            {
                if (entities[i])
                    entities[i].transform.position += move;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Physics.BoxCast(collid.bounds.center, new Vector3(extents.x - 0.1f, extents.y / 3f, extents.z), Vector3.up, Quaternion.identity, extents.y * transform.localScale.y, playerMask | enemyMask))
        {
            if (collision.gameObject.tag == "Player")
                ++playerOnElevator;

            Entity entity = collision.gameObject.GetComponent<Entity>();
            if (entity)
                entities.Add(entity);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && playerOnElevator > 0)
        {
            --playerOnElevator;
        }

        Entity entity = collision.gameObject.GetComponent<Entity>();

        if (entity)
            entities.Remove(entity);
    }

    public void OnPause()
    {
        enabled = false;
    }

    public void OnResume()
    {
        enabled = true;
    }
}
