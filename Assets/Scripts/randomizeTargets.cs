using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRsqrCore;

/*
 * This is the script that randomizes the ET VALIDATION targets at the beginning of each trial. It is connected to the ETValidation object. 
 */
public class randomizeTargets : ULinkBase
{

    public Renderer[] stimRenderers;
    public float HighlightDuration;
    public float WaitBetweenTargetsDuration;
    public Color HighlightColor;
    private bool isNextTarget = true;
    private int i = -1;
    private Color _originalColor;
    private float startTime; // documents the time we start
    private float waitTime = 0f;  // documents the time we've been waiting (off)
    private List<int> indList;

    // IN
    // IN from HighlightAtGaze
    public ULinkBool_In didLookAtTarget; // whether or not the current target square was looked at (meaning, gaze was directed to it and tracking worked properly)
    // IN from ValidationInstructions 
    public ULinkBool_In skipValidation; // This in order to skip the validation after instructions. Ulink is Contorllers Connector, and it is from startValidation. 

    // OUT
    public ULinkBool_Out isValidationEnd; // did this round of validation end
    public ULinkInt_Out hitCount;  // how many of the 9 target squares were successfully looked at + OUT to isValidationEnd's "Text (TMP)" component
    public ULinkBool_Out[] isInterestingArray; // THIS IS NOT OUT TO ANYONE - IT'S ULINKBOOL OUT JUST BECAUSE IT HOLDS ALL THE ULINKBOOLOUTS 
    // OUT to HighlightAtGaze -- separate params in order to separate the targets and be able to keep track of whether a target was gazed at or not
    public ULinkBool_Out isInteresting1; // is this square relevant
    public ULinkBool_Out isInteresting2; 
    public ULinkBool_Out isInteresting3; 
    public ULinkBool_Out isInteresting4; 
    public ULinkBool_Out isInteresting5; 
    public ULinkBool_Out isInteresting6; 
    public ULinkBool_Out isInteresting7; 
    public ULinkBool_Out isInteresting8; 
    public ULinkBool_Out isInteresting9;

    // 
    public bool[] targetCounted;
    //public ULinkBool_In isTrialStart; // DO WE NEED TO TURN OFF EVERYTHING SINCE TRIAL IS STARTING

    // Start is called before the first frame update
    private void OnEnable()
    {
        targetCounted = new bool[10];
        startValidation();
    }
    /*
    Start()
    {
        startValidation();
    }
    */

    // Update is called once per frame
    void Update()
    {
        if ( i >= stimRenderers.Length)  // finished all targets
        {
            RecordLog("ETValidationHits" + "\t" + hitCount.val.ToString());
            isValidationEnd.val = true;
            switchTargets(false);
            return;
        }

        if (Time.realtimeSinceStartup <= waitTime + WaitBetweenTargetsDuration)  // waiting between targets
        {
            return;
        }


        if (isNextTarget)  // if we need to start time from zero (new target)
        {
            startTime = Time.realtimeSinceStartup;  // start time
            i++; // target index
            if (i >= 0 && i < indList.Count)
                _originalColor = stimRenderers[indList[i]].material.color;  // take the original color
        }

        if (i >= 0 && i < indList.Count)
        {
            isNextTarget = LightUp(stimRenderers[indList[i]], isInterestingArray[i]);  // value of boolean depends on the target situation
        }
            
        if (isNextTarget)
        {
            waitTime = Time.realtimeSinceStartup;  // start wait time over 
        }
    }

    void startValidation()
    {
        InitArray();
        hitCount.val = 0;
        i = -1;
        System.Random rnd = new System.Random();
        var randomNumbers = Enumerable.Range(0, stimRenderers.Length).OrderBy(x => rnd.Next()).Take(stimRenderers.Length).ToList();
        indList = (from num in randomNumbers select num).ToList();
        RecordLog("ETValidationTargetIndOrder" + "\t" + string.Join(";", indList));
        isValidationEnd.val = false;
        switchTargets(true);
    }


    public bool LightUp(Renderer rend, ULinkBool_Out isInteresting)
    {
        if (Time.realtimeSinceStartup <= startTime + HighlightDuration) // if target should be Lit (colored in HighlightColor)
        {
            isInteresting.val = true;
            rend.material.color = HighlightColor;
            return false; // we are using the same renderer and time

        }
        else  // target should be off 
        {
            isInteresting.val = false;
            rend.material.color = _originalColor;  // return to original color
            return true;  // we move on to the next renderer, meaning we need to start time from zero again
        }

    }

    public void switchTargets(bool targetsOn)
    {
        for (int i=0; i < stimRenderers.Length; i++)
        {
            stimRenderers[indList[i]].enabled = targetsOn;
        }
    }

    public void didLookAtTargetHandler()
    {
        if (didLookAtTarget.val == true)  // if a target was looked at successfully
        {
            for (int i = 0; i < stimRenderers.Length; i++)
            {
                if (stimRenderers[indList[i]].enabled == true && !targetCounted[indList[i]])
                {
                    hitCount.val++;
                    targetCounted[indList[i]] = true;
                }
            }
        }
    }

    public void InitArray()
    {
        isInterestingArray = new ULinkBool_Out[10];
        isInterestingArray[0] = isInteresting1;
        isInterestingArray[1] = isInteresting2;
        isInterestingArray[2] = isInteresting3;
        isInterestingArray[3] = isInteresting4;
        isInterestingArray[4] = isInteresting5;
        isInterestingArray[5] = isInteresting6;
        isInterestingArray[6] = isInteresting7;
        isInterestingArray[7] = isInteresting8;
        isInterestingArray[8] = isInteresting9;
    }

    public void skipValidationHandler()
    {
        // SKIPPING validation
        RecordLog("ETValidationHits" + "\t" + "SKIPPED!");
        isValidationEnd.val = true;
        switchTargets(false);
    }



    /*
    public void isTrialStartHandler()
    {
        if (isTrialStart.val == false)
        {
            for (int i = 0; i < stimRenderers.Length; i++)  // repeat validation, so start everything
            {
                stimRenderers[indList[i]].enabled = true;
            }
            startValidation();
        }
        else  // turn off entire object
        {
            gameObject.SetActive(false); 
        }
    }
    */
}
