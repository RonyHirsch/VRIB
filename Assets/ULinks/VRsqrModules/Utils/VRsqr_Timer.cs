using UnityEngine;
using System.Collections;
using VRsqrCore;
using VRsqrUtil;
using UnityEngine.UI;
using System;

[Serializable]
public class VRsqr_Timer : ULinkBase 
{
    public ULinkInt_In TimerDuration;
    public ULinkBool_Out timerEndEventOut;

    public void Start()
    {      

    }

    public void TimerDurationHandler() // reserved name for var handler
    {
        VRsqrUtil.Debug.Log(LogLevel.Debug, "startTimerHandler");
        StartCoroutine("TimerFunc");
    }

    private IEnumerator TimerFunc()
    {
        VRsqrUtil.Debug.Log(LogLevel.Debug, "TimerFunc: TimerInputs.TimerDuration = " + TimerDuration.val);
        yield return new WaitForSeconds(TimerDuration.val / 1000.0f); 

        timerEndEventOut.val = true;
    }
}
