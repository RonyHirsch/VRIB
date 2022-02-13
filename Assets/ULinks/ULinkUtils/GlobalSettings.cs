using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRsqrCore;

namespace Assets.Scripts.ULinkUtils
{
    public class GlobalSettings : ULinkBase // MonoBehaviour
    {
        public static GlobalSettings Instance;

        // IN
        public ULinkString_In dataPath;
        public ULinkString_In subjectCode;
        public ULinkString_In subjectAge;
        public ULinkString_In subjectDomHand;
        public ULinkString_In subjectGender;


        // OUT
        public ULinkString_Out dataPathOut;
        public ULinkString_Out subjectCodeOut;

        //public SubjectSettings subjSettings;
        //public static SubjectSettings SubjSettings;

        public VRsqrUtil.LogLevel maxLogLevel = VRsqrUtil.LogLevel.None;
        public string messageFilter;

        public string currSubPath = "C:\\Users\\Liadlab\\Documents\\VR_output\\currSub.txt";  // the path to the file the subject code will be saved into

        private static DateTime initDateTime;
        private static int initDateTimeMS;
        private static bool initializedTime = false;
        private bool do_once = true;
        public string currentSubCode; //A string that holds current subject code

        void Start()
        {
            currentSubCode = File.ReadAllText(currSubPath);
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Awake()
        {
            if (!initializedTime)
            {
                initializeTime();
            }
        }

        public static DateTime InitDateTime()
        {
            if (!initializedTime)
            {
                initializeTime();
            }
            return initDateTime;
        }

        public static int InitDateTimeMS()
        {
            if (!initializedTime)
            {
                initializeTime();
            }
            return initDateTimeMS;
        }

        private static void initializeTime()
        {
            initDateTime = DateTime.Now;
            initDateTimeMS = DateTimeToMS(initDateTime);
            initializedTime = true;
        }

        public static int DateTimeToMS(DateTime time)
        {
            int timeMS = time.Hour * 60 * 60 * 1000 + time.Minute * 60 * 1000 + time.Second * 1000 + time.Millisecond;
            return timeMS;
        }

        public static double TimeStamp()
        {
            DateTime now = DateTime.Now;
            TimeSpan interval = now - initDateTime;
            return interval.TotalMilliseconds;
        }

        // Update is called once per frame
        void Update()
        {
            if (do_once)
            {
                do_once = false;
                if (!File.Exists(dataPath.val + "\\" + currentSubCode))  // if the subject folder does not even exist - open one for all the files to be saved in
                {
                    Directory.CreateDirectory(dataPath.val + "\\" + currentSubCode);
                }
                dataPathOut.val = dataPath.val;
                subjectCodeOut.val = currentSubCode;
            }
        }
    }
}
