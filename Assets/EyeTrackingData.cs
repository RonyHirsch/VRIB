using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using VRsqrCore;

public class EyeTrackingData : ULinkBase //MonoBehaviour
{
    public ULinkVector3_Out eyeGazeDirection;
    public ULinkVector3_Out eyeGazeOrigin;
    public ULinkBool_Out eyeGazeLeftEyeBlinking;
    public ULinkBool_Out eyeGazeRightEyeBlinking;

    // Start is called before the first frame update
    void Start()
    {
        var settings = new TobiiXR_Settings();
        TobiiXR.Start(settings);
    }

    // Update is called once per frame
    void Update()
    {
        // Get eye tracking data in world space
        var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

        // Check if gaze ray is valid
        if (eyeTrackingData.GazeRay.IsValid)
        {
            // The origin of the gaze ray is a 3D point
            //var rayOrigin = eyeTrackingData.GazeRay.Origin;
            // The direction of the gaze ray is a normalized direction vector
            //var rayDirection = eyeTrackingData.GazeRay.Direction;

            eyeGazeOrigin.val = eyeTrackingData.GazeRay.Origin;
            RecordLog("eyeGazeOrigin: " + eyeGazeOrigin.val.ToString("F3"));

            eyeGazeDirection.val = eyeTrackingData.GazeRay.Direction;
            RecordLog("eyeGazeDirection: " + eyeGazeDirection.val.ToString("F3"));

            eyeGazeLeftEyeBlinking.val = eyeTrackingData.IsLeftEyeBlinking;
            RecordLog("eyeGazeLeftEyeBlinking: " + eyeGazeLeftEyeBlinking.val);

            eyeGazeRightEyeBlinking.val = eyeTrackingData.IsRightEyeBlinking;
            RecordLog("eyeGazeRightEyeBlinking: " + eyeGazeRightEyeBlinking.val);
        }

        /*
        // For social use cases, data in local space may be easier to work with
        var eyeTrackingDataLocal = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

        // The EyeBlinking bool is true when the eye is closed
        var isLeftEyeBlinking = eyeTrackingDataLocal.IsLeftEyeBlinking;
        var isRightEyeBlinking = eyeTrackingDataLocal.IsRightEyeBlinking;

        // Using gaze direction in local space makes it easier to apply a local rotation
        // to your virtual eye balls.
        var eyesDirection = eyeTrackingDataLocal.GazeRay.Direction;
        */
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
        TobiiXR.Stop();
    }
}
