using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.IO;

/*
 * This script is controlling the INDIVIDUAL motion of each and every bee
 */

public class BeeMotion : ULinkBase
{

    // IN
    public float speed;  // speed of the bee motion inside the sphere - THIS IS AFFECTED BY CORRECT/INCORRECT BEE SELECTION BETWEEN TRIALS
    public float speedStep; // this is the amount by which the bees' speed increases / decreases when subjects are correct / incorrect in bee selection
    public ULinkFloat_In radius; // radius of the sphere to move in, comes from MotionPivot object so it's identical to any bee under it
    public float rotationSpeed = 0.2f; // Amount of bee rotation (how 'crazy' it will move)
    public ULinkBool_In isEndOfTrial;
    public float freezeAtStartDuration;
    public ULinkBool_In isClue; // did the subject press for a clue to show target bee marker
    public ULinkInt_In beeTargetLocation; //IN from bus motion, relevant int for final location 
    public ULinkBool_In isReplay; //IN from RandomizeTrials which is on BLOCK - is this a replay or not - FOR REPLAY PURPOSES
    public ULinkInt_In stimInd; // IN from RandomizeTrials which is on BLOCK - to know the ind of the current stimulus in the stimulus list
    //public ULinkBool_In trialStarted;  // since a trial only starts once the eye tracking validation is over. Delete this var + handler to make the bees start moving when "play" is hit.
    //public ULinkBool_In isValidationEnd; // did the validation end successfully

    // IN FROM GLOBAL SETTINGS
    public ULinkString_In outputPath;
    public ULinkString_In subFolder;
    // IN FROM controllerProjection
    public ULinkString_In subjectSpeedPath;

    // INTERNAL VARS
    // public need to be edited manually for each bee:
    public bool isTargetBee = false; // if target bee this is true
    public float targetMarkerDuration = 1f; // how long will the marker around the target bee will last
    public float clueDuration = 1f;

    // private
    private Vector3 currLocation; // bee's current location
    private Quaternion directionToNextPoint; // what actually changes the bee's location
    private Vector3 nextPointInCircle; // the next point the bee needs to move to within the circle
    private List<Vector3> allPointsInCircle; // This is for writing to the disk all the bee's "target points" they moved to during the trial - FOR REPLAY PURPOSES
    private float distanceToNextPoint = 0f; // the distance between the current location and the next point that was randomly chosen in the sphere
    private float prevDistanceToNextPoint = 0f; // the previous distance to next pt
    private float startTime; // documents the time we start
    public int clueCounter = 0;
    private bool ignoreFirstClue = true;
    private float clueStartTime;
    
    // OUT TO SCORESCALE - SAME LOGIC FOR COUNTING CLUE-RELATED MONEY
    public ULinkBool_Out activateClue;
    public ULinkBool_Out startCountingClues;

    // OUT TO ScoreCalc
    public ULinkFloat_Out beeSpeedOut;
    public ULinkFloat_Out speedStepOut;

    //Those are the points the bees are going to after the trial is over for selection
    private Vector3 pointA = new Vector3(0, 0, 1);
    private Vector3 pointB = new Vector3(0, 1, 0);
    private Vector3 pointC = new Vector3(0, 0, -1);
    private Vector3 targetPoint;
    private bool completeStop = false;
    
    // Those are related to replay
    private int currPointIndex = 0; //the index in allPointsInCircle - FOR REPLAY PURPOSES 
    private bool beeFileWrite = false;  // in order to do it once per bee
    private Vector3 startPoint; // - FOR REPLAY PURPOSES

