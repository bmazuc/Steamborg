using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour {

    [SerializeField]
    private GameObject firstSelectedObjectOnPause = null;

    private bool isInStory = false;
    public bool IsInStory { set { isInStory = value; } }

    private bool gameIsPaused = false;
    public bool GameIsPaused
    {
        set { gameIsPaused = value; }
        get { return gameIsPaused; }
    }

    [SerializeField]
    private GameObject pausePanelPrefab = null;
    private GameObject pausePanelInstance = null;

    private int pausePlayerID = 0;

    private List<IPausable> pausables = new List<IPausable>();

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        RemoveAllPausable();
        DestroyPausePanel();
        gameIsPaused = false;
    }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void Update()
    {
        if (isInStory)
            return;

        if (Input.GetButtonDown("Pause_P1"))
            Switch(1);

        if (Input.GetButtonDown("Pause_P2") && GameManager.Instance.HasTwoPlayer)
            Switch(2);
}

    private void ChangeVerticalAxis(int playerID)
    {
        string verticalInput = "Vertical_P";
        pausePlayerID = playerID;

        StandaloneInputModule module = EventSystem.current.currentInputModule as StandaloneInputModule;
        if (module)
            module.verticalAxis = verticalInput.Substring(0, verticalInput.Length) + playerID.ToString();
    }

    public void Switch(int playerID)
    {
        if (GameIsPaused)
        {
            if (playerID == pausePlayerID)
                Resume();
        }
        else
        {
            ChangeVerticalAxis(playerID);
            Pause();
        }
    }

    public void AddPausable(IPausable pausable)
    {
        if (pausable == null)
            return;

        pausables.Add(pausable);
    }

    public void RemovePausable(IPausable pausable)
    {
        if (pausable == null)
            return;

        pausables.Remove(pausable);
    }

    public void RemoveAllPausable()
    {
        pausables.Clear();
    }

    public void DestroyPausePanel()
    {
        if (pausePanelInstance)
        {
            Destroy(pausePanelInstance);
            pausePanelInstance = null;
        }
    }

    public void Pause(bool withPausePanel = true)
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (gameIsPaused || activeSceneName == "MainMenu" || activeSceneName == "LoadingScene")
            return;

        if (pausePanelPrefab && withPausePanel)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas)
                pausePanelInstance = Instantiate(pausePanelPrefab, canvas.transform);
            if (pausePanelInstance)
                EventSystem.current.SetSelectedGameObject(pausePanelInstance.GetComponentInChildren<UnityEngine.UI.Button>().gameObject);
        }

        for (int idx = 0; idx < pausables.Count; ++idx)
            pausables[idx].OnPause();

        gameIsPaused = true;
    }

    public void Resume()
    {
        if (!gameIsPaused)
            return;

        StartCoroutine(ResumeCoroutine());
    }

    private IEnumerator ResumeCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        DestroyPausePanel();

        for (int idx = 0; idx < pausables.Count; ++idx)
            pausables[idx].OnResume();

        gameIsPaused = false;
    }
}
