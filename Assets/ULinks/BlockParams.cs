using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;


public class BlockParams : ULinkBase
{
    // INPUT PARAMS
    // Parameters from SESSION
    public ULinkIntList_In stimTimes; // all stimulus duration options
    public ULinkIntList_In ISITimes; // all ISI duration options
    public ULinkIntList_In maskTimes;
    public ULinkInt_In timeToRate; // a time constant we should wait before we ask the subject to rate visibility
    public ULinkInt_In ITItime; // the ITI
    public ULinkIntList_In stimOrder;


    //public ULinkInt_In interTrialLengthMSIn; // the time of the inter-trial-interval in ms - Currently still hardcoded in the editor
    //public ULinkBool_In itiEndEvent; // input signaling the end of the ITI duration

    // Parameters from TRIAL
    public ULinkBool_In trialEnd;
    public ULinkBool_In subjectNextBlock; // this input is received when the subject saw the "block end" message, and pressed when ready

    //Parameters from ITI TIMER 
    public ULinkBool_In ITIEnd;


    // OUTPUT PARAMS
    // Parameters sent to SESSION
    public ULinkBool_Out blockEndOut; // indication fo session whenever the block has ended

    // Parameters sent TO TRIAL
    public ULinkInt_Out trialStimOut; // output of the current trial stimulus
    public ULinkInt_Out stimTimeOut;
    public ULinkInt_Out ISITimeOut;
    public ULinkInt_Out TimeToRateOut;
    public ULinkInt_Out maskTimeOut;
    public ULinkBool_Out showBlockEndOut; // show the "block end" message, wait for subject response

    //Parameters sent TO ITI TIMER
    public ULinkInt_Out ITIOut;


    int currStimInd = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void stimOrderHandler()
    {
        // write to log the stiumuli order 
        RecordLog("Stim Order In Block:");
        string stimOrderStr = "";
        foreach (int o in stimOrder.val)
        {
            stimOrderStr = stimOrderStr + " " + o.ToString();
        }
        RecordLog(stimOrderStr);
        RecordLog("End of Stim Order List:");
        currStimInd = 0;


        // Starting the first trial after stim order is in
        trialStart();
    }

    void trialStart()
    {
        // send the next trial info to TRIAL: next target stimulus, next time intervals
        stimTimeOut.val = stimTimes.val[(currStimInd)%(stimTimes.val.Count)];
        ISITimeOut.val = ISITimes.val[(currStimInd) % (ISITimes.val.Count)];
        maskTimeOut.val = maskTimes.val[(currStimInd) % (maskTimes.val.Count)];
        TimeToRateOut.val = timeToRate.val;
        trialStimOut.val = stimOrder.val[currStimInd]; //The order is important - the listener is on this variable, must be last

        currStimInd++; 

    }

    public void trialEndHandler() 
    {
        ITIOut.val = ITItime.val;
    }

    public void subjectNextBlockHandler()
    {
        blockEndOut.val = true;
    }

    public void ITIEndHandler() 
    {
        // when TRIAL updated BLOCK that trial is finished - block needs to send the info to start the next trial
        if (currStimInd < stimOrder.val.Count) // if we have more stimuli in this block
        {
            trialStart();

        }
        // else: this was the last trial in block
        else
        {
            showBlockEndOut.val = true;

        }
    }
}
