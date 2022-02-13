using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.Linq;
using UnityEngine.SceneManagement;

public class valenceQProjection : ULinkBase
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
    public ULinkInt_Out selectedTargetInd; // index of the selected answer. In the subjective: 0, 1, 2, 3
    public ULinkBool_Out isTargetSelected;

    public Transform[] markers;

    private void OnEnable()
    {
        markerTransform = Instantiate(markerPrefab).transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = controllerPosition.val + (controllerDirection.val * motionScale) + new Vector3(0, 0, distance);
        markerTransform.position = position;


        if (markers.Length > 0)
        {
            float minDist = float.MaxValue;
            int closest = -1;
            for (int i = 0; i < markers.Length; i++)
            {
                markers[i].gameObject.SetActive(false);
                Transform markedObj = markers[i];
                float dist = Utils3D.DistancePointRay(controllerPosition.val, controllerDirection.val, markedObj.position);
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
                }
            }
        }
    }

}
