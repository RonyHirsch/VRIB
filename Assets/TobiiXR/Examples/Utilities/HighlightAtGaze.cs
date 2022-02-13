// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;
using VRsqrCore;

namespace Tobii.XR.Examples
{
//Monobehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus

    /*
     * The original script was EDITED to accomodate Mudriklab internal VR system, and communicate lighting up to other game objects. 
     */

    public class HighlightAtGaze : ULinkBase, IGazeFocusable// MonoBehaviour, IGazeFocusable
    {
        public Color HighlightColor = Color.red;
        public float AnimationTime = 0.1f;

        private Renderer _renderer;
        private Color _originalColor;
        private Color _targetColor;

        private float startGazeTime;
        private float endGazeTime;
        private bool isGazing = false;

        // IN from randomizedTargets
        public ULinkBool_In currentlyInteresting; // Is the square the current target square

        // OUT TO randomizedTargets
        public ULinkBool_Out didLookAtTarget; // whether or not the current target square was looked at (meaning, gaze was directed to it and tracking worked properly)

        // OUT 
        public float gazeTime;
        public float minGazeTime; 




        //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
        public void GazeFocusChanged(bool hasFocus)
        {
            //If this object received focus, fade the object's color to highlight color
            if (hasFocus)
            {
                _targetColor = HighlightColor; 
                startGazeTime = Time.realtimeSinceStartup;
                // manual additions to template script (Rony) to log gaze at the image 
                RecordLog("startGazeTime" + "\t" + startGazeTime);
            }
            //If this object lost focus, fade the object's color to it's original color
            else
            {
                _targetColor = _originalColor;
                endGazeTime = Time.realtimeSinceStartup;
                gazeTime = Mathf.Max(endGazeTime - startGazeTime, gazeTime);
                // manual additions to template script (Rony) to log gaze at the image 
                RecordLog("endGazeTime" + "\t" + endGazeTime);
                RecordLog("GazeDuration" + "\t" + gazeTime);
            }
            isGazing = hasFocus;

            if (currentlyInteresting.val == true && isGazing == true)  // if the square is the current target and subject gaze was on it
            {
                didLookAtTarget.val = true;  // then target was looked at successfully
            }

        }

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _originalColor = _renderer.material.color;
            _targetColor = _originalColor;
        }

        private void Update()
        {
            //This lerp will fade the color of the object
            if (_renderer.material.HasProperty(Shader.PropertyToID("_BaseColor"))) // new rendering pipeline (lightweight, hd, universal...)
            {
                _renderer.material.SetColor("_BaseColor", Color.Lerp(_renderer.material.GetColor("_BaseColor"), _targetColor, Time.deltaTime * (1 / AnimationTime)));
            }
            else // old standard rendering pipline
            {
                _renderer.material.color = Color.Lerp(_renderer.material.color, _targetColor, Time.deltaTime * (1 / AnimationTime));
            }

            if (isGazing)
            {
                gazeTime = Mathf.Max(Time.realtimeSinceStartup - startGazeTime, gazeTime);  // don't need subject to "leave" target to measure time on it
            }

        }


        public bool checkGazeTime()
        {
            bool aboveThreshold = gazeTime >= minGazeTime;
            gazeTime = 0f;
            return aboveThreshold;
        }

        public void startMeasure()
        {
            gazeTime = 0f;

        }

        public void currentlyInterestingHandler()
        {
            if (currentlyInteresting.val == true && isGazing == true)  // if the square is the current target and subject gaze was on it
            {
                didLookAtTarget.val = true;  // then target was looked at successfully
            }
        }
    }
}
