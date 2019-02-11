using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LoadingScreen : MonoBehaviour
{
	static private LoadingScreen instance = null;
	static public LoadingScreen Instance
	{
		get
		{
			if (instance == null)
				instance = FindObjectOfType<LoadingScreen>();

			return instance;
		}
	}

	[SerializeField]
	private Slider progressBar = null;
	[SerializeField]
	private Image blackScreen = null;
	private float blackScreenSpeed = 1f;

	private string sceneName = "";

	private float progressValue;

	private bool asyncLoadFinshed = false;
	private bool transitionToNext = false;


	private void Start()
	{
		blackScreen.color = new Color(0f, 0f, 0f, 0f);

		TransitionScreen.Instance.Hide();
	}
	private void Update()
	{
		progressBar.value = Mathf.Lerp(progressBar.value, progressValue, Time.deltaTime * (2f * progressValue));

		if (asyncLoadFinshed)
		{
			Color newColor = blackScreen.color;
			newColor.a += Time.deltaTime * blackScreenSpeed;
			blackScreen.color = newColor;

			if (newColor.a >= 1f)
				transitionToNext = true;
		}
	}

	public void LoadScene(string sceneToLoad)
	{
		sceneName = sceneToLoad;

		StartCoroutine(Loading());
	}

	private IEnumerator Loading()
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		asyncOperation.allowSceneActivation = false;

		while (!asyncLoadFinshed)
		{
			progressValue = asyncOperation.progress;

			if (progressValue >= 0.9f)
			{
				progressValue = 1f;

				if (progressBar.value >= 0.99f)
				{
					asyncLoadFinshed = true;
				}
			}

			yield return null;
		}

		while (!transitionToNext)
			yield return null;

		asyncOperation.allowSceneActivation = true;

		while (!asyncOperation.isDone)
			yield return null;

		gameObject.SetActive(false);

		SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
		
		if (sceneName != "MainMenu")
			GameManager.Instance.Init();

		SceneManager.UnloadSceneAsync("LoadingScene");
		TransitionScreen.Instance.FadeIn();
	}
}
