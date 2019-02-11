using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pausePanel;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

    public void ReturnToMenuButtonClick()
    {
        GameManager.Instance.ReturnToMainMenu();
    }

    public void AskPause()
    {
        GameManager gM = GameManager.Instance;

        if (gM.PauseManager.GameIsPaused)
        {
            gM.PauseManager.Resume();
            pausePanel.SetActive(false);
        }
        else
        {
            gM.PauseManager.Pause();
            pausePanel.SetActive(true);
        }

    }

    public void ResumeGameButtonClick()
    {
        AskPause();
    }
}
