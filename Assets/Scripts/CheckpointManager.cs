using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour {
    List<Checkpoint> checkpoints = new List<Checkpoint>();

    public Checkpoint CurrentCheckpoint { get { return (curIndex < checkpoints.Count) ? checkpoints[curIndex] : null; } }

    int curIndex = 0;

    bool hasStartRespawn = false;

    [SerializeField]
    private float respawnTime = 2f;
    public float RespawnTime { get { return respawnTime; } }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        checkpoints.AddRange(GameObject.FindObjectsOfType<Checkpoint>());

        foreach (Checkpoint checkpoint in checkpoints)
            checkpoint.OnTriggered += OnCheckPointTriggered;
    }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    public void InvokeCurrentCheckpoint()
    {
        if (curIndex >= checkpoints.Count)
            return;

        Checkpoint curCheckpoint = checkpoints[curIndex];

        if (curCheckpoint)
            curCheckpoint.InvokeRespawnable();
    }

    private void OnCheckPointTriggered(Checkpoint newCheckPoint)
    {
        curIndex = checkpoints.IndexOf(newCheckPoint);
    }

    public void StartRespawnCoroutine()
    {
        if (hasStartRespawn)
            return;

        GameManager gameManager = GameManager.Instance;

        if (gameManager.HasTwoPlayer)
        {
            if (!gameManager.Player1.IsDead || !gameManager.Player2.IsDead)
                return;
        }
        else if (!gameManager.Player1.IsDead)
            return;

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        hasStartRespawn = true;
        TransitionScreen.Instance.FadeOut("", respawnTime);
        yield return new WaitForSeconds(respawnTime);

        GameManager gameManager = GameManager.Instance;
        gameManager.Player1.Respawn();
        gameManager.Player2.Respawn();

        InvokeCurrentCheckpoint();

        hasStartRespawn = false;
    }
}
