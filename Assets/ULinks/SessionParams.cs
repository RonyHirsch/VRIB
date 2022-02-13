using Assets.Scripts.ULinkUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;

public class SessionParams : ULinkBase
{
    // IN PARAMS - SUBJECT INFO # should go to GLOBAL SETTINGS but when called, prints to file 4 times in a row
    //public int SubjectAge;
    //public string SubjectDomHand;
    //public string SubjectGender;

    // IN params FROM USER (manually)
    public ULinkInt_In isNBack; // IF THIS IS AN N-BACK EXP : 1 ELSE (psychophysics): 0 - this is sent to choose the randomization function
    public ULinkInt_In numOfStim; // literally how many stimuli are there
    public ULinkInt_In numOfBlocks; // how many blocks are in the experiment
    public ULinkInt_In numOfStimPerBlock; // how many stimuli (pictures) are in each block
    public ULinkIntList_In numOfNBackRepsPerBlock; // how many n-back repetitions will be in each block
    public ULinkIntList_In allStimTimes; // all stimulus duration options
    public ULinkIntList_In allISITimes; // all ISI duration options
    public ULinkIntList_In allMaskTimes; // al mask duration options
    public ULinkInt_In timeToRate; // a time constant we should wait before we ask the subject to rate visibility - if there's no rating in exp - PUT 0
    public ULinkInt_In ITI; // time interval to wait between trials. If there's rating and you want immediate transition post rating, PUT 0


    // In params FROM RANDOMIZER
    public ULinkIntList_In stimOrder;
    // note that stimOrder is actually a list of [stim, ISI, stim, ISI..] so in the handler it is split back to 2 lists
    // this implementation is due to Randoimzer and ULink implementations

    // In params FROM BLOCK
    public ULinkBool_In blockEnd; // indication for end of block

    // OUT Parameters sent to BLOCK
    public ULinkIntList_Out stimTimesOut; // all stimulus duration options
    public ULinkIntList_Out ISITimesOut; // all ISI duration options
    public ULinkIntList_Out maskTimesOut;
    public ULinkInt_Out timeToRateOut; // a time constant we should wait before we ask the subject to rate visibility
    public ULinkIntList_Out stimOrderOut;
    public ULinkInt_Out ITIOut;

    // OUT params to RANDOMIZER
    public ULinkInt_Out isNBackOut; // IF THIS IS AN N-BACK EXP : 1 ELSE (psychophysics): 0 
    public ULinkIntList_Out numOfStimOut; // how many stim are there
    public ULinkIntList_Out allISITimesOut; // all ISI duration options
    public ULinkInt_Out numOfBlocksOut; // how many blocks in exp
    public ULinkInt_Out numOfStimPerBlockOut; // how many unique stimuli (pictures) in each block
    public ULinkIntList_Out numOfNBrepsPerBlockOut; // how many stimuli will be repeated in a 1-back manner 

    // OUT params send to SPECIAL STIMULI DIRECTLY : THIS IS THE CASE OF EXPERIMENT INSTRUCTIONS AT START / END OF SESSION
    public ULinkBool_Out runInstructionsStartOut; // for experiment beginning
    public ULinkBool_Out runExpEndOut; // for the end of the experiment

    // IN FROM KEYBOARD for beginning the experiment
    public ULinkBool_In selfPace; // this is for when subject presses kb to continue from instructions to experiment

    public ULinkBool_In gotSubjectDetails; // make sure the subject has entered necessary personal details before running

    // other parameters (internal)
    private int blockIndex = 0; //the index of the stimuli and ISIs for the current block
    private int block = 0; // the current block index (out of multiple blocks)
    private List<int> stimList; // the full stimuli list of the entire session (not divided to blocks)
    private List<int> ISIList; // the full ISI list of the entire session (not divided to blocks)
    public bool randomize = true;

    public int quitAppDelaySec;
    private int numOfTrialsPerBlock; // meaning, all stim + repetitions

