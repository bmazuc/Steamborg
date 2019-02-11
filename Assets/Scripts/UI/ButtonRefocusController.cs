using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonRefocusController : MonoBehaviour, ISelectHandler {

	public void OnSelect(BaseEventData eventData)
    {
        ControllerRefocus controllerRefocus = ControllerRefocus.Instance;

        if (controllerRefocus)
            ControllerRefocus.Instance.LastSelectedGameObject = gameObject;
    }
}