    // Start is called before the first frame update
    void Start()
    {
        activateClue.val = false;
        startCountingClues.val = false;
        allPointsInCircle = new List<Vector3>();  //init the all points list 
        startPoint = transform.localPosition;
        //gameObject.SetActive(false);  // this is to make the bees invisible until called
        if (isReplay.val)
        {
            parseBeeLocations();
            nextPointInCircle = allPointsInCircle[currPointIndex++];
            transform.localPosition = startPoint;
        }
        else
        {
            nextPointInCircle = Random.onUnitSphere * radius.val; // draw a random point
            allPointsInCircle.Add(nextPointInCircle); // add it to the list
        }
        directionToNextPoint = Quaternion.LookRotation(nextPointInCircle); // rotate? towards next point
        distanceToNextPoint = Vector3.Distance(transform.localPosition, nextPointInCircle); // calculate distance
        prevDistanceToNextPoint = distanceToNextPoint;
        RecordLog("beeSpeed" + "\t" + speed);
        RecordLog("beeRotationSpeed" + "\t" + rotationSpeed);
        RecordLog("beeSphereRadius" + "\t" + radius.val);
        RecordLog("distanceToNextPoint" + "\t" + distanceToNextPoint);
        RecordLog("nextPointInCircle" + "\t" + nextPointInCircle);
        //startTime = Time.realtimeSinceStartup;
    }

    private void OnEnable()
    {
        startTime = Time.realtimeSinceStartup;
        subjectSpeedPathHandler();
        beeSpeedOut.val = speed;
        speedStepOut.val = speedStep;
    }

