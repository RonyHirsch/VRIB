using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;

namespace Assets.Scripts.ULinkUtils
{
    public class LogRecorder //:MonoBehaviour
    {
        private string dataPath;

        // subject-related:
        private string subjectCode;
        private int subjectAge;
        private string subjectDomHand;
        private string subjectGender;

        public string initTimeHeader;

        private string fullFileName;
        private string fullSubjectsPath;
        private List<string> dataLines;
        private int numLines = 0;
        private bool isSubjectInfo = false;

        public LogRecorder()
        {
            dataLines = new List<string>();
            
            //int timeMs = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond; //(float)ts.TotalMilliseconds;
            initTimeHeader = "startTime" + "\t" + GlobalSettings.InitDateTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\t" + GlobalSettings.InitDateTimeMS();

        }

        public void RecordLog(string logMessage)
        {
            VRsqrUtil.Debug.Log(LogLevel.Debug, "RecordLog: logMessage = " + logMessage);
            dataLines.Add(GlobalSettings.TimeStamp() + "\t" + logMessage + Environment.NewLine); // TimeStamp is the time that has passed between the trial's start time and the moment of record
        }



        public void FlushToDisk(string recordedEntityName)
        {
            dataPath = GlobalSettings.Instance.dataPath.val; //GlobalSettings.DataPath;
            VRsqrUtil.Debug.Log(LogLevel.Debug, "LogRecorder: dataPath = " + dataPath);
            subjectCode = GlobalSettings.Instance.currentSubCode;

            //fullSubjectsPath = @"C:\Users\ARpalus\Documents\temp\789"; // System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + dataPath + @"\" + subjectCode;
            //fullSubjectsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/" + dataPath + "/" + subjectCode; // @"\" + subjectCode;
            fullSubjectsPath = dataPath + "/" + subjectCode; // @"\" + subjectCode;
            try
            {
                System.IO.Directory.CreateDirectory(fullSubjectsPath);
            }
            catch (System.Exception ex)
            {
                VRsqrUtil.Debug.Log(LogLevel.Debug, "LogRecorder: exception! ex = " + ex);
            }

            fullFileName = fullSubjectsPath + "/" + recordedEntityName + "_" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".txt"; //@"\" + recordedEntityName + "_" + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".txt";
            VRsqrUtil.Debug.Log(LogLevel.Debug, "FlushToDisk: fullFileName = " + fullFileName);

            File.AppendAllText(fullFileName, initTimeHeader + Environment.NewLine);

            /*foreach (var line in dataLines)
            {
                File.AppendAllText(fullFileName, line);
            }*/

            //System.IO.File.WriteAllLines(fullFileName, dataLines.Select(x => string.Join("\n", x)));

            System.IO.File.AppendAllLines(fullFileName, dataLines);

            string endTimeHeader = "endTime" + "\t" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\t" + GlobalSettings.DateTimeToMS(DateTime.Now);
            System.IO.File.AppendAllText(fullFileName, endTimeHeader + Environment.NewLine);

            dataLines.Clear();
            
        }
    }
}
