using System;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using System.Collections.Generic;
using VRsqrUtil;
using UnityEditor;
using Assets.Scripts.ULinkUtils;

namespace VRsqrCore
{
    [Serializable]
    public abstract class ULinkVarBase //: MonoBehaviour
    {
        [HideInInspector]
        public GameObject gameObj;
        [HideInInspector]
        public ULinkBase script;
        [HideInInspector]
        public string objectName;
        [HideInInspector]
        public string scriptName;
        [HideInInspector]
        public string varName;

        [ReadOnly, SerializeField]
        public string varType;
        [ReadOnly, SerializeField]
        public string fullVarName;

        [Serializable]
        public enum ULinkDirection
        {
            Input,
            Output,
            //InputAndOutput,
            Disabled
        }

        [HideInInspector]
        public abstract ULinkDirection ulinkDir { get; }

        [ReadOnly, SerializeField]
        public string ulinkDirection;

        [SerializeField]
        [HideInInspector]
        public string strVal;
    }


    [Serializable]
    public abstract class ULinkVar <T> : ULinkVarBase //where T : new()
    {
        [SerializeField]
        protected bool recordUlink = false;

        //[SerializeField]
        //protected bool replayUlink = false;

        [SerializeField]
        protected T _val;

        protected LogRecorder logRecorder;

        public object getVal()
        {
            if (_val == null)
            {
                Type theType = typeof(T);

                ConstructorInfo constructor = theType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

                //_val = new T(); //default(T);
                _val = constructor != null ? (T)constructor.Invoke(null) : default(T);

                VRsqrUtil.Debug.Log(LogLevel.Warning, "getVal: _val == null ==> setting _val = new(T) = " + _val);
            }
            return ToObject(_val);  
        }

        public virtual string GetStringVal()
        {
            return _val.ToString();
        }

        public virtual T FromObject(object obj)
        {
            try
            {
                return (T)obj;
            }
            catch (System.InvalidCastException ex)
            {
                VRsqrUtil.Debug.Log(LogLevel.Error, "Invalid cast");
                return default(T);
            }
        }

        public virtual object ToObject(T val)
        {
            return (object)val;
        }
    }

 
    [Serializable]
    public class ULinkVarIn<T> : ULinkVar<T> //where T: new()
    {
        [SerializeField]
        public T val
        {
            get { return _val; }
        }

        public void setVal(object objVal)
        {
            _val = FromObject(objVal);
            if (recordUlink)
                LogVar();
                //logRecorder.RecordLog("IN: " + varName + " <<< " + GetStringVal());
        }

        [HideInInspector]
        public override ULinkVarBase.ULinkDirection ulinkDir { get { return ULinkVarBase.ULinkDirection.Input; } }

