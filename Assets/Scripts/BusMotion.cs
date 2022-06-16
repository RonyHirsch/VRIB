using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.Linq;
using System;
using System.Globalization;


/*
 * This is the script that controls the bus motion. Currently being in use by BusBeesAndPlayer object, which includes the bus, all the bees and the camera the player looks through.
 * Right now we only mark the destination of the bus ride (meaning it assumes a drive in a straight line - if we want to change it we need dest to become a list of locations).
 */

public class BusMotion : ULinkBase
{
    // IN
    public ULinkVector3_In dest; // the destination of the bus ride (where the bus will stop)
    public ULinkFloat_In speed; // bus speed
    public ULinkBool_In isValidationEnd; // did the validation end successfully
    public float freezeAtStartDuration;

    // OUT
    public ULinkVector3_Out busPosition; // current position of the bus
    public ULinkBool_Out isEndOfTrial; // when bus stops, it outputs this variable as "true" to mark the trial has ended
    public ULinkFloat_Out busSpeed; // out to all other cars 

    // OUT to BeeMotion for final location of bees
    public ULinkInt_Out beeTargetLoc;
    public ULinkInt_Out beeDistractALoc;
    public ULinkInt_Out beeDistractBLoc;


    // private
    private float startTime; // documents the time we start
    private bool doOnce = false;
    private bool printOnce = false; // This is used in order to log the REAL start of the trial

    // Start is called before the first frame update
    void Start()
    {
        busPosition.val = transform.position;
        isEndOfTrial.val = false;
        busSpeed.val = 0;
        RecordLog("trialBusSpeed" + "\t" + speed.val);
    }

    private void OnEnable()
    {
  
    }

    // Update is called once per frame
    void Update()
    {
        busPosition.val = transform.position;

        if (speed.val > 0 && (Time.realtimeSinceStartup > startTime + freezeAtStartDuration) && isValidationEnd.val) 
        {
            // right now, the direction of the bus' ride is in one straight line between its current location and the breadcrumb which marks its destination. 
            Vector3 direction = (dest.val - transform.position).normalized;
            if (direction.x < 0)
            {
                transform.position = transform.position + (direction * speed.val * Time.deltaTime);
                busSpeed.val = speed.val;
                RecordLog("busPosition" + "\t" + busPosition.val);
            }
            else
            {
                // bus stops and bee's final locations (for the bee question) are set
                if (!doOnce)
                {
                    System.Random rnd = new System.Random();
                    List<int> finalLocations = Enumerable.Range(0, 3).OrderBy(x => rnd.Next()).Take(3).ToList();
                    beeTargetLoc.val = finalLocations[0];
                    beeDistractALoc.val = finalLocations[1];
                    beeDistractBLoc.val = finalLocations[2];
                    isEndOfTrial.val = true;
                    busSpeed.val = 0;
                    doOnce = true;
                }
            }
        }
    }

    public void isValidationEndHandler()
    {
        if (isValidationEnd.val)
        {
            startTime = Time.realtimeSinceStartup;
            RecordLog("RealTrialStart" + "\t" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
        }
        
    }
}
