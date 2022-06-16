// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;
using VRsqrCore;
using ViveSR.anipal.Eye;
using System.IO;
using System;
using System.Globalization;

namespace Tobii.XR.Examples
{
    //Monobehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus
    // DOES EXACTLY WHAT HIGHLIGHT AT GAZE DOES - BUT INSTEAD OF HIGHLIGHTING IT RECORDS WHETHER THERE WAS A GAZE OR NOT (BINARY)
    public class RecordAtGaze : ULinkBase, IGazeFocusable// MonoBehaviour, IGazeFocusable
    {
        //public Color HighlightColor = Color.red;
        //public float AnimationTime = 0.1f;

        //private Renderer _renderer;
        //private Color _originalColor;
        //private Color _targetColor;

        private float startGazeTime;
        private float endGazeTime;
        private bool isGazing = false;

        // OUT 
        public float gazeTime;
        public float minGazeTime;
        public float totalGazeDur = 0f;  // the accumulated total gaze duration over this object in the current trial
        public float totalGazeTimes = 0f;  // the number of separate gaze events on this object in the current trial

        // IN
        public ULinkBool_In isCorrectInBee;  // IN FROM controllerProjection of TargetBeeSelection
        public ULinkString_In outputPath; // IN FROM GLOBAL SETTINGS
        public ULinkString_In subFolder;



        //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
        public void GazeFocusChanged(bool hasFocus)
        {
            //If this object received focus, fade the object's color to highlight color
            if (hasFocus)
            {
                // _targetColor = HighlightColor; // RONY COMMENTED IT OUT SO THAT IT WON'T CHANGE IMAGE COLOR
                startGazeTime = Time.time;
                // manual additions to template script (Rony) to log gaze at the image 
                RecordLog("startGazeTime" + "\t" + startGazeTime);
            }
            //If this object lost focus, fade the object's color to it's original color
            else
            {
                //_targetColor = _originalColor;
                endGazeTime = Time.time;
                gazeTime = Mathf.Max(endGazeTime - startGazeTime, gazeTime);
                totalGazeDur += gazeTime;
                totalGazeTimes++;
                // manual additions to template script (Rony) to log gaze at the image 
                RecordLog("endGazeTime" + "\t" + endGazeTime);
                RecordLog("GazeDuration" + "\t" + gazeTime);
            }
            isGazing = hasFocus;
        }

        private void Start()
        {
            //_renderer = GetComponent<Renderer>();
            //_originalColor = _renderer.material.color;
            //_targetColor = _originalColor;
        }

        private void Update()
        {
            /*
            //This lerp will fade the color of the object
            if (_renderer.material.HasProperty(Shader.PropertyToID("_BaseColor"))) // new rendering pipeline (lightweight, hd, universal...)
            {
                _renderer.material.SetColor("_BaseColor", Color.Lerp(_renderer.material.GetColor("_BaseColor"), _targetColor, Time.deltaTime * (1 / AnimationTime)));
            }
            else // old standard rendering pipline
            {
                _renderer.material.color = Color.Lerp(_renderer.material.color, _targetColor, Time.deltaTime * (1 / AnimationTime));
            }
            */

            if (isGazing)
            {
                gazeTime = Mathf.Max(Time.time - startGazeTime, gazeTime);  // don't need subject to "leave" target to measure time on it
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

        private void RecordAtGazePrint()
        {
            // Write the file in a format readable by ParseScrambledMats - FOR REPLAY PURPOSES
            string subjectPath = outputPath.val + "\\" + subFolder.val;
            string subjectScramblePath = subjectPath + "\\" + gameObject.name + "RecordAtGazeBackup.txt"; // current scramble locations

            using (TextWriter tw = new StreamWriter(subjectScramblePath, true))
            {
                tw.WriteLine("CurrentTime" + "\t"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\t" + "totalGazeDur" + "\t" + totalGazeDur + "\t" + "totalGazeTimes" + "\t" + totalGazeTimes);
            }
        }


        public void isCorrectInBeeHandler()
        {
            RecordAtGazePrint();
        }

    }
}
