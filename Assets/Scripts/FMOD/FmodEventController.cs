using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodEventController : MonoBehaviour {
    [SerializeField]
    private string path = "";

    private FMOD.Studio.EventInstance fmodEvent;

    void Start()
    {
        FMOD.Studio.EventInstance fmodEvent = FMODUnity.RuntimeManager.CreateInstance(path);
    }

    public void PlayFModEvent()
    {
        //if (fmodEvent.IsValid())
            fmodEvent.start();
    }

    public void StopFModEvent()
    {
       // if (fmodEvent.IsValid())
            fmodEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
