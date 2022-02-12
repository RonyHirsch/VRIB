using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.Linq;

/*
 * This script is the one that moves the "sphere" of bees towards and away from billboards in the game.
 * MotionPivot is a CHILD of BUS, so everything here is RELATIVE to the bus anyway
 */
public class motionPivot : ULinkBase
{
    [SerializeField]
    private List<Vector3> billboardPositions = new List<Vector3>(); // the list of billboard positions, filled by the billboardLocation In variable, and sorted at the beginning in increasing distance form the bus's initial location
    // remember we don't need the bus location since the Motion Pivot object is a child of BUS
    
    // IN VARS
    // FROM USER
    public float sphereRadius; // the radius of the sphere containing all the bees. In var from user
    public float rightSideLimit; // the R limit beyond which we don't want the sphere with the bees to go when approaching a billboard, since it's not visible to the user 
    public float leftSideLimit; // the L limit beyond which we don't want the sphere with the bees to go when approaching a billboard, since it's not visible to the user 
    // ULINK
    public ULinkVector3_In billboardLocation; // the input var through which each billboard sends us its location
   

    // OUT VARS
    public ULinkVector3_Out sphereCenter; // the sphere center
    public ULinkFloat_Out sphereRadiusOut; // the sphere radius, out to BeeMotion

    // INTERNAL
    [SerializeField]
    public int closestBillboard; // index of the closest billboard to bus from the list of billboard locations
    public Quaternion lastBillboardRotation;
    public Vector3 lastBillboardPosition;
    public Quaternion nextBillboardRotation;
    public Vector3 nextBillboardPosition;
    public float rotationSpeed = 5;
    public bool readyForSwitch = true; 


    // Start is called before the first frame update
    void Start()
    {
        closestBillboard = 0;
        sphereRadiusOut.val = sphereRadius;
        RecordLog("sphereRadius" + "\t" + sphereRadius);
        RecordLog("sphereRightSideLimit" + "\t" + rightSideLimit);
        RecordLog("sphereLeftSideLimit" + "\t" + leftSideLimit);
    }

    // Update is called once per frame
    void Update()
    {
        // on each frame, check what is the 
        if (closestBillboard < billboardPositions.Count())
        {
            //Vector3 forward = billboardPositions[closestBillboard] - transform.position; // rotate the transform so it will point to the direction of the closest billboard's location
            //Quaternion nextTarget = Quaternion.LookRotation(forward); // smooth the movement so that the bee-sphere won't "jump" 
            //nextTarget *= Quaternion.Euler(0, 90, -12); // correct Y due to bus asset rotation, correct Z so that the bees will be higher in the scene 
            //transform.rotation = Quaternion.Lerp(transform.rotation, nextTarget, 0.4f);
            //transform.LookAt(billboardPositions[closestBillboard]); // rotate the transform so it will point to the direction of the closest billboard's location
            //transform.Rotate(0, 90, 0); // apply a 90-euler degree roataion on the Y axis : to move the bees towards the billboard?
            
            Vector3 forward = billboardPositions[closestBillboard] - transform.position; // rotate the transform so it will point to the direction of the closest billboard's location
            Quaternion nextBillboardRotation = Quaternion.LookRotation(forward); // smooth the movement so that the bee-sphere won't "jump" 
            nextBillboardRotation *= Quaternion.Euler(0, 90, -12); // correct Y due to bus asset rotation, correct Z so that the bees will be higher in the scene 
            float distFromLastZ = Mathf.Abs(transform.position.z - nextBillboardPosition.z);
            float distBetweenBillboardsZ = Mathf.Abs(lastBillboardPosition.z - nextBillboardPosition.z);
            float motionFraction = distFromLastZ / distBetweenBillboardsZ; // normalized distance between previous and next billboards
            RecordLog("lastBillboardPosition.z" + "\t" + lastBillboardPosition.z + "\t" + "nextBillboardPosition.z" + "\t" + nextBillboardPosition.z + "\t" + "normDistBetPrevAndNextBillboards" + "\t" + motionFraction + "\t" + "distBetweenPrevAndNextBillboardsZ" + "\t" + distBetweenBillboardsZ + "\t" + "spherePosition" + "\t" + transform.position + "\t" + "distBetSphereAndLastBillboardZ" + "\t" + distFromLastZ);
            transform.rotation = Quaternion.Lerp(nextBillboardRotation, lastBillboardRotation, Mathf.Pow(motionFraction, rotationSpeed));
            if (readyForSwitch && ((transform.localEulerAngles.y < leftSideLimit) || (transform.localEulerAngles.y > rightSideLimit) || transform.position.z > (nextBillboardPosition.z - 0.5f)))
            //if (readyForSwitch && transform.position.x < (nextBillboardPosition.x + 0.5f))
            {
                readyForSwitch = false;
                closestBillboard++;
                SwitchToNextBillboard();
            }
            else
            {
                readyForSwitch = true;
            }

        }

    }

    void SwitchToNextBillboard()
    {
        if (closestBillboard < billboardPositions.Count())
        {
            lastBillboardRotation = transform.rotation;
            lastBillboardPosition = transform.position;
            nextBillboardPosition = billboardPositions[closestBillboard];
            RecordLog("closestBillboardIndex" + "\t" + closestBillboard);
            RecordLog("closestBillboardPosition" + "\t" + billboardPositions[closestBillboard]);
        }
    }

    
    public void billboardLocationHandler()
    {
        billboardPositions.Add(billboardLocation.val);
        // sort list of billboard locs by distance from the bus [NOTE: this relies on this sort being performed at the beginning of the trial!]
        billboardPositions = billboardPositions.OrderBy(v => Vector3.Distance(v, transform.position)).ToList();
        //transform.LookAt(billboardPositions[0]); // rotate the transform so it will point to the direction of the closest billboard's location
        SwitchToNextBillboard();
    }
    
}
