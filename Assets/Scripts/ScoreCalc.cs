using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using VRsqrCore;
using System;

public class ScoreCalc : ULinkBase
{
    // PARAMS
    public float startMoney = 0f;
    public float correctGain = 1f;
    public float wrongLoss = -1f;
    public float clueWorth = -0.5f;
    public float minimalBeeSpeed = 0.1f;
    public float maximalBeeSpeed = 3.0f;
    public float currSum;
    public GameObject threshold; // this is the object who's position is the threshold from which the clue will become more expensive by a factor of hint_priceIncrease_factor

    // IN
    public ULinkBool_In isCorrectInBee;  // IN FROM controllerProjection of TargetBeeSelection
    public ULinkBool_In isClue; // IN FROM controller Right CluePress
    // in from BeeMotion for clue count logic
    public ULinkBool_In activateClueIn;
    public ULinkBool_In startCountingCluesIn;

    // IN FROM GLOBAL SETTINGS
    public ULinkString_In outputPath;
    public ULinkString_In subFolder;

    // IN FROM BEES - REASON: this script (controllerProjection) is connected to "TargetBeeSelection" object, which informs of the selection (or lack thereof) of the target bee.
    // based on that, we want to change the bees' speed. The thing is that selection of bees also TURNS THEM OFF, as once a bee is selected we move on the the subjective question.
    // thus we can't update the bees' speed for the next trial based on the answer to the bee question, as they are off. we'll do that here instead (the writing to file) and reading to it will still
    // remain in the BeeMotion script that's on top of each bee.
    // IN FROM BEEMOTION
    public ULinkFloat_In beeSpeed;
    public ULinkFloat_In speedStep;

    // IN from BusMotion: bus location. We use this and a pre-defined location (the last crossroads) to make a clue more expensive (towards the end of a trial). 
    public ULinkVector3_In busPosition; // current position of the bus

    public ULinkString_Out subjectSpeedPathOut;  // OUT TO BeeMotion 

    TextMeshPro scoreText;

    // internal : paths and counter for keeping up score and writing to files
    private string subjectScorePath;
    private string trialSummaryPath;
    private string subjectSpeedPath;
    private int hint_counter;
    private int hint_priceIncrease_factor = 2; // we will multiply clueWorth by this to increase the clueWorth towards the end of the trial
    private bool clueRiseFlag = false; // did we already increase the clue worth in this trial

    // internal 
    private float nextSpeed;

    int score = 0;

    // Start is called before the first frame update
    void Start()
    {
        currSum = startMoney;
        scoreText = GetComponentInChildren<TextMeshPro>(); //FindObjectOfType<TextMeshPro>();
        RecordLog("startMoney" + "\t" + startMoney);
        RecordLog("PointsWhenCorrect" + "\t" + correctGain);
        RecordLog("PointsWhenWrong" + "\t" + wrongLoss);
        RecordLog("PointsWhenClue" + "\t" + clueWorth);
        RecordLog("minimalBeeSpeed" + "\t" + minimalBeeSpeed);
        RecordLog("maximalBeeSpeed" + "\t" + maximalBeeSpeed);

    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "₪ = " + currSum;
        
    }

    public void isCorrectInBeeHandler()
    {
        if (isCorrectInBee.val)
        {
            currSum += correctGain;
            // DEFINE A MAXIMUM SPEED FOR BEES, OTHERWISE THIS WILL DO TO INF
            nextSpeed = Math.Min(beeSpeed.val + speedStep.val, maximalBeeSpeed);
        }
        else
        {
            currSum += wrongLoss;
            // DEFINE A MINUMUM SPEED FOR BEES, OTHERWISE THIS WILL DO TO ZERO
            nextSpeed = Math.Max(beeSpeed.val - speedStep.val, minimalBeeSpeed);
        }
        RecordLog("CurrPoints" + "\t" + currSum);

        Debug.Log("isCorrectInBeeHandler 1");

        File.WriteAllText(subjectSpeedPath, nextSpeed.ToString());
        writeSubjectScore(); // update the subject trial sheet to upload at the beginning of the next trial 
        writeTrialSummary(); // add a line to the subject summary file with details about bee and score 

        Debug.Log("isCorrectInBeeHandler 2");

    }


    public void writeSubjectScore()
    {
        System.IO.File.WriteAllText(subjectScorePath, currSum.ToString());
    }

    public void writeTrialSummary()
    {
        string str_to_write = "correct" + "\t" + isCorrectInBee.val + "\t" + "hints" + "\t" + hint_counter.ToString() + "\t" + "score" + "\t" + currSum.ToString() + "\n";
        //System.IO.File.WriteAllText(trialSummaryPath, str_to_write);
        File.AppendAllText(trialSummaryPath, str_to_write + "\n");
    }

    public void activateClueInHandler()
    {
        if (startCountingCluesIn.val && activateClueIn.val)
        {
            currSum += clueWorth;
            RecordLog("CurrPoints" + "\t" + currSum);
        }
        
    }

    public void subFolderHandler()
    {
        // Handler only to subFolder because this is the second parameter of the two strings
        string subjectPath = outputPath.val + "\\" + subFolder.val;

        subjectScorePath = subjectPath + "\\" + "currTrialScore.txt"; // current trial score - accumulated, to know what value to start with during the following trial. 
        trialSummaryPath = subjectPath + "\\" + "trialScoreData.txt"; // score data: correct in bee + num of hints in trial + score in end of trial
        subjectSpeedPath = subjectPath + "\\" + "nextSpeed.txt"; // current Trial Speed

        subjectSpeedPathOut.val = subjectSpeedPath;

        if (!File.Exists(subjectScorePath))
        {
            // if we DON'T have a currTrialScore file to load curr Sum from.
            currSum = startMoney; //No file, we will write when we are done with the trial, starting from 0
        }
        else
        {
            // if we have a currTrialScore file to load curr Sum from. 
            currSum = float.Parse(File.ReadAllText(subjectScorePath));
        }
    }

    public void busPositionHandler()
    {
        // right now, the direction of the bus' ride is in one straight line between its current location and the breadcrumb which marks its destination. 
        Vector3 direction = (threshold.transform.position - busPosition.val).normalized;
        
        if ((direction.z < 0) && !(clueRiseFlag))
        {
            clueWorth = hint_priceIncrease_factor * clueWorth;
            clueRiseFlag = true;
        }
    }
}