        public void RegisterHandler(Action handler)
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "RegisterHandler: fullVarName = " + fullVarName);
            ULinkConnect.Instance.registerHandler(this.fullVarName, handler);
        }

        public void RegisterVar()
        {
            varType = typeof(T).ToString(); // getVal().GetType().ToString(); //_val.GetType().ToString();

            this.fullVarName = objectName + "." + scriptName + "." + varName + " (" + varType + ")";
            VRsqrUtil.Debug.Log(LogLevel.Debug, "RegisterVar: fullVarName = " + this.fullVarName);

            ULinkConnect.Instance.registerULinkVar(this.gameObj, this.script, this.objectName, this.scriptName, this.varName, this.fullVarName, this.varType, this.setVal, this.getVal);

            if (recordUlink && logRecorder == null)
            {
                logRecorder = new LogRecorder();
            }
        }

        public void releaseVar()
        {
            VRsqrUtil.Debug.Log(LogLevel.Debug, "releaseVar");
            if (recordUlink)
                logRecorder.FlushToDisk(fullVarName);
        }

        public void LogVar()
        {
            logRecorder?.RecordLog("IN" + "\t" + varName + "\t" + GetStringVal());
        }
    }

    [Serializable]
    public class ULinkVarOut<T> : ULinkVar<T> //where T : new()
    {
        [SerializeField]
        public T val
        {
            get { return FromObject(getVal()); }//_val; }
            set { setVal(ToObject(value)); }
        }

        [HideInInspector]
        public override ULinkVarBase.ULinkDirection ulinkDir { get { return ULinkVarBase.ULinkDirection.Output; } }

        public void setVal(object objVal)
        {
            _val = FromObject(objVal);
            if (recordUlink)
                LogVar();
                //logRecorder.RecordLog("OUT: " + varName + " >>> " + GetStringVal());

            // ==========================================================================
            // Most important call - propagate the new value to all connected variables
            broadcast();
            // ==========================================================================
        }
        public void broadcast()
        {
            VRsqrUtil.Debug.Log(LogLevel.Debug, "broadcast");
            strVal = val.ToString();
            ULinkConnect.Instance.broadcastOutVar(fullVarName, val);
        }

        public void RegisterVar()
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "RegisterVar: _val = " + getVal()); //_val);
            varType = varType = typeof(T).ToString(); //getVal().GetType().ToString(); //_val.GetType().ToString();

            this.fullVarName = objectName + "." + scriptName + "." + varName + " (" + varType + ")";
            VRsqrUtil.Debug.Log(LogLevel.Debug, "RegisterVar: fullVarName = " + this.fullVarName);

            ULinkConnect.Instance.registerULinkVar(this.gameObj, this.script, this.objectName, this.scriptName, this.varName, this.fullVarName, this.varType, this.setVal, this.getVal);

            if (recordUlink && logRecorder == null)
            {
                logRecorder = new LogRecorder();
            }
        }

        public void releaseVar()
        {
            VRsqrUtil.Debug.Log(LogLevel.Debug, "releaseVar");
            if (recordUlink)
                logRecorder.FlushToDisk(fullVarName);
        }

        public void LogVar()
        {
            logRecorder?.RecordLog("OUT" + "\t" + varName + "\t" + GetStringVal());
        }
    }


    [Serializable]
    public class ULinkInt_In : ULinkVarIn<int> { }
    [Serializable]
    public class ULinkInt_Out : ULinkVarOut<int> { }


    [Serializable]
    public class ULinkBool_In : ULinkVarIn<bool> 
    {
        public override bool FromObject(object obj)
        {
            try
            {
                return (bool)obj;
            }
            catch (System.InvalidCastException ex)
            {
                bool boolRes;
                int intRes;
                try
                {
                    intRes = (int)obj;
                    boolRes = (intRes == 0 ? false : true);
                    VRsqrUtil.Debug.Log(LogLevel.Error, "Casting non-boolean value: " + intRes + " to boolean: " + boolRes);
                }
                catch
                {
                    boolRes = (obj == null ? false : true);
                    VRsqrUtil.Debug.Log(LogLevel.Warning, "Casting non-boolean value: " + obj + " to boolean: " + boolRes);
                }
                //return default(T);
                return boolRes;
            }
        }
    }
    [Serializable]
    public class ULinkBool_Out : ULinkVarOut<bool> 
    {
        public override bool FromObject(object obj)
        {
            try
            {
                return (bool)obj;
            }
            catch (System.InvalidCastException ex)
            {
                bool boolRes;
                int intRes;
                try
                {
                    intRes = (int)obj;
                    boolRes = (intRes == 0 ? false : true);
                    VRsqrUtil.Debug.Log(LogLevel.Warning, "Casting non-boolean value: " + intRes + " to boolean: " + boolRes);
                }
                catch
                {
                    boolRes = (obj == null ? false : true);
                    VRsqrUtil.Debug.Log(LogLevel.Error, "Casting non-boolean value: " + obj + " to boolean: " + boolRes);
                }
                //return default(T);
                return boolRes;
            }
        }
    }

    [Serializable]
    public class ULinkFloat_In : ULinkVarIn<float> { }
    [Serializable]
    public class ULinkFloat_Out : ULinkVarOut<float> { }


    [Serializable]
    public class ULinkDouble_In : ULinkVarIn<double> { }
    [Serializable]
    public class ULinkDouble_Out : ULinkVarOut<double> { }

    [Serializable]
    public class ULinkString_In : ULinkVarIn<string> { }
    [Serializable]
    public class ULinkString_Out : ULinkVarOut<string> { }

    [Serializable]
    public class ULinkVector3_In : ULinkVarIn<Vector3> { }
    [Serializable]
    public class ULinkVector3_Out : ULinkVarOut<Vector3> { }

    [Serializable]
    public class ULinkIntList_In : ULinkVarIn<List<int>> { }
    [Serializable]
    public class ULinkIntList_Out : ULinkVarOut<List<int>> { }

    [Serializable]
    public class ULinkEvent_In : ULinkVarIn<Event> { }
    [Serializable]
    public class ULinkEvent_Out : ULinkVarOut<Event> { }


    public sealed class ULinkConnect
    {
        private static readonly ULinkConnect instance = new ULinkConnect();
        static ULinkConnect() { }
        private ULinkConnect() { }
        public static ULinkConnect Instance
        {
            get
            {
                return instance;
            }
        }

        public class ULinkVarData
        {
            public GameObject gameObj;
            public ULinkBase script;
            public string objectName;
            public string scriptName;
            public string varName;
            public string varType;

            public string fullVarName;

            public Action<object> setFunc;
            public Func<object> getFunc;

            public List<string> targetInVars;

            //public Action<object> eventHandler;
            public Action eventHandler;
        };

        private Dictionary<string, ULinkVarData> ULinkVarDataDict = new Dictionary<string, ULinkVarData>();

        public void registerULinkVar(GameObject gameObj, ULinkBase script, string obj, string scriptName, string var, string fullName, string type, Action<object> setter, Func<object> getter)
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "registerULinkVar: varName = " + fullName);
            ULinkVarData varData = new ULinkVarData { gameObj = gameObj, script = script, objectName = obj, scriptName = scriptName, varName = var, varType = type, fullVarName = fullName, setFunc = setter, getFunc = getter, targetInVars = new List<string>(), eventHandler = null };
            if (ULinkVarDataDict.ContainsKey(fullName) == false)
            {
                ULinkVarDataDict.Add(fullName, varData);
            }
            else
            {
                ULinkVarDataDict[fullName] = varData;
            }
        }

        public void registerHandler(string fullName, Action handler)
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "ULinkConnect::registerHandler: fullName = " + fullName);
            ULinkVarData varData = null;
            if (fullName != null && handler != null && ULinkVarDataDict.TryGetValue(fullName, out varData))
            {
                VRsqrUtil.Debug.Log(LogLevel.DebugX, "ULinkConnect::registerHandler: setting eventHandler");
                varData.eventHandler = handler;
            }
            else
            {
                VRsqrUtil.Debug.Log(LogLevel.Warning, "ULinkConnect::registerHandler: Registration failed! fullName = " + fullName);
            }
        }

        public void setULinkInValue(string fullName, object newVal)
        {
            ULinkVarData varData = null;
            if (fullName != null && ULinkVarDataDict.TryGetValue(fullName, out varData))
            {
                VRsqrUtil.Debug.Log(LogLevel.DebugX, "setULinkInValue:  " + newVal + " --> " + varData); 
                if (varData.gameObj != null && varData.script != null) // the gameObject containing the var's script, and the script componnent, are valid and active
                {
                    VRsqrUtil.Debug.Log(LogLevel.DebugX, "setULinkInValue: varData.gameObj = " + varData.gameObj + "  |  varData.script = " + varData.script);
                    varData.setFunc(newVal);
                    if (varData.gameObj.activeInHierarchy)
                    {
                        VRsqrUtil.Debug.Log(LogLevel.DebugX, "setULinkInValue: Invoking event-handler of var " + fullName + " in game-object " + varData.gameObj);
                        varData.eventHandler?.Invoke();
                    }
                    else
                    {
                        VRsqrUtil.Debug.Log(LogLevel.Warning, "setULinkInValue: Game-object " + varData.gameObj + " of var " + fullName + " is inactive ==> Not invoking eventHandler");
                    }
                }
            }
        }

        public bool getULinkOutValue(string fullName, out object currVal)
        {
            currVal = default(object);
            ULinkVarData varData = null;
            if (fullName != null && ULinkVarDataDict.TryGetValue(fullName, out varData))
            {
                currVal = varData.getFunc();
                return true;
            }
            return false;
        }

        public void connectULinkVars(string outVarName, string inVarName)
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "connectULinkVars: " + outVarName + " ==> " + inVarName);

            ULinkVarData varData = null;
            if (ULinkVarDataDict.TryGetValue(outVarName, out varData))
            {
                VRsqrUtil.Debug.Log(LogLevel.DebugX, "connectULinkVars: varData.targetInVars.Count = " + varData.targetInVars.Count);

                VRsqrUtil.Debug.Log(LogLevel.DebugX, "connectULinkVars: Add var - " + inVarName);
                varData.targetInVars.Add(inVarName);
            }
            else
            {
                VRsqrUtil.Debug.Log(LogLevel.Error, "connectULinkVars: failed ULinkVarDataDict.TryGetValue(" + outVarName);
            }
        }

        public void disconnectULinkVars(string outVarName, string inVarName)
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "disconnectULinkVars: " + outVarName + " ==> " + inVarName);

            ULinkVarData varData = null;
            if (ULinkVarDataDict.TryGetValue(outVarName, out varData))
            {
                VRsqrUtil.Debug.Log(LogLevel.DebugX, "disconnectULinkVars: varData.targetInVars.Count = " + varData.targetInVars.Count);

                bool removed = varData.targetInVars.Remove(inVarName);
                VRsqrUtil.Debug.Log(LogLevel.DebugX, "disconnectULinkVars: Remove var - " + inVarName + "  removed = " + removed);
            }
            else
            {
                VRsqrUtil.Debug.Log(LogLevel.Error, "disconnectULinkVars: failed ULinkVarDataDict.TryGetValue(" + outVarName);
            }
        }

        public void broadcastOutVar(string outVarName)
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "broadcastOutVar: " + outVarName);

            object outVarVal = default(object);
            if (outVarName != null && getULinkOutValue(outVarName, out outVarVal))
            {
                ULinkVarData varData = null;
                if (outVarName != null && ULinkVarDataDict.TryGetValue(outVarName, out varData))
                {
                    foreach (var inVarName in varData.targetInVars)
                    {
                        setULinkInValue(inVarName, outVarVal);
                    }
                }
            }
        }

        public void broadcastOutVar(string fullVarName, object outVarVal)
        {
            VRsqrUtil.Debug.Log(LogLevel.DebugX, "broadcastOutVar: " + fullVarName + " --> " + outVarVal);

            ULinkVarData varData = null;
            if (fullVarName != null && ULinkVarDataDict.TryGetValue(fullVarName, out varData))
            {
                VRsqrUtil.Debug.Log(LogLevel.DebugX, "broadcastOutVar: varData.targetInVars.Count = " + varData.targetInVars.Count);
                foreach (var inVarName in varData.targetInVars)
                {
                    VRsqrUtil.Debug.Log(LogLevel.DebugX, "broadcastOutVar: " + outVarVal + " --> " + inVarName);
                    setULinkInValue(inVarName, outVarVal);
                }
            }
        }
    }
}
