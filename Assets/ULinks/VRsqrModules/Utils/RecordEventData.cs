#define VERBOSE

using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using VRsqrCore;
using System.Globalization;
using VRsqrUtil;

namespace VR2SaveData
{
    public class RecordEventData : MonoBehaviour
    {

        public bool UseGlobalPath = true;
        public string dataPath = @"D:\Rotem\OneDrive\Work\HaifaUniversity";
        public string subjectsStr = "";

        public string eventName = "";
        public string fullFileName;

        public List<string> DataFieldNames;
        public string initTimeHeader;

        private bool gotEventData = false;
        public List<List<string>> dataLines;
        public int numLines = 0;
        public int numFields = 0;

        public bool saveData = false;


        void OnEnable()
        {
            if (UseGlobalPath)
            {
                dataPath = VRsqrGlobalSettings.DataPrePath;
                subjectsStr = VRsqrGlobalSettings.SubjectsStr;
            }
            string fullSubjectsPath = dataPath + @"\" + subjectsStr;
            fullFileName = fullSubjectsPath + @"\" + eventName + ".txt"; // "_" + DateTime.Now.ToString("s") + ".txt";
            Directory.CreateDirectory(fullSubjectsPath);

            VRsqr_EventsManager.StartListening(eventName, dataEventHandler);

            dataLines = new List<List<string>>();

            VRsqr_InputKeyboard.AddKeyListener(KeyCode.S, toggleRecording, null, "Toggle recording");

            int timeMs = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond; //(float)ts.TotalMilliseconds;
            initTimeHeader = "Start time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\t" + timeMs;

            DataFieldNames.Add("[EventName]");
            numFields++;
            DataFieldNames.Add("[TimeStampOUT]");
            numFields++;
            DataFieldNames.Add("[TimeStampIN]");
            numFields++;
        }

        void dataEventHandler(int EventContext)
        {
            VRsqrUtil.Debug.Log(LogLevel.Debug, "============= dataEventHandler");

            if (saveData)
            {
                UnityEventData eventData = VRsqr_EventsManager.GetEventData(EventContext);
                dataSave(eventData.eventName, eventData.eventParams, eventData.eventInTimeStamp, eventData.eventOutTimeStamp);
            }
        }

        void dataSave(string eventName, EventParam[] eventParams, double timeStampIn, double timeStampOut)
        {
            VRsqrUtil.Debug.Log(LogLevel.Debug, ">>>>>>>>>>>>>> dataSave: eventName = " + eventName);
            VRsqrUtil.Debug.Log(LogLevel.Debug, ">>>>>>>>>>>>>> dataSave: eventParams.Length = " + eventParams.Length);

            if (gotEventData == false)
            {
                gotEventData = true;

                for (int i = 0; i < eventParams.Length; i++)
                {
                    string fieldName = "["; // + eventName + "-";
                    if (eventParams[i].NameString != "")
                    {
                        fieldName += eventParams[i].NameString;
                    }
                    fieldName += "]";
                    DataFieldNames.Add(fieldName);
                    numFields++;
                }
            }

            dataLines.Add(new List<string>(numFields));
            VRsqrUtil.Debug.Log(LogLevel.Debug, "============= dataSave: numLines = " + numLines + " , numFields = " + numFields);
            VRsqrUtil.Debug.Log(LogLevel.Debug, "============= dataSave: dataLines[0].Count = " + dataLines[0].Count + " , dataLines[0].Capacity = " + dataLines[0].Capacity);

            dataLines[numLines].Add(eventName);
            VRsqrUtil.Debug.Log(LogLevel.Debug, "============= dataSave: dataLines[numLines][0] = " + dataLines[numLines][0]);
            dataLines[numLines].Add(timeStampOut.ToString());
            VRsqrUtil.Debug.Log(LogLevel.Debug, "============= dataSave: dataLines[numLines][1] = " + dataLines[numLines][1]);
            dataLines[numLines].Add(timeStampIn.ToString());
            VRsqrUtil.Debug.Log(LogLevel.Debug, "============= dataSave: dataLines[numLines][2] = " + dataLines[numLines][2]);

            int fieldInd = 0;
            while (fieldInd < eventParams.Length)
            {
                VRsqrUtil.Debug.Log(LogLevel.Debug, "============= dataSave: numLines = " + numLines + " , fieldInd = " + fieldInd + "  , eventParams[fieldInd].ValString = " + eventParams[fieldInd].ValString);
                dataLines[numLines].Add(eventParams[fieldInd].ValString);
                fieldInd++;
            }

            numLines++;
        }

        void flushDataToFile()
        {
            File.AppendAllText(fullFileName, initTimeHeader + Environment.NewLine);

            for (int i = 0; i < DataFieldNames.Count; i++)
            {
                File.AppendAllText(fullFileName, DataFieldNames[i] + "\t");
            }
            File.AppendAllText(fullFileName, Environment.NewLine);

            foreach (var line in dataLines)
            {
                string lineString = string.Join("\t", line.Select(i => i.ToString()).ToArray());
                File.AppendAllText(fullFileName, lineString + Environment.NewLine);
            }
        }

        private void OnDestroy()
        {
            flushDataToFile();
        }

        void toggleRecording(int EventContext)
        {
            saveData = !saveData;
        }
    }
}
