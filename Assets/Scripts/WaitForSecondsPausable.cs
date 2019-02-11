using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForSecondsPausable : CustomYieldInstruction
{
    private float seconds;

    public override bool keepWaiting
    {
        get
        {
            while (GameManager.Instance.PauseManager.GameIsPaused)
                return true;

            seconds -= Time.deltaTime;
            return seconds > 0;
        }
    }

    public WaitForSecondsPausable(float _seconds)
    {
        seconds = _seconds;
    }
}
