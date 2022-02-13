using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;


[Serializable]
public class TrialParams : ULinkBase
{
    // trial time parameters (in ms) : IN FROM BLOCK 
    public ULinkInt_In currentStimInd;
    public ULinkInt_In currentStimTime;
    public ULinkInt_In ISI;
    public ULinkInt_In maskTime;
    public ULinkInt_In timeToRating; // if there's no rating, it's just 0 
    public ULinkBool_In isActiveBlockEnd;

    // for timer communication we need to get boolean values from timer object : OUT FROM TIMER TO TRIAL
    // meaning, when the appropriate timers stop running - we receive these vars 
    public ULinkBool_In currentStimEnd;
    public ULinkBool_In ISIEnd;
    public ULinkBool_In MaskEnd;
    public ULinkBool_In timeToRatingEnd;

    public ULinkBool_In gotResponse; // got keyboard response of SPACE from the subject
    public ULinkEvent_In subjectRating; // got keyboard response of NUM 1 - 4 from the subject

    // out params to STIMULUS
    public ULinkInt_Out currentStimIndOut;
    public ULinkBool_Out isActiveStimOut;
    public ULinkBool_Out isActiveMaskOut;
    public ULinkBool_Out isActiveRankOut;
    public ULinkBool_Out isActiveBlockEndOut;

    // out params to EXPERIMENT TEST
    public ULinkBool_Out isActiveTestOut;

    // for timer communication we need to send times to timer object : OUT FROM TRIAL TO TIMER
    public ULinkInt_Out currentStimTimeOut;
    public ULinkInt_Out ISIOut;
    public ULinkInt_Out maskTimeOut;
    public ULinkInt_Out timeToRatingOut;

    // out params to BLOCK
    public ULinkBool_Out trialEndOut; // signaling the block that the trial has ended
    public ULinkBool_Out subjectNextBlockOut; // sent when the subject saw the "block end" message, and pressed when ready


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void gotResponseHandler()
    {
        DebugLog("Got response from subject");
        // record to log
        RecordLog("<KB> Got response from subject: SpaceBar");
        if (isActiveBlockEndOut.val)
        {
            isActiveBlockEndOut.val = false;
            subjectNextBlockOut.val = true;
        }
    }

    public void subjectRatingHandler()
    {
        if (isActiveRankOut.val) // JUST WHEN THE SUBJECT RESPONSE ON KB IS RELEVANT - RECORD IT
        {
            // make the rating screen disappear
            isActiveRankOut.val = false; //deactivate rating 
            // record
            DebugLog("Subject rating: subjectRating.val.keyCode = " + subjectRating.val.keyCode);
            RecordLog("<Rating> Subject rating: = " + subjectRating.val.keyCode);
            // now, trial has finished : update the block:
            trialEndOut.val = true;
            
        }
        
    }

    public void currentStimIndHandler()
    {
        currentStimIndOut.val = currentStimInd.val;
        currentStimTimeOut.val = currentStimTime.val;
        isActiveTestOut.val = true;
        isActiveStimOut.val = true; //The order is important - the listener is on this variable, must be last
        RecordLog("<Stim> <Start> <"+ currentStimTimeOut.val.ToString() +"> " + "Target Stimulus Start, expected duration: " + currentStimTimeOut.val.ToString() + " ms");
    }

    public void currentStimEndHandler()
    {
        // current stimulus timer done = stop showing target stimulus (ISI starts)
        isActiveStimOut.val = false; //deactivate target stimulus
        //Start ISI timer
        if (ISI.val != 0)
        {
            isActiveTestOut.val = false;
            ISIOut.val = ISI.val; // ISI timer
            DebugLog("Target Stimulus " + currentStimInd.ToString() + "End: ISI start, expected ISI duration: " + ISIOut.val.ToString() + " ms");
            RecordLog("<ISI> <Start> <" + ISIOut.val.ToString() + "> " + "Target Stimulus " + currentStimInd.ToString() + "End: ISI start, expected ISI duration: " + ISIOut.val.ToString() + " ms");
        }
        else
        {
            DebugLog("Target Stimulus " + currentStimInd.ToString() + "End: waiting for next trial");
            RecordLog("<ISI> <Start> <" + ISIOut.val.ToString() + "> " + "Target Stimulus " + currentStimInd.ToString() + "End: waiting for next trial");
            MaskEndHandler();
        }
        
    }

    public void ISIEndHandler()
    {
        isActiveTestOut.val = true;
        //ISI timer done = need to turn on mask 
        isActiveMaskOut.val = true; //activate mask
        maskTimeOut.val = maskTime.val; // mask timer
        RecordLog("<Mask> <Start> <"+ maskTimeOut.val.ToString() + "> " + "ISI End: mask start, expected mask duration: " + maskTimeOut.val.ToString() + " ms");
        DebugLog("ISI End: mask start, expected mask duration: " + maskTimeOut.val.ToString() + " ms");
    }

    public void MaskEndHandler()
    {
        isActiveTestOut.val = false;
        // mask timer done = need to turn off mask and turn on timer for PAS waiting
        isActiveMaskOut.val = false; // deactivate mask
        timeToRatingOut.val = timeToRating.val; // time to rating timer
        RecordLog("<Mask> <End> mask End: time to PAS start");
        DebugLog("mask End: time to PAS start");
    }

    public void timeToRatingEndHandler()
    {
        if (timeToRating.val != 0) // if we have visibility ratings at the end of this trial
        {
            // time to rating done = show rating screen and wait for subject kb response
            isActiveRankOut.val = true; //activate rating 
            RecordLog("time to PAS End: Showing PAS waiting for subject response");
            DebugLog("time to PAS End: Showing PAS waiting for subject response");
        }
        else // we don't have visibility ratings, trial needs to end 
        {
            trialEndOut.val = true;
            RecordLog("time to next trial End: Start Next Trial");
            DebugLog("time to next trial End: Start Next Trial");
        }
    }

    public void isActiveBlockEndHandler()
    {
        isActiveBlockEndOut.val = true;
    }

}
