#define VERBOSE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;

public class VRsqr_EventsConnector : MonoBehaviour {

    //public GameObject sender;


    [Serializable]
    public class StringMapping
    {
        public string From;
        public string To;
    }

    [SerializeField]
    public StringMapping EventNameConnect;
    [SerializeField]
    public StringMapping[] ParamNameConnect;

    private List<EventParam> EventParamsTo = new List<EventParam>();

    private void Awake()
    {
        if (EventNameConnect.From != "")
        {
            VRsqr_EventsManager.StartListening(EventNameConnect.From, eventHandler);
            VRsqrUtil.Debug.Log(LogLevel.Debug, "VRsqr_EventsConnector:Awake - StartListening - EventNameConnect.From = " + EventNameConnect.From);
        }
    }

    public void eventHandler(int EventContext)
    {
        UnityEventData eventData = VRsqr_EventsManager.GetEventData(EventContext);

        foreach (StringMapping paramMapping in ParamNameConnect)
        {
            EventParam paramData = VRsqr_EventsManager.GetEventParam(eventData, paramMapping.From);
            if (paramData != null && !String.IsNullOrEmpty(paramMapping.To))
            {
                EventParam newParamData = paramData;
                newParamData.NameString = paramMapping.To;
                EventParamsTo.Add(newParamData);
                VRsqrUtil.Debug.Log(LogLevel.Debug, "VRsqr_EventsConnector:eventHandler - newParamData = " + newParamData);
            }
        }

        EventNameConnect.To = (!String.IsNullOrEmpty(EventNameConnect.To) ? EventNameConnect.To : EventNameConnect.From);

        VRsqr_EventsManager.TriggerDataEvent(EventNameConnect.To, EventParamsTo.ToArray());
    }

    // Use this for initialization
    void Start () {
        		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
