using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(BoxCollider))]
public class EventZone : MonoBehaviour, IRespawnable
{
	private enum DIRECTION
	{
		None,
		Left,
		Right
	}

    [SerializeField]
    private UnityEvent onTriggerEnter;
    bool isActivated = false;

    [SerializeField]
    private bool canActiveMultipleTime = false;

	[SerializeField]
	private bool twoPlayerNeeded = false;
	private int numberOfPlayer = 0;

    private int playerNumber = 0;

	[SerializeField]
	private DIRECTION direction = DIRECTION.None;

	private GameManager gameMgr = null;

    void Start()
    {
        if (onTriggerEnter == null)
            onTriggerEnter = new UnityEvent();

		gameMgr = GameManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
		if ((!isActivated || canActiveMultipleTime) && other.tag == "Player")
		{
			Player player = other.gameObject.GetComponent<Player>();
			if (twoPlayerNeeded && gameMgr.HasTwoPlayer && !gameMgr.Player1.IsDead && !gameMgr.Player2.IsDead)
			{
				HandleTwoPlayerEnter(player);
			}
			else
			{
				HandleOnePlayerEnter(player);
			}
		}
    }
	private void OnTriggerExit(Collider other)
	{
		if (gameMgr.HasTwoPlayer && !gameMgr.Player1.IsDead && !gameMgr.Player2.IsDead && other.tag == "Player")
		{
			HandlePlayerOut(other.gameObject.GetComponent<Player>());
		}
	}

	public void LaunchTrigger()
	{
		if (onTriggerEnter != null)
			onTriggerEnter.Invoke();

		isActivated = true;
	}

	public void ResetTrigger()
    {
        isActivated = false;
    }

    public void Respawn()
    {
        isActivated = false;
    }

	private void HandleOnePlayerEnter(Player player)
	{
        if (player.IsDead)
            return;

        LaunchTrigger();
	}
	private void HandleTwoPlayerEnter(Player player)
	{
        if (player.IsDead)
            return;

		if (direction == DIRECTION.None)
		{
			++numberOfPlayer;

		    if (playerNumber == 0)
		    {
		        if (player is PlayerMelee)
		            playerNumber = 1;
		        else if (player is PlayerRanged)
		            playerNumber = 2;

            }

			if (numberOfPlayer >= 2 && ((playerNumber == 1 && player is PlayerRanged) || (playerNumber == 2 && player is PlayerMelee)))
				LaunchTrigger();
		}
		else
		{
			DIRECTION playerEnterDir = (player.MoveDirection.x > 0f) ? DIRECTION.Right : DIRECTION.Left;

			if (playerEnterDir == direction)
			{
				++numberOfPlayer;

			    if (playerNumber == 0)
			    {
			        if (player is PlayerMelee)
			            playerNumber = 1;
			        else if (player is PlayerRanged)
			            playerNumber = 2;
			    }
            }

			if (numberOfPlayer >= 2 && ((playerNumber == 1 && player is PlayerRanged) || (playerNumber == 2 && player is PlayerMelee)))
			{
				LaunchTrigger();
			}
		}
	}
	private void HandlePlayerOut(Player player)
	{
		if (direction != DIRECTION.None)
		{
			DIRECTION playerOutDir = (player.MoveDirection.x > 0f) ? DIRECTION.Right : DIRECTION.Left;

			if (playerOutDir != direction && numberOfPlayer > 0)
			{
				--numberOfPlayer;

			    if (numberOfPlayer == 0)
			        playerNumber = 0;
			}
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (UnityEditor.ArrayUtility.Contains(UnityEditor.Selection.objects, gameObject))
		{
			int count = onTriggerEnter.GetPersistentEventCount();
			if (count > 0)
			{
				for (int i = 0; i < count; ++i)
				{
					Object obj = onTriggerEnter.GetPersistentTarget(i);

					if (!obj)
						continue;

					GameObject go = GameObject.Find(obj.name);

					if (!go || !go.transform)
						continue;

					Gizmos.color = Color.green;
					Gizmos.DrawLine(transform.position, go.transform.position);
					Gizmos.DrawIcon(go.transform.position, "testIcon");
				}
			}
		}
	}
#endif
}
