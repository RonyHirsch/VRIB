using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.Linq;

public class subjectiveProjection : ULinkBase
{
    public float motionScale;
    public float distance;
    public GameObject markerPrefab;
    private Transform markerTransform;
    //IN
    public ULinkVector3_In controllerPosition;
    public ULinkVector3_In controllerDirection;
    public ULinkBool_In controllerButtonPress;

    //OUT
    public ULinkInt_Out selectedTargetInd;
    public ULinkBool_Out isTargetSelected;
    public ULinkBool_Out isCorrectInBeeSelection;

    public Transform[] markers;

    // Start is called before the first frame update
    void Start()
    {
        markerTransform = Instantiate(markerPrefab).transform;
    }

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
                if (controllerButtonPress.val)
                {
                    selectedTargetInd.val = closest;
                    isTargetSelected.val = true;
                    isCorrectInBeeSelection.val = (markers[closest].parent.gameObject.name == "bee_target"); // bool, whether the selected bee is the right one
                }
            }
        }
    }

}
