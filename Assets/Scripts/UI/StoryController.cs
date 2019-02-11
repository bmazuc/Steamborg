using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StoryController : MonoBehaviour {

    [SerializeField]
    private GameObject button;

    [SerializeField]
    private HUD hud;

	void Start () {
        if (GameManager.Instance.ShowStory)
            StartCoroutine(PauseGame());
        else
            Destroy(gameObject);
    }

    public void DestroyUI()
    {
        Destroy(gameObject);
        GameManager.Instance.PauseManager.Resume();
        GameManager.Instance.PauseManager.IsInStory = false;
        if (hud)
            hud.ShowPlayerUI();
    }

    private IEnumerator PauseGame()
    {
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.PauseManager.Pause(false);
        GameManager.Instance.PauseManager.IsInStory = true;

        if (button)
        {
            ControllerRefocus.Instance.LastSelectedGameObject = button;
            EventSystem.current.SetSelectedGameObject(button);
        }

        if (hud)
            hud.HidePlayerUI();
    }
}
