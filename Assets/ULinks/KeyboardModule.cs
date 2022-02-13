using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;

public class KeyboardModule : ULinkBase
{
    [Serializable]
    public class KeyEvents {
        public KeyCode keyCode = KeyCode.None;
        public EventType eventType = EventType.Ignore;
    }
    [SerializeField]
    public KeyEvents[] keyEvents;

    [HideInInspector]
    public ULinkEvent_Out keyboardEvent;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    protected override void OnValidate()
    {
        // avoid broadcasting the event whenever something changes in the inspector (to avoid false keyboard/mouse events)
        //base.OnValidate();
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.type != EventType.Repaint && e.type != EventType.Layout) // most common GUI events which we ignore
        {
            foreach (var keyEvent in keyEvents)
            {
                if(e.type == keyEvent.eventType)
                { 
                    if (e.keyCode == keyEvent.keyCode)
                    {
                        DebugLog("OnGUI: e.type = " + e.type + "  |  e.keyCode = " + e.keyCode);
                        RecordLog("Got user response: e.type = " + e.type + "  |  e.keyCode = " + e.keyCode);
                        keyboardEvent.val = e; // trigger all the modules subscribed to this event
                    }
                }
            }
        }
    }
}