    // Experiment to see if now bees are OK.
    void FixedUpdate()
    {
        Quaternion currRotation;

        // bees should not do anything if trial ended (freeze)
        if (isEndOfTrial.val == true) // || trialStarted.val == false)
        {
            if (!isReplay.val && !beeFileWrite)
            {
                writeBeeLocations();
                beeFileWrite = true; // to write the bee things only once
            }

            // moves the bee to its target location, when it is close enough it stops
            if (!completeStop)
            {
                // handle bee movement
                distanceToNextPoint = Vector3.Distance(transform.localPosition, targetPoint);
                if (distanceToNextPoint < 0.2)
                {
                    completeStop = true;
                }
                directionToNextPoint = Quaternion.LookRotation(targetPoint - transform.localPosition);

                prevDistanceToNextPoint = distanceToNextPoint;
                currRotation = Quaternion.Lerp(transform.localRotation, directionToNextPoint, rotationSpeed);
                transform.localRotation = currRotation;
                transform.position += transform.forward * speed * Time.deltaTime;
            }

            return;
        }

        // if target bee: mark it for the expected duration
        if (isTargetBee && Time.realtimeSinceStartup > startTime + targetMarkerDuration && !startCountingClues.val)
        {
            startCountingClues.val = true;
            Transform targetMarker = transform.Find("TargetMarker");
            if (targetMarker != null)
            {
                targetMarker.gameObject.SetActive(false);
            }
        }

        // this is for a feature which starts the bees "frozen", meaning, for "freezeAtStartDuration" sec you'll see one marked non-moving bee, and after "freezeAtStartDuration" they will diverge 
        if (Time.realtimeSinceStartup <= startTime + freezeAtStartDuration)
        {
            return;
        }

        if (activateClue.val && Time.realtimeSinceStartup > clueStartTime + clueDuration)
        {
            activateClue.val = false;
            Transform targetMarker = transform.Find("TargetMarker");
            if (targetMarker != null)
            {
                targetMarker.gameObject.SetActive(false);
            }
        }

        // handle bee movement
        distanceToNextPoint = Vector3.Distance(transform.localPosition, nextPointInCircle);
        //Debug.Log("distanceToNextPoint = " + distanceToNextPoint + " | prevDistanceToNextPoint = " + prevDistanceToNextPoint);
        RecordLog("distanceToNextPoint" + "\t" + distanceToNextPoint);
        if (distanceToNextPoint > prevDistanceToNextPoint) // if we are closer to where we were than to where we are going
        {
            if (!isReplay.val)
            {
                nextPointInCircle = Random.onUnitSphere * radius.val; // draw a new point
                allPointsInCircle.Add(nextPointInCircle); // and add it to the list
            }
            else
            {
                if (currPointIndex < allPointsInCircle.Count)
                {
                    nextPointInCircle = allPointsInCircle[currPointIndex++];
                }
                else
                {
                    nextPointInCircle = Random.onUnitSphere * radius.val; // draw a new point
                }
            }
            distanceToNextPoint = Vector3.Distance(transform.localPosition, nextPointInCircle);
            currLocation = transform.localPosition;
            directionToNextPoint = Quaternion.LookRotation(nextPointInCircle - currLocation);
            RecordLog("nextPointInCircle" + "\t" + nextPointInCircle);
            //Debug.Log("choosing next point: nextPointInCircle = " + nextPointInCircle);
        }

        prevDistanceToNextPoint = distanceToNextPoint;
        currRotation = Quaternion.Lerp(transform.localRotation, directionToNextPoint, rotationSpeed);
        transform.localRotation = currRotation;
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void isClueHandler()
    {
        if (isClue.val == true && isTargetBee && !activateClue.val && !isEndOfTrial.val == true)
        {
            if (!startCountingClues.val)
            {
                return;
            }

            Transform targetMarker = transform.Find("TargetMarker");
            if (targetMarker != null)
            {
                targetMarker.gameObject.SetActive(true);
            }

            clueCounter++;
            clueStartTime = Time.realtimeSinceStartup;
            activateClue.val = true;
        }
    }

    public void isEndOfTrialHandler()
    {
        if (isReplay.val)
        {
            return;
        }
        if (beeTargetLocation.val == 0)
        {
            targetPoint = pointA * radius.val;
        }
        if (beeTargetLocation.val == 1)
        {
            targetPoint = pointB * radius.val;
        }
        if (beeTargetLocation.val == 2)
        {
            targetPoint = pointC * radius.val;
        }
    }


    public void subjectSpeedPathHandler()
    {
        // Handler only to subFolder because this is the second parameter of the two strings
        if (File.Exists(subjectSpeedPath.val))
        {
            // if we have a speed file to load curr speed from.
            speed = float.Parse(File.ReadAllText(subjectSpeedPath.val));
        }
        // if file doesn't exist this means this is the FIRST TRIAL and we should use speed as the bees' speed 
    }

    public string vectorToString(Vector3 point)
    {
        return "(" + point.x.ToString() + ", " + point.y.ToString() + ", " + point.z.ToString() + ")";
    }

    public void writeBeeLocations()
    {
        // find out which bee am I (this script is on all 3 bees)
        string beeID = gameObject.name;
        string subjectPath = outputPath.val + "\\" + subFolder.val;
        string subjectBeePath = subjectPath + "\\" + beeID + "_" + stimInd.val.ToString() + "_" + "Locations.txt"; // current bee locations 
        using (TextWriter tw = new StreamWriter(subjectBeePath))
        {
            tw.WriteLine(vectorToString(startPoint));
            tw.WriteLine(vectorToString(targetPoint)); // bee's target point
            tw.WriteLine(speed.ToString());  //bee's speed
            foreach (Vector3 point in allPointsInCircle)
            {
                tw.WriteLine(vectorToString(point));
            }
        }

    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public void parseBeeLocations()
    {
        string beeID = gameObject.name;
        string subjectPath = outputPath.val + "\\" + subFolder.val;
        string subjectBeePath = subjectPath + "\\" + beeID + "_" + stimInd.val.ToString() + "_" + "Locations.txt"; // current bee locations 
        string[] lines = File.ReadAllLines(subjectBeePath);
        speed = float.Parse(lines[2]);  // matches the way it was written in writeBeeLocations
        targetPoint = StringToVector3(lines[1]); // matches the way it was written in writeBeeLocations
        startPoint = StringToVector3(lines[0]); // matches the way it was written in writeBeeLocations
        for (int i = 3; i < lines.Length; i++)
        {
            allPointsInCircle.Add(StringToVector3(lines[i]));
        }
    }

}

