// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.G2OM;
using UnityEngine;
using ViveSR.anipal.Eye;

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
    }
}
