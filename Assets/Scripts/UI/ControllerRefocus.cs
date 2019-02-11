using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ControllerRefocus : MonoBehaviour {

    static private ControllerRefocus instance;
    static public ControllerRefocus Instance
    {
        get
        {
            if (!instance)
            {
                ControllerRefocus target = GameObject.FindObjectOfType<ControllerRefocus>();
                if (target)
                {
                    instance = target;
                }
            }
            return instance;
        }
    }

    private GameObject lastSelectedGameObject = null;
    public GameObject LastSelectedGameObject { set { lastSelectedGameObject = value; } }

    private void Awake()
    {
        lastSelectedGameObject = EventSystem.current.firstSelectedGameObject;
    }

    void Update()
    {
        if (EventSystem.current && !EventSystem.current.currentSelectedGameObject)
            EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
    }
}
