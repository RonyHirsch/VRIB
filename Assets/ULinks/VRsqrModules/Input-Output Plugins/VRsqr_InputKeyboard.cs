using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using VRsqrCore;

public class VRsqr_InputKeyboard : MonoBehaviour {

    [System.Serializable]
    public struct KeyEventMapping
    {
        //public string Key;
        public KeyCode keyCode;
        public string keyDownEventName;
        public string keyUpEventName;
        public string keyHelpText;
    };

    public List<KeyEventMapping> keyEventMap;

    public Dictionary<KeyCode, List<int>> keyEventIndDict = new Dictionary<KeyCode, List<int>>();

    public string updateHelpTextEventOut = "";

    private static VRsqr_InputKeyboard instance = null;

    void Awake()
    {
        //Check if instance already exists
        if (!instance)
            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)
        {
            //Debug.LogError("To handle keyboard events there must be exactly one active VRsqr_InputKeyboard script on a GameObject in your scene.");
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }

        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);

        // initialize the LUT with the keys pre-set in the inspector
        for (int ind = 0; ind < instance.keyEventMap.Count; ind++)
        {
            KeyEventMapping keyEvent = instance.keyEventMap[ind];
            AddKeyToEventIndDict(keyEvent.keyCode, ind);
        }
    }

    // Use this for initialization
    void Start () {
        //updateDictionary();
        //AddKeyEvent(KeyCode.G, "Start3Dmotion");

        //for (int ind = 0; ind < instance.keyEventMap.Count; ind++)
        //{
        //    KeyEventMapping keyEvent = instance.keyEventMap[ind];
        //    AddKeyToEventIndDict(keyEvent.keyCode, ind);
        //}
    }

    // Update is called once per frame
    void Update ()
    {

    }

    void OnGUI()
    {
        if (!instance)
            return;

        Event e = Event.current;
        //if (e.isKey)
        if (e.type == EventType.KeyDown || e.type == EventType.KeyUp)
        {
            ////Debug.Log("Detected Key event - code: " + e.keyCode + "    EventType = " + e.type);

            List<int> keyEventList;
            if (instance.keyEventIndDict.TryGetValue(e.keyCode, out keyEventList))
            {
                foreach (int eventInd in keyEventList)
                {
                    KeyEventMapping keyEvent = instance.keyEventMap[eventInd];
                    if (e.type == EventType.KeyDown && keyEvent.keyDownEventName != null)
                    {
                        ////Debug.Log("Handling Key event - code: " + e.keyCode + "    EventType = " + e.type);
                        VRsqr_EventsManager.TriggerEvent(keyEvent.keyDownEventName);
                    }
                    if (e.type == EventType.KeyUp && keyEvent.keyUpEventName != null)
                    {
                        ////Debug.Log("Handling Key event - code: " + e.keyCode + "    EventType = " + e.type);
                        VRsqr_EventsManager.TriggerEvent(keyEvent.keyUpEventName);
                    }
                }
            }
        }
    }

    public static void AddKeyEventListener(KeyCode keyCode, string keyDownEventName, UnityAction<int> keyDownlistener, string keyUpEventName = null, UnityAction<int> keyUplistener = null, string keyHelpText = null)
    {
        AddKeyEvent(keyCode, keyDownEventName, keyUpEventName, keyHelpText);
        VRsqr_EventsManager.StartListening(keyDownEventName, keyDownlistener);
        if (keyUpEventName != null && keyUplistener != null)
        {
            VRsqr_EventsManager.StartListening(keyUpEventName, keyUplistener);
        }
    }

    public static void AddKeyEvent(KeyCode keyCode, string keyDownEventName, string keyUpEventName = null, string keyHelpText = null)
    {
        if (!instance)
        {
            //Debug.LogError("To handle keyboard events there must be exactly one active VRsqr_InputKeyboard script on a GameObject in your scene.");
            return;
        }

        instance.keyEventMap.Add(new KeyEventMapping { keyCode = keyCode, keyDownEventName = keyDownEventName, keyUpEventName = keyUpEventName , keyHelpText = keyHelpText });
        AddKeyToEventIndDict(keyCode, instance.keyEventMap.Count - 1);

        UpdateKeyHelpText();
    }

    public static void AddKeyListener(KeyCode keyCode, UnityAction<int> keyDownListener, UnityAction<int> keyUpListener = null, string keyHelpText = null)
    {
        if (!instance)
            return;

        int numHandlers = instance.keyEventMap.Count;
        string keyDownEventName = "KeyDownHandler" + numHandlers + "_" + keyDownListener.Method.Name;
        string keyUpEventName = (keyUpListener == null ? null : "KeyUpHandler" + numHandlers + "_" + keyUpListener.Method.Name);
        AddKeyEventListener(keyCode, keyDownEventName, keyDownListener, keyUpEventName, keyUpListener, keyHelpText);
    }

    public static void AddKeyToEventIndDict(KeyCode keyCode, int eventInd)
    {
        if (!instance)
            return;

        List<int> keyEventList;
        if (!instance.keyEventIndDict.TryGetValue(keyCode, out keyEventList))
        {
            keyEventList = new List<int>();
        }
        keyEventList.Add(eventInd); // instance.keyEventMap.Count - 1);
        instance.keyEventIndDict[keyCode] = keyEventList;
    }

    public static void UpdateKeyHelpText()
    {
        if (!instance)
            return;

        string helpText = ""; // "Keyboard shortcuts: \n\n";
        //foreach (var keyEvent in instance.keyEventMap)
        //{
        //    helpText += keyEvent.keyCode + ":   " + keyEvent.keyHelpText + "\n";
        //}
        foreach(var keyEventInd in instance.keyEventIndDict)
        {
            List<int> keyEventInds = keyEventInd.Value;
            KeyCode keyCode = instance.keyEventMap[keyEventInds[0]].keyCode;
            helpText += keyCode + " :  ";
            foreach (var eventInd in keyEventInds)
            {
                helpText +=  "<" + instance.keyEventMap[eventInd].keyHelpText + ">  ";
            }
            helpText += "\n";
        }

        VRsqr_EventsManager.TriggerEvent(instance.updateHelpTextEventOut, helpText);
    }

    //void OnValidate()
    //{
    //    updateDictionary();
    //}

    //void updateDictionary()
    //{
    //    keyEventDict = new Dictionary<string, string>();
    //    foreach (KeyEventMapping entry in keyEventMap)
    //    {
    //        keyEventDict.Add(entry.Key, entry.EventName);
    //    }
    //}
}
