using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;

[Serializable]
public static class ULinkVarsManager //: MonoBehaviour
{
    //static ULinkVarsManager() { }
    //private ULinkVarsManager() { }
    //public static ULinkVarsManager Instance { get; } = new ULinkVarsManager();

    private static bool scannedVars = false;

    //public Dictionary<string, List<string>> ObjectToScriptsDict = new Dictionary<string, List<string>>();
    //public Dictionary<string, List<string>> ScriptToVarsDict = new Dictionary<string, List<string>>();
    public static List<string> ULinkInputVars = new List<string>();
    public static List<string> ULinkOutputVars = new List<string>();

    public class varMethodInfo
    {
        public MethodInfo methodInfo;
        public ULinkVarBase varObject;
    }
    public static Dictionary<string, List<varMethodInfo>> scriptVarsBroadcastDict = new Dictionary<string, List<varMethodInfo>>();
    public static Dictionary<string, List<varMethodInfo>> scriptVarsReleaseDict = new Dictionary<string, List<varMethodInfo>>();
    public static Dictionary<string, List<varMethodInfo>> scriptVarsLogDict = new Dictionary<string, List<varMethodInfo>>();

    //[StringInList(typeof(ULinkVarLists), "AllSceneNames")] public string SceneName;

    //[Serializable]
    //public class dropDownList
    //{
    //    [StringInList(typeof(ULinkVarsManager), "AllSceneNames")] public string SceneName;
    //}

    //#if UNITY_EDITOR
    //    public static string[] AllSceneNames()
    //    {
    //        var temp = new List<string>(); // { "yyy", "aaa", "bbb", "ccc" };

    //        var varObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

    //        foreach (var o in varObjs)
    //            temp.Add(o.ToString());

    //        return temp.ToArray();
    //    }
    //#endif

    public static void broadcastAllVars(string fullScriptName)
    {
        VRsqrUtil.Debug.Log(LogLevel.DebugX, "broadcastAllVars: fullScriptName = " + fullScriptName);
        List <varMethodInfo> varsBroadcastInfo = null;
        if(scriptVarsBroadcastDict.TryGetValue(fullScriptName, out varsBroadcastInfo))
        {
            foreach (var broadcastInfo in varsBroadcastInfo)
            {
                if (broadcastInfo.varObject != null)
                {
                    VRsqrUtil.Debug.Log(LogLevel.DebugX, "broadcastAllVars: calling broadcastInfo - fullScriptName = " + fullScriptName);
                    broadcastInfo.methodInfo.Invoke(broadcastInfo.varObject, null);
                }
            }
        }
    }

    public static void releaseAllVars(string fullScriptName)
    {
        VRsqrUtil.Debug.Log(LogLevel.DebugX, "releaseAllVars: fullScriptName = " + fullScriptName);
        List<varMethodInfo> varsReleaseInfo = null;
        if (scriptVarsReleaseDict.TryGetValue(fullScriptName, out varsReleaseInfo))
        {
            foreach (var releaseInfo in varsReleaseInfo)
            {
                if (releaseInfo.varObject != null)
                {
                    VRsqrUtil.Debug.Log(LogLevel.DebugX, "releaseAllVars: calling releaseInfo - fullScriptName = " + fullScriptName);
                    releaseInfo.methodInfo.Invoke(releaseInfo.varObject, null);
                }
            }
        }
    }

    public static void LogAllVars(string fullScriptName)
    {
        VRsqrUtil.Debug.Log(LogLevel.DebugX, "LogAllVars: fullScriptName = " + fullScriptName);
        List<varMethodInfo> varsLogInfo = null;
        if (scriptVarsLogDict.TryGetValue(fullScriptName, out varsLogInfo))
        {
            foreach (var logInfo in varsLogInfo)
            {
                if (logInfo.varObject != null)
                {
                    VRsqrUtil.Debug.Log(LogLevel.DebugX, "LogAllVars: calling logInfo - fullScriptName = " + fullScriptName);
                    logInfo.methodInfo.Invoke(logInfo.varObject, null);
                }
            }
        }
    }

