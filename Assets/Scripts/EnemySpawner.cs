using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour, IRespawnable, IPausable
{
	private enum ENEMY_SPAWN_TYPE
	{
		SmallMelee,
		MediumMelee,
		BigMelee,
		SmallRanged,
		MediumRanged,
		BigRanged
	}

	[SerializeField]
	private UnityEvent OnSpawnEnd;

    [SerializeField]
    private UnityEvent OnAllEnemyKilled;

    [SerializeField]
	private float spawnDelay = 1f;
	[SerializeField]
	private float spawnRate = 1f;

	[SerializeField]
	private Vector3 goToPoint = Vector3.zero;

	[SerializeField]
	private List<ENEMY_SPAWN_TYPE> spawnOrder;

	[SerializeField]
	private Enemy smallMeleePrefab = null;
	[SerializeField]
	private Enemy mediumMeleePrefab = null;
	[SerializeField]
	private Enemy bigMeleePrefab = null;
	[SerializeField]
	private Enemy smallRangedPrefab = null;
	[SerializeField]
	private Enemy mediumRangedPrefab = null;
	[SerializeField]
	private Enemy bigRangedPrefab = null;

	private int spawnIndex = 0;

	private bool activated = false;
    private bool hasFinishedSpawn = false;

    private List<Enemy> spawnedEnemies = new List<Enemy>();

    void Update()
    {
        if (hasFinishedSpawn)
        {
            if (spawnedEnemies.Count == 0 && OnAllEnemyKilled != null)
            {
                OnAllEnemyKilled.Invoke();
                hasFinishedSpawn = false;
            }
        }
    }

	public void Activate()
	{
		if (!activated)
		{
            GameManager.Instance.PauseManager.AddPausable(this);
            activated = true;
            StartCoroutine(SpawnStart());
		}
	}

	private void Spawn()
	{
		Enemy newEnemy = null;

		switch (spawnOrder[spawnIndex])
		{
			case ENEMY_SPAWN_TYPE.SmallMelee:
				newEnemy = Instantiate(smallMeleePrefab, transform.position, Quaternion.identity);
				break;
			case ENEMY_SPAWN_TYPE.MediumMelee:
				newEnemy = Instantiate(mediumMeleePrefab, transform.position, Quaternion.identity);
				break;
			case ENEMY_SPAWN_TYPE.BigMelee:
				newEnemy = Instantiate(bigMeleePrefab, transform.position, Quaternion.identity);
				break;
			case ENEMY_SPAWN_TYPE.SmallRanged:
				newEnemy = Instantiate(smallRangedPrefab, transform.position, Quaternion.identity);
				break;
			case ENEMY_SPAWN_TYPE.MediumRanged:
				newEnemy = Instantiate(mediumRangedPrefab, transform.position, Quaternion.identity);
				break;
			case ENEMY_SPAWN_TYPE.BigRanged:
				newEnemy = Instantiate(bigRangedPrefab, transform.position, Quaternion.identity);
				break;
			default:
				break;
		}

		newEnemy.Rigid.useGravity = false;
		newEnemy.Collid.enabled = false;
		newEnemy.DetectionZone.SetActive(false);
		newEnemy.MoveToPoint(goToPoint + transform.position);

        newEnemy.OnDie += () => { spawnedEnemies.Remove(newEnemy); };
        spawnedEnemies.Add(newEnemy);

		++spawnIndex;

		if (spawnIndex < spawnOrder.Count)
			StartCoroutine(SpawnerTimer());
		else
			newEnemy.OnMoveToPointFinished += SpawnEnd;
	}

	private void SpawnEnd()
	{
        hasFinishedSpawn = true;
		if (OnSpawnEnd != null)
			OnSpawnEnd.Invoke();
	}

	private IEnumerator SpawnerTimer()
	{
		yield return new WaitForSecondsPausable(spawnRate);

		Spawn();
	}

    public void Respawn()
    {
        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy)
                DestroyImmediate(enemy.gameObject);
        }

        spawnIndex = 0;
        activated = false;
    }

    public void OnPause()
    {
        enabled = false;
    }

    public void OnResume()
    {
        enabled = true;
    }

	#region Debug
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position, new Vector3(5f, 5f, 5f));

		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(goToPoint + transform.position, 2f);

		Gizmos.color = Color.white;
		Gizmos.DrawLine(transform.position, goToPoint + transform.position);
	}
	#endregion

    private IEnumerator SpawnStart()
    {
        yield return new WaitForSecondsPausable(spawnDelay);
        Spawn();
    }
}
