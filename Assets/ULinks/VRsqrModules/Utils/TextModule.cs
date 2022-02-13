using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRsqrCore;

public class TextModule : MonoBehaviour {

    [Serializable]
    public class InEventVars : InEvent
    {
        //public EventParam text_old;
        public string Text;
    }
    public InEventVars inParams;

    Text textDisplay;

    private void OnEnable()
    {
        textDisplay = GetComponent<Text>();
        //textDisplay.text = "Rotem";

        inParams.StartListening(gameObject, eventHandler);
    }

    // Use this for initialization
//void Start () {
//}

    public void eventHandler(int EventContext)
    {
        //textDisplay.text = inParams.text.ValString;
        textDisplay.text = inParams.Text;

        //VRsqr_EventsManager.TriggerEvent("NextStep"); //TEMP
    }
}
