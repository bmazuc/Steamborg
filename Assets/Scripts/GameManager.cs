using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour {

    static private  GameManager instance;
    static public   GameManager Instance
    {
        get
        {
            if (!instance)
            {
                GameManager target = GameObject.FindObjectOfType<GameManager>();
                if (target)
                {
                    instance = target;
                }
            }
            return instance;
        }
    }

    [SerializeField]
    private GameObject      playerMeleePrefab;
    [SerializeField]
    private GameObject      playerRangePrefab;

    [SerializeField]
    private Transform playerMeleeSpawnTransform;
    [SerializeField]
    private Transform playerRangeSpawnTransform;

    private PlayerMelee       playerMelee;
    public PlayerMelee        PlayerMelee { get { return playerMelee; } }
    private PlayerRanged    playerRange;
    public PlayerRanged     PlayerRange { get { return playerRange; } }

    private CameraManager cameraManager;
    public CameraManager CameraManager { get { return cameraManager; } }
    private Player[]        players;
    public Player Player1 { get { return players[0]; } }
    public Player Player2 { get { return players[1]; } }

    private bool hasTwoPlayer = false;
    public bool HasTwoPlayer { get { return hasTwoPlayer; } }

    private int player1CharChoice = 1;
    public int Player1CharChoice { set { Instance.player1CharChoice = value; } get { return player1CharChoice; } }

    public Action OnPlayer2Connect;
    public Action OnPlayer2Disconnect;

    private bool gameOver = false;

	public bool isInit
	{
		get;
		private set;
	}

    private PauseManager pauseManager;
    public PauseManager PauseManager { get { return Instance.pauseManager; } }

    private CheckpointManager checkpointManager;
    public CheckpointManager CheckpointManager { get { return Instance.checkpointManager; } }

    [SerializeField]
    private float winWaitTime = 2f;
    [SerializeField]
    private float loseWaitTime = 2f;

    private bool useLoseWaitTimeNextGameOver = true;
    public bool UseLoseWaitTimeNextGameOver { set { useLoseWaitTimeNextGameOver = value; } }

    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    private bool showStory = true;
    public bool ShowStory { get { return showStory; } }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

	private void Start()
	{
		if (SceneManager.GetActiveScene().name == "GpScene")
		{
			players = new Player[2];
			players[0] = FindObjectOfType<PlayerMelee>();
			players[1] = FindObjectOfType<PlayerRanged>();

            cameraManager = GetComponent<CameraManager>();
            pauseManager = GetComponent<PauseManager>();
            checkpointManager = GetComponent<CheckpointManager>();

		    if (players[0] != null)
                cameraManager.AddTransformToCameraGroup(players[0].transform, 0);

		    if (players[1] != null)
		        cameraManager.AddTransformToCameraGroup(players[1].transform, 1);
            
            playerMelee = (PlayerMelee)players[0];
			playerRange = (PlayerRanged)players[1];

			hasTwoPlayer = true;
		}
	}

	private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
		string activeSceneName = SceneManager.GetActiveScene().name;

		cameraManager = GetComponent<CameraManager>();
		isInit = false;

        pauseManager = GetComponent<PauseManager>();
        checkpointManager = GetComponent<CheckpointManager>();

		if (activeSceneName != "GpScene" && activeSceneName != "MainMenu" && activeSceneName != "LoadingScene")
		{
			Init();
            GameObject go = GameObject.Find("Player Melee Spawner Transform");

			if (go)
				playerMeleeSpawnTransform = go.transform;

			go = GameObject.Find("Player Range Spawner Transform");

			if (go)
				playerRangeSpawnTransform = go.transform;
            
			players = new Player[2];

			int joystickConnected = Input.GetJoystickNames().Length;

			if (joystickConnected == 0)
				Debug.LogError("No controller detected");

			hasTwoPlayer = !(joystickConnected == 1) && joystickConnected > 0;

			if (!hasTwoPlayer)
				InstantiatePlayerSolo();
			else
				InstantiatePlayerMulti();
                
            players[0].OnDie += CheckIfGameOver;
			players[1].OnDie += CheckIfGameOver;
		}
    }

    void Update()
    {
        if (Input.GetButtonDown("Join_P2") && !hasTwoPlayer)
            ConnectPlayerTwo();
        /*if (Input.GetKeyDown(KeyCode.P))
            DisconnectPlayerTwo();*/
    }

	public void Init()
	{
		gameOver = false;

		cameraManager = GetComponent<CameraManager>();
        pauseManager = GetComponent<PauseManager>();
        checkpointManager = GetComponent<CheckpointManager>();

        GameObject go = GameObject.Find("Player Melee Spawner Transform");

		if (go)
			playerMeleeSpawnTransform = go.transform;

		go = GameObject.Find("Player Range Spawner Transform");

		if (go)
			playerRangeSpawnTransform = go.transform;

		players = new Player[2];

		int joystickConnected = Input.GetJoystickNames().Length;

		if (joystickConnected == 0)
			Debug.LogError("No controller detected");

		hasTwoPlayer = !(joystickConnected == 1) && joystickConnected > 0;

		if (!hasTwoPlayer)
			InstantiatePlayerSolo();
		else
			InstantiatePlayerMulti();

		players[0].OnDie += CheckIfGameOver;
		players[1].OnDie += CheckIfGameOver;

		isInit = true;
	}

    private void InstantiatePlayerSolo()
    {
        GameObject meleeInstance = Instantiate(playerMeleePrefab, playerMeleeSpawnTransform.position, Quaternion.identity);
        GameObject rangeInstance = Instantiate(playerRangePrefab, playerRangeSpawnTransform.position, Quaternion.identity);

        rangeInstance.GetComponent<Controller>().ChangeControllerPlayer(1);
        meleeInstance.GetComponent<Controller>().ChangeControllerPlayer(1);

        if (player1CharChoice == 1)
        {
            players[0] = meleeInstance.GetComponent<Player>();
            players[1] = rangeInstance.GetComponent<Player>();
            
            rangeInstance.SetActive(false);
            if (cameraManager)
                cameraManager.AddTransformToCameraGroup(meleeInstance.transform, 0);
        }
        else
        {
            players[0] = rangeInstance.GetComponent<Player>();
            players[1] = meleeInstance.GetComponent<Player>();
            meleeInstance.SetActive(false);
            if (cameraManager)
                cameraManager.AddTransformToCameraGroup(rangeInstance.transform, 0);
        }
    }

    private void InstantiatePlayerMulti()
    {
        GameObject meleeInstance = Instantiate(playerMeleePrefab, playerMeleeSpawnTransform.position, Quaternion.identity);
        GameObject rangeInstance = Instantiate(playerRangePrefab, playerRangeSpawnTransform.position, Quaternion.identity);

        if (player1CharChoice == 1)
        {
            players[0] = meleeInstance.GetComponent<Player>();
            players[1] = rangeInstance.GetComponent<Player>();
            meleeInstance.GetComponent<Controller>().ChangeControllerPlayer(1);
            rangeInstance.GetComponent<Controller>().ChangeControllerPlayer(2);
        }
        else
        {
            players[0] = rangeInstance.GetComponent<Player>();
            players[1] = meleeInstance.GetComponent<Player>();
            meleeInstance.GetComponent<Controller>().ChangeControllerPlayer(2);
            rangeInstance.GetComponent<Controller>().ChangeControllerPlayer(1);
        }

        if (cameraManager)
        {
            cameraManager.AddTransformToCameraGroup(meleeInstance.transform, 0);
            cameraManager.AddTransformToCameraGroup(rangeInstance.transform, 1);
        }
    }

    public void ConnectPlayerTwo()
    {
        if (hasTwoPlayer)
            return;

        Player player1 = players[0];
        Player player2 = players[1];

        Controller controller = player2.gameObject.GetComponent<Controller>();

        if (!controller)
            return;

        player2.gameObject.SetActive(true);
        controller.ChangeControllerPlayer(2);
        player2.TeleportTo(player1.transform);

        hasTwoPlayer = true;
        if (OnPlayer2Connect != null)
            OnPlayer2Connect();
    }

    public void DisconnectPlayerTwo()
    {
        if (!hasTwoPlayer)
            return;

        Controller controller = players[1].gameObject.GetComponent<Controller>();

        if (!controller)
            return;

        players[1].gameObject.SetActive(false);
        controller.ChangeControllerPlayer(1);

        hasTwoPlayer = false;

        if (OnPlayer2Disconnect != null)
            OnPlayer2Disconnect();
    }

    public void CheckIfWin(string scene)
    {
        if (gameOver)
            return;

        StartCoroutine(WinTimer(scene));

		gameOver = true;
    }

    private void CheckIfGameOver()
    {
        if (gameOver)
            return;

        if (hasTwoPlayer)
        {
            if ((players[0].CurrLife > 0 || players[1].CurrLife > 0))
                return;
        }
        else
        {
            if (players[0].CurrLife > 0)
                return;
        }

        gameOver = true;

        if (useLoseWaitTimeNextGameOver)
            StartCoroutine(LoseTimer());
        else
            TransitionScreen.Instance.FadeOut(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        TransitionScreen.Instance.FadeOut(mainMenuSceneName);
    }

    public void LoadLevel(string sceneName)
    {
		TransitionScreen.Instance.FadeOut(sceneName);
        instance.showStory = true;
    }

    public void RestartLevel()
    {
        TransitionScreen.Instance.FadeOut(SceneManager.GetActiveScene().name);
        instance.showStory = false;
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LockPlayers()
    {
        if (Player1)
        {
            Controller controller = Player1.GetComponent<Controller>();

            if (controller)
                controller.Lock(true);
        }

        if (Player2)
        {
            Controller controller = Player2.GetComponent<Controller>();

            if (controller)
                controller.Lock(true);
        }
    }

    public void UnlockPlayers()
    {
        if (Player1)
        {
            Controller controller = Player1.GetComponent<Controller>();

            if (controller)
                controller.Lock(false);
        }

        if (Player2)
        {
            Controller controller = Player2.GetComponent<Controller>();

            if (controller)
                controller.Lock(false);
        }
    }

    public void MenuSelectMeleePlayer()
    {
        Player1CharChoice = 1;
    }

    public void MenuSelectRangePlayer()
    {
        Player1CharChoice = 2;
    }

    IEnumerator WinTimer(string scene)
    {
        yield return new WaitForSecondsPausable(winWaitTime);
        TransitionScreen.Instance.FadeOut(scene);
        showStory = true;
    }

    IEnumerator LoseTimer()
    {
        yield return new WaitForSecondsPausable(loseWaitTime);
        TransitionScreen.Instance.FadeOut(SceneManager.GetActiveScene().name);
        showStory = false;
    }

    public void ResumeGame()
    {
        instance.pauseManager.Resume();
    }
}
