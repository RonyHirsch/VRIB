#define VERBOSE
#define VERBOSE_EXTRA

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;

public class FlowSequence : MonoBehaviour {

    //public string startFlowEventIn;
    [Serializable]
    public class EventSequence
    {
        public string SequenceStepTitle;
        public string TriggerEventIn;
        public EventOut eventOut;
        public bool EnableStep = true;
    }
    public EventSequence[] eventSequence;

    //public string TriggerFirstIn;
    //public EventOut[] eventTriggers;
    int eventInd = 0;

    public int numIterations = 1;
    //public string[] iterIds;
    public GameObject dataPool;
    public bool iterWithReturn;
    
    [Serializable]
    public enum randomizationMethod
    {
        noRandomization,
        Uniform,
        CounterBalance
    };



	// Use this for initialization
	private void OnEnable () {
        if (eventSequence.Length > 0)
        {
            if (eventSequence.Length > 0 && 
                !string.IsNullOrEmpty(eventSequence[0].TriggerEventIn) &&
                eventSequence[0].EnableStep)
            {
                VRsqr_EventsManager.StartListening(VRsqr_EventsManager.StandardizeString(eventSequence[eventInd].TriggerEventIn), NextEventHandler);
            }
        }
    }

    private void Start()
    {
        if (eventSequence.Length > 0 &&
            string.IsNullOrEmpty(eventSequence[0].TriggerEventIn) &&
            eventSequence[0].EnableStep)
        {
            NextEvent();
        }
    }

    void NextEventHandler(int eventContext)
    {
        NextEvent();
    }

    void NextEvent()
    {
        // trigger the sequence of events, until the sequence requires waiting for an event to continue triggering it
        while (eventSequence.Length > eventInd)
        {
            if (eventSequence[eventInd].EnableStep)
            {
                eventSequence[eventInd].eventOut.trigger();
            }

            eventInd++;

            // Next event in sequence is pending an event to trigger it
            if (eventSequence.Length > eventInd &&
                !string.IsNullOrEmpty(eventSequence[eventInd].TriggerEventIn) &&
                eventSequence[eventInd].EnableStep)
            {
                VRsqr_EventsManager.StartListening(VRsqr_EventsManager.StandardizeString(eventSequence[eventInd].TriggerEventIn), NextEventHandler);
                break;
            }
        }
    }
}
