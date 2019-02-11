using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;


public class TransitionScreen : MonoBehaviour
{
	static private TransitionScreen instance = null;
	static public TransitionScreen Instance
	{
		get
		{
			if (instance == null)
				instance = FindObjectOfType<TransitionScreen>();

			return instance;
		}
	}

	[SerializeField]
	private float transitionSpeed = 1f;
	private float originalXSize;

	[SerializeField]
	private RectTransform transition = null;

	private bool fadeOutTransition = false;
	private bool fadeInTransition = false;

	private string sceneName;

    private float waitTime = 0.1f;

	private void Start()
	{
		originalXSize = transition.sizeDelta.x;
	}

	private void Update()
	{
		if (fadeInTransition)
		{
			Vector2 newSize = transition.sizeDelta;
			newSize.x = newSize.y = newSize.x + (Time.deltaTime * transitionSpeed);
			transition.sizeDelta = newSize;

			if (newSize.x >= originalXSize)
			{
				fadeInTransition = false;
			}
		}

		if (fadeOutTransition)
		{
			Vector2 newSize = transition.sizeDelta;
			newSize.x = newSize.y = newSize.x - (Time.deltaTime * transitionSpeed);
			transition.sizeDelta = newSize;

			if (newSize.x <= 0f)
			{
				fadeOutTransition = false;

                if (sceneName != "")
                {
                    SceneManager.LoadScene("LoadingScene");
                    StartCoroutine(LoadDelay());
                }
                else
                    StartCoroutine(FadeInCoroutine());
			}
		}
	}

	public void FadeIn(float _waitTime = 0.1f)
	{
		transition.gameObject.SetActive(true);
		fadeInTransition = true;
        waitTime = _waitTime;
	}
	public void FadeOut(string sceneToLoad, float _waitTime = 0.1f)
	{
		transition.gameObject.SetActive(true);
		fadeOutTransition = true;
		sceneName = sceneToLoad;
        waitTime = _waitTime;
	}

	public void Hide()
	{
		transition.gameObject.SetActive(false);
	}

	private IEnumerator LoadDelay()
	{
		yield return new WaitForSeconds(waitTime);

		LoadingScreen.Instance.LoadScene(sceneName);
	}

    private IEnumerator FadeInCoroutine()
    {
        yield return new WaitForSeconds(waitTime - 0.1f);

        FadeIn();
    }
}