    public static string[] AllULinkInputVars()
    {
        scanVars();
        return ULinkInputVars.ToArray();
    }
    public static string[] AllULinkOutputVars()
    {
        scanVars();
        return ULinkOutputVars.ToArray();
    }

    //void Awake()
    //{
    //    scanVars();
    //}

    public static void RescanVars()
    {
        if (scannedVars)
        {
            scannedVars = false;
            scanVars();
        }
    }

    public static void scanVars()
    {
        if (scannedVars)
            return;

        scannedVars = true;

        scriptVarsBroadcastDict = new Dictionary<string, List<varMethodInfo>>();
        scriptVarsReleaseDict = new Dictionary<string, List<varMethodInfo>>();
        scriptVarsLogDict = new Dictionary<string, List<varMethodInfo>>();

        ULinkInputVars = new List<string>();
        ULinkInputVars.Add("---");
        ULinkOutputVars = new List<string>();
        ULinkOutputVars.Add("---");

        // Get the top-most hierarchy objects to start traversing and searching for all the relevant scene objects
        var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject ro in rootObjs)
        {
            var transforms = ro.GetComponentsInChildren<Transform>(true); // get all child objects (including self, and including inactive objects)

            foreach (Transform t in transforms)
            {
                GameObject go = t.gameObject;
                //var scripts = go.GetComponentsInChildren<ULinkBase>(true);
                var scripts = go.GetComponents<ULinkBase>();
                foreach (ULinkBase s in scripts)
                {
                    if (s != null)
                    {
                        string scriptName = s.ToString();
                        char[] delimiterChars = { '(', ')' };
                        string[] parts = scriptName.Split(delimiterChars);
                        s.scriptName = parts[1];
                        s.objectName = go.name;
                        s.fullScriptName = s.objectName + "." + s.scriptName;

                        VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: s.fullScriptName = " + s.fullScriptName);

                        List<varMethodInfo> scriptVarsBroadcasts = new List<varMethodInfo>();
                        List<varMethodInfo> scriptVarsRelease = new List<varMethodInfo>();
                        List<varMethodInfo> scriptVarsLog = new List<varMethodInfo>();

                        /*
                        // handle erroneous declaration of ULinkVars as non-public (to avoid later less-understandable exceptions)
                        var nonPublicVars = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                        foreach (var sv in nonPublicVars)
                        {
                            if (sv.FieldType.Name.Contains("ULink"))  //sv.FieldType == typeof(OutVar) || sv.FieldType == typeof(InVar))
                            {
                                VRsqrUtil.Debug.LogError("Illegal declaration of UlinkVar as non-public in: " + s.fullScriptName);
                            }
                        }
                        */

                        var scriptVars = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: scriptVars.Length = " + scriptVars.Length);
                        foreach (var sv in scriptVars)
                        {
                            VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: sv = " + sv);
                            if (sv.FieldType.Name.Contains("ULink"))  //sv.FieldType == typeof(OutVar) || sv.FieldType == typeof(InVar))
                            {
                                var fieldObj = sv.GetValue(s);
                                VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: fieldObj = " + fieldObj);

                                MethodInfo broadcastMethod = sv.FieldType.GetMethod("broadcast");
                                if (broadcastMethod != null)
                                {
                                    scriptVarsBroadcasts.Add(new varMethodInfo { methodInfo = broadcastMethod, varObject = (ULinkVarBase)fieldObj });
                                }

                                MethodInfo releaseMethod = sv.FieldType.GetMethod("releaseVar");
                                if (releaseMethod != null)
                                {
                                    scriptVarsRelease.Add(new varMethodInfo { methodInfo = releaseMethod, varObject = (ULinkVarBase)fieldObj });
                                }

                                MethodInfo logMethod = sv.FieldType.GetMethod("LogVar");
                                if (logMethod != null)
                                {
                                    scriptVarsLog.Add(new varMethodInfo { methodInfo = logMethod, varObject = (ULinkVarBase)fieldObj });
                                }

                                // get handle to the registration method of the variable class object
                                MethodInfo registerMethod = sv.FieldType.GetMethod("RegisterVar");
                                if (registerMethod != null)
                                {
                                    ((ULinkVarBase)fieldObj).gameObj = go;
                                    ((ULinkVarBase)fieldObj).script = s;
                                    ((ULinkVarBase)fieldObj).objectName = go.name;
                                    ((ULinkVarBase)fieldObj).scriptName = s.scriptName;
                                    ((ULinkVarBase)fieldObj).varName = sv.Name;
                                    //((ULinkVarBase)fieldObj).fullVarName = fullVarName;

                                    // call the registration function to add the variable to the system
                                    registerMethod.Invoke(fieldObj, null);

                                    // Only now we have a valid ((ULinkVarBase)fieldObj).varType & ((ULinkVarBase)fieldObj).fullVarName, which are set in the registration method
                                    string fullVarName = ((ULinkVarBase)fieldObj).fullVarName;
                                    VRsqrUtil.Debug.Log(LogLevel.Debug, "scanVars: fullVarName = " + fullVarName);
                                    VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: obj: " + go.name + " | script: " + s + " | field: " + sv.FieldType + " | name: " + sv.Name + " | type: " + ((ULinkVarBase)fieldObj).varType);

                                    string varName = ((ULinkVarBase)fieldObj).varName;

                                    ULinkVarBase.ULinkDirection uLinkDir = ((ULinkVarBase)fieldObj).ulinkDir;
                                    ((ULinkVarBase)fieldObj).ulinkDirection = uLinkDir.ToString();
                                    //string uLinkDir = ((ULinkVarBase)fieldObj).ulinkDirection;
                                    //if (uLinkDir != ULinkVarBase.ULinkDirection.Disabled)
                                    {
                                        if (uLinkDir == ULinkVarBase.ULinkDirection.Input) // ||
                                                                                           //uLinkDir == ULinkVarBase.ULinkDirection.InputAndOutput)
                                        {
                                            if (ULinkInputVars.Contains(fullVarName) == false)
                                            {
                                                ULinkInputVars.Add(fullVarName);
                                            }
                                        }
                                        if (uLinkDir == ULinkVarBase.ULinkDirection.Output)// ||
                                                                                           //uLinkDir == ULinkVarBase.ULinkDirection.InputAndOutput)
                                        {
                                            if (ULinkOutputVars.Contains(fullVarName) == false)
                                            {
                                                ULinkOutputVars.Add(fullVarName);
                                            }
                                        }
                                    }

                                    MethodInfo varHandlerMethod = s.GetType().GetMethod(varName + "Handler");
                                    if (varHandlerMethod != null)
                                    {
                                        Action varHandlerAction = (Action)varHandlerMethod.CreateDelegate(typeof(Action), s);
                                        ULinkConnect.Instance.registerHandler(fullVarName, varHandlerAction);
                                        VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: registered handler of " + fullVarName);
                                    }

                                    /*
                                    var varFields = sv.FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                                    foreach (var vf in varFields)
                                    {
                                        VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: vf.Name = " + vf.Name);
                                        if (vf.Name == "fullVarName")
                                        {
                                            vf.SetValue(fieldObj, fullVarName);
                                            VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: vf.GetValue(sv) = " + vf.GetValue(fieldObj));

                                            VRsqrUtil.Debug.Log(LogLevel.DebugX, "scanVars: Invoking registerMethod on " + fullVarName);
                                            registerMethod.Invoke(fieldObj, null);

                                            //ULinkConnect.Instance.registerULinkVar(fullVarName)
                                        }
                                    }
                                    */
                                }
                            }
                        }

                        scriptVarsBroadcastDict.Add(s.fullScriptName, scriptVarsBroadcasts);
                        scriptVarsReleaseDict.Add(s.fullScriptName, scriptVarsRelease);
                        scriptVarsLogDict.Add(s.fullScriptName, scriptVarsLog);
                    }
                }
            }
        }
    }



    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
