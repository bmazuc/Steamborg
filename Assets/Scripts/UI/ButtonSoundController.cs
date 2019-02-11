using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.

public class ButtonSoundController : MonoBehaviour, IPointerEnterHandler, IMoveHandler
{
    [FMODUnity.EventRef]
    [SerializeField]
    private string clickedSoundEventName = "";
    [SerializeField]
    private FMOD.Studio.STOP_MODE clickedSoundStopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT;
    FMOD.Studio.EventInstance clickedSoundEventInstance;

    [FMODUnity.EventRef]
    [SerializeField]
    private string changedSoundEventName = "";
    [SerializeField]
    private FMOD.Studio.STOP_MODE changedSoundStopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT;
    FMOD.Studio.EventInstance changedSoundEventInstance;

    [SerializeField]
    private bool playSoundOnMove = true;
    [SerializeField]
    private bool playSoundOnHighlighted = true;

    void Start()
    {
        InitSounds();

        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button)
            button.onClick.AddListener(PlayClickedSound);
        UnityEngine.UI.Toggle toggle = GetComponent<UnityEngine.UI.Toggle>();
        if (toggle)
            toggle.onValueChanged.AddListener(PlayToggledSound);
    }

    private void InitSounds()
    {
        if (clickedSoundEventName != "")
            clickedSoundEventInstance = FMODUnity.RuntimeManager.CreateInstance(clickedSoundEventName);
        if (changedSoundEventName != "")
            changedSoundEventInstance = FMODUnity.RuntimeManager.CreateInstance(changedSoundEventName);
    }

    private void PlayToggledSound(bool toggle)
    {
        if (clickedSoundEventInstance.isValid())
        {
            clickedSoundEventInstance.stop(clickedSoundStopMode);
            clickedSoundEventInstance.start();
        }
    }

    private void PlayClickedSound()
    {
        if (clickedSoundEventInstance.isValid())
        {
            clickedSoundEventInstance.stop(clickedSoundStopMode);
            clickedSoundEventInstance.start();
        }
    }

    private void PlayChangedSound()
    {
        if (changedSoundEventInstance.isValid())
        {
            changedSoundEventInstance.stop(changedSoundStopMode);
            changedSoundEventInstance.start();
        }
    }

    private void OnDestroy()
    {
        if (clickedSoundEventInstance.isValid())
            clickedSoundEventInstance.stop(clickedSoundStopMode);
        if (changedSoundEventInstance.isValid())
            changedSoundEventInstance.stop(changedSoundStopMode);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playSoundOnHighlighted)
        {
            Debug.Log(gameObject.name + " was highlighted");
            PlayChangedSound();
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        if (playSoundOnMove)
        {
            Debug.Log(gameObject.name + " was moved");
            PlayChangedSound();
        }
    }
}
