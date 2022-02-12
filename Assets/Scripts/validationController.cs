using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using TMPro;
using System.Linq;

public class validationController : ULinkBase
{
    public float motionScale;
    public float distance;
    public GameObject markerPrefab;
    private Transform markerTransform;
    //IN
    public ULinkVector3_In controllerPosition;
    public ULinkVector3_In controllerDirection;
    public ULinkBool_In controllerButtonPress;
    public ULinkInt_In numOfHitsIn; // IN from ETValidation (randomizeTargets), number of successful validation targets

    //OUT
    public ULinkInt_Out selectedTargetInd;
    public ULinkBool_Out isTargetSelected;
    public ULinkBool_Out isTrialStart; // whether to start trial (true) or repeat validation (false)
    public ULinkBool_Out isRepeatingET; // whether to repeat  (true) or repeat validation (false)

    public Transform[] markers;
    TextMeshPro hitsText;  // to communicate the number of validation squares that the gaze was recorded on

    private void OnEnable()
    {
        isTargetSelected.val = false;
        markerTransform = Instantiate(markerPrefab).transform;
        hitsText = GetComponentInChildren<TextMeshPro>();
    }
    /*
    Start()
    {
        markerTransform = Instantiate(markerPrefab).transform;
    }
    */

    // Update is called once per frame
    void Update()
    {
        Vector3 position = controllerPosition.val + (controllerDirection.val * motionScale) + new Vector3(0, 0, distance);
        markerTransform.position = position;

        //Utils3D.DrawLine(controllerPosition.val, controllerDirection.val, 20, Color.red, null, 0.2f); //minus since but moves in descending direction on X axis, and it rotated 90 so it's Z axis

        if (markers.Length > 0)
        {
            float minDist = float.MaxValue;
            int closest = -1;
            for (int i = 0; i < markers.Length; i++)
            {
                markers[i].gameObject.SetActive(false);
                Transform markedObj = markers[i];
                float dist = Utils3D.DistancePointRay(controllerPosition.val, controllerDirection.val, markedObj.position);
                //float dist = Vector3.Distance(markedObj.transform.position, markerTransform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = i;
                }
            }
            if (closest >= 0)
            {
                markers[closest].gameObject.SetActive(true);

                hitsText.text = "Hits: " + numOfHitsIn.val.ToString();
                if (controllerButtonPress.val)
                {
                    selectedTargetInd.val = closest;
                    isTargetSelected.val = true;
                    isTrialStart.val = (markers[closest].gameObject.name == "opt_trialStart"); // bool, whether the selected answer is to start the trial
                    isRepeatingET.val = (markers[closest].gameObject.name != "opt_trialStart"); // bool, whether the selected answer is to repeat the ET
                }
            }
        }
    }

}
