using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReturnInputMenuBehaviour : MonoBehaviour {

    enum MenuState
    {
        MainScreen,
        CharSelect,
        Controls,
        Credits,
        TitleScreen
    }

    [SerializeField]
    private Animator MenuAnimator;

    [SerializeField]
    private Button TitleScreenAButton;
    [SerializeField]
    private Button MainPanelPlayButton;
    [SerializeField]
    private Button MainPanelControlsButton;
    [SerializeField]
    private Button MainPanelCreditsButton;

    private EventSystem eventSystem;

    private MenuState currentState;

	// Use this for initialization
	void Start ()
	{
	    eventSystem = GetComponent<EventSystem>();
        currentState = MenuState.TitleScreen;
	}

    public void ChangeCurrentState(int state)
    {
        currentState = (MenuState)state;
    }

    // Update is called once per frame
	void Update () {
	    if (Input.GetButtonDown("Cancel_P1"))
	    {
	        switch (currentState)
	        {
	            case MenuState.TitleScreen:
                    break;
                case MenuState.MainScreen:
                    MenuAnimator.SetTrigger("ToTittleScreen");
                    currentState = MenuState.TitleScreen;
                    eventSystem.SetSelectedGameObject(TitleScreenAButton.gameObject);
                    break;
	            case MenuState.CharSelect:
                    eventSystem.SetSelectedGameObject(MainPanelPlayButton.gameObject);
                    MenuAnimator.SetTrigger("ToMainMenu");
	                currentState = MenuState.MainScreen;
                    break;
	            case MenuState.Controls:
	                eventSystem.SetSelectedGameObject(MainPanelControlsButton.gameObject);
	                MenuAnimator.SetTrigger("ToMainMenu");
	                currentState = MenuState.MainScreen;
                    break;
	            case MenuState.Credits:
	                eventSystem.SetSelectedGameObject(MainPanelCreditsButton.gameObject);
	                MenuAnimator.SetTrigger("ToMainMenu");
	                currentState = MenuState.MainScreen;
                    break;
	            default:
	                throw new ArgumentOutOfRangeException();
	        }
	    }
	}
}
