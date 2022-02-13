using Assets.Scripts.ULinkUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public abstract class ULinkBase : MonoBehaviour
{
    [ReadOnly, SerializeField]
    public string scriptName;
    [ReadOnly, SerializeField]
    public string objectName;
    [ReadOnly, SerializeField]
    public string fullScriptName;

    //==========================================
    // Experiment logging functionality
    public bool saveLog;
    public bool saveDebugLog;
    LogRecorder logRecorder;
    LogRecorder debugLogRecorder;

    //public bool recordULinks = false;
    //==========================================

    protected virtual void Awake()
    {
        VRsqrUtil.Debug.Log(LogLevel.Debug, "ULinkBase - OnEnable: fullScriptName = " + fullScriptName);
        if (saveLog)
        {
            logRecorder = new LogRecorder();
        }
        if (saveDebugLog)
        {
            debugLogRecorder = new LogRecorder();
        }
    }

    /*
    protected virtual void OnEnable()
    {
        VRsqrUtil.Debug.Log(LogLevel.Debug, "ULinkBase - OnEnable: fullScriptName = " + fullScriptName);
        if (saveLog)
        {
            logRecorder = new LogRecorder();
        }
        if (saveDebugLog)
        {
            debugLogRecorder = new LogRecorder();
        }
    }

    // Start is called before the first frame update
    void Start() // called after sceneLoaded
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    */

    public void RecordLog(string logLine)
    {
        logRecorder?.RecordLog(logLine);
    }

    public void DebugLog(LogLevel logLevel, string message)
    {
        VRsqrUtil.Debug.Log(logLevel, ">> " + fullScriptName + ": " + message);

        if (saveDebugLog)
        {
            debugLogRecorder?.RecordLog("[" + logLevel.ToString() + "]   " + message);
        }
    }

    public void DebugLog(string message)
    {
        DebugLog(LogLevel.Debug, message);
    }

    // Workaround for editor-mode changes - force calling the variables' setter functions whenever a value is changed in the inspector
    protected virtual void OnValidate()
    {
        VRsqrUtil.Debug.Log(LogLevel.Debug, "ULinkBase - OnValidate: call broadcastAllVars - objectName = " + objectName + " | fullScriptName = " + fullScriptName);
        ULinkVarsManager.broadcastAllVars(fullScriptName);
        ULinkVarsManager.LogAllVars(fullScriptName);
    }

    protected void FlushVarsLogs()
    {
        ULinkVarsManager.releaseAllVars(fullScriptName);
    }

    public void OnDestroy()
    {
        ULinkVarsManager.releaseAllVars(fullScriptName);

        if (saveLog)
        {
            logRecorder?.FlushToDisk(fullScriptName);
        }
        if (saveDebugLog)
        { 
            debugLogRecorder?.FlushToDisk("DebugLog_" + fullScriptName);
        }
    }
}
