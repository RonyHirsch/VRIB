using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.Linq;
using UnityEngine.SceneManagement;

public class objectiveQProjection : ULinkBase
{
    public float motionScale;
    public float distance;
    public GameObject markerPrefab;
    private Transform markerTransform;
    //IN
    public ULinkVector3_In controllerPosition;
    public ULinkVector3_In controllerDirection;
    public ULinkBool_In controllerButtonPress;
    public ULinkInt_In correctAnsLoc; // the name of the marker or the correct answer

    // IN FROM GLOBAL SETTINGS
    public ULinkString_In outputPath;
    public ULinkString_In subFolder;
    public ULinkInt_In currTrialIndexIn; // IN from RandomizeTrials which is on BLOCK - to know the ind of the current stimulus in the stimulus list


    //OUT
    public ULinkInt_Out selectedTargetInd;
    public ULinkBool_Out isTargetSelected;
    public ULinkBool_Out isCorrectAnswer; // whether the selected target is the one with the right picture or not

    public Transform[] markers;

    private void OnEnable()
    {
        isTargetSelected.val = false;
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
                    isCorrectAnswer.val = (closest == correctAnsLoc.val); // bool, whether the selected answer is correct
                    //FlushVarsLogs();

                    string subjectPath = outputPath.val + "\\" + subFolder.val;
                    string currStimPath = subjectPath + "\\" + "nextStimInd.txt";
                    System.IO.File.WriteAllText(currStimPath, (currTrialIndexIn.val + 1).ToString()); // + 1 for the next stimulus

                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // get the name of the active scene and then load it
                }
            }
        }
    }

}