    // Start is called before the first frame update
    void Start()
    {
        // at the beginning, log subject info to SESSION file
        RecordLog("Subject Age: " + GlobalSettings.Instance.subjectAge); // SubjectAge.ToString());
        RecordLog("Subject Dominant Hand: " + GlobalSettings.Instance.subjectDomHand); //SubjectDomHand);
        RecordLog("Subject Gender: " + GlobalSettings.Instance.subjectGender); //SubjectGender);
        // session info:
        RecordLog("Session: Number of Blocks: " + numOfBlocks.ToString());
        RecordLog("Session: Number of Trials Per Block: " + numOfStimPerBlock.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (randomize)
        {
            randomize = false;
            numOfStimPerBlockOut.val = numOfStimPerBlock.val;
            numOfNBrepsPerBlockOut.val = numOfNBackRepsPerBlock.val;
            allISITimesOut.val = allISITimes.val;
            numOfBlocksOut.val = numOfBlocks.val;
            isNBackOut.val = isNBack.val;
            numOfStimOut.val = Enumerable.Range(0, numOfStim.val).ToList();

        }
    }

    // This function runs when the randomizer is done, meaning that we can start the blocks
    public void stimOrderHandler()
    {
        stimList = new List<int>();
        ISIList = new List<int>();

        for (int i=0; i< stimOrder.val.Count; i+=2)
        {
            stimList.Add(stimOrder.val[i]);
            ISIList.Add(stimOrder.val[i+1]);
        }

        // write to log the stiumuli order 
        RecordLog("Stim Order In Session:");
        string stimOrderStr = "";
        foreach (int o in stimList)
        {
            stimOrderStr = stimOrderStr + " " + o.ToString();
        }
        RecordLog(stimOrderStr);
        RecordLog("End of Stim Order List");

        // write to log the stiumuli SOA order 
        RecordLog("SOA Order In Session:");
        string isiOrderStr = "";
        foreach (int o in ISIList)
        {
            isiOrderStr = isiOrderStr + " " + o.ToString();
        }
        RecordLog(isiOrderStr);
        RecordLog("End of SOA Order List");

        //runInstructionsStart(); // show instructions
    }

    public void gotSubjectDetailsHandler()
    {
        runInstructionsStart(); // show instructions
    }

    public void runInstructionsStart()
    {
        runInstructionsStartOut.val = true; // we are at the begining of the experiment
    }

    public void runExpEnd()
    {
        runExpEndOut.val = true; // we are at the end of the experiment
    }


    public void selfPaceHandler()
    {
        DebugLog("Got response from subject");
        // record to log
        RecordLog("Subject start experiment (finished instructions)");
        if (runInstructionsStartOut.val)
        {
            runInstructionsStartOut.val = false;
            runNextBlock(); // start experiment
        }
    }


    // called when a block ends (see blockEndHandler) and sends Block unit the relevant stimuli and isi indices
    public void runNextBlock()
    {
        numOfTrialsPerBlock = numOfStimPerBlock.val + numOfNBackRepsPerBlock.val[block]; // calculate the actual number of TRIALS per block - this will be sent to block
        ISITimesOut.val = ISIList.GetRange(blockIndex, numOfTrialsPerBlock);
        stimTimesOut.val = allStimTimes.val;
        maskTimesOut.val = allMaskTimes.val;
        timeToRateOut.val = timeToRate.val;
        ITIOut.val = ITI.val;
        stimOrderOut.val = stimList.GetRange(blockIndex, numOfTrialsPerBlock); //SENT TO BLOCK: The order is important!!! - the listener is on this variable, must be last.
        // Starting the first trial after stim order is in

        blockIndex += numOfTrialsPerBlock;
        block += 1;
    }

    // This function calls the next block if there is one
    public void blockEndHandler()
    {
        if (blockIndex < stimList.Count && 
            blockIndex + numOfTrialsPerBlock <= stimList.Count &&
            block < numOfBlocks.val)
        {
            runNextBlock();
        }

        else
        {
            runExpEnd(); // show subject the "thank you" screen
            StartCoroutine(Quit());
        }
    }
    IEnumerator Quit()
    {
        yield return new WaitForSeconds(quitAppDelaySec);
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
