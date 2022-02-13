#define VERBOSE

using UnityEngine;
using System.Collections;
using VRsqrCore;
using UnityEngine.UI;
using System;

public class BasicSender : MonoBehaviour
{

    [Serializable]
    public class OutEventVars : OutEvent
    {
        [Header("Output Variables")]
        public int handSpeed;
    }
    public OutEventVars leapMotionParams;
    
    void Start () {
        leapMotionParams.sendData(gameObject);
    }

    private void OnValidate()
    {
        leapMotionParams.sendData(gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
