using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour, IPausable
{
    [SerializeField]
    Color gizmosColor = Color.red;
    [SerializeField]
    Vector3 gizmosSize = new Vector3(0.4f, 0.4f, 0.4f);

    private List<Entity> entities = new List<Entity>();

    [SerializeField]
    private Vector3[] destinations;

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    float arriveOffset = 0.35f;

    private int destIdx = 0;
    public int DestIdx { set { destIdx = value; } }

    private bool canMove = false;
    [SerializeField]
    private bool blockPlayerInput = false;

    private BoxCollider collid;
    private Vector3 extents;
    private int enemyMask;
    private int playerMask;

    private void OnDrawGizmosSelected()
    {
        if (destinations.Length < 0)
            return;

        Vector3 previousPos = transform.position;

        foreach (Vector3 dest in destinations)
        {
            Gizmos.color = gizmosColor;
            Gizmos.DrawLine(previousPos, dest);
            Gizmos.DrawCube(dest, gizmosSize);
            previousPos = dest;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.AddPausable(this);

        collid = GetComponent<BoxCollider>();
        extents = collid.bounds.extents;

        enemyMask = 1 << LayerMask.NameToLayer("Enemy");
        playerMask = 1 << LayerMask.NameToLayer("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (destIdx < destinations.Length && canMove)
        {
            if (!CheckIfArrive())
            {
                Vector3 move = (destinations[destIdx] - transform.position).normalized * speed * Time.deltaTime;

                transform.position += move;

                int count = entities.Count;
                if (count > 0)
                    for (int i = 0; i < count; ++i)
                    {
                        if (entities[i])
                            entities[i].transform.position += move;
                    }
            }
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance.PauseManager)
            GameManager.Instance.PauseManager.RemovePausable(this);
    }

    public void StartMoving(float delay)
    {
        StartCoroutine(Move(delay));
    }

    private void OnCollisionEnter(Collision collision)
    {
            Entity entity = collision.gameObject.GetComponent<Entity>();
            if (entity)
                entities.Add(entity);
    }

    private void OnCollisionExit(Collision collision)
    {
        Entity entity = collision.gameObject.GetComponent<Entity>();

        if (entity)
            entities.Remove(entity);
    }

    private bool CheckIfArrive()
    {
        float distance = Vector3.Distance(transform.position, destinations[destIdx]);

        if (distance <= arriveOffset)
        {
            if (blockPlayerInput)
                GameManager.Instance.UnlockPlayers();
            canMove = false;
            return true;
        }

        return false;
    }

    IEnumerator Move(float delay)
    {
        yield return new WaitForSecondsPausable(delay);
        canMove = true;
        if (blockPlayerInput)
            GameManager.Instance.LockPlayers();
    }

    public void OnResume()
    {
        enabled = true;
    }

    public void OnPause()
    {
        enabled = false;
    }
}
