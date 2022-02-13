#define VERBOSE

using UnityEngine;
using System.Collections;
using VRsqrCore;
using UnityEngine.UI;
using System;

public class BasicReceiver : MonoBehaviour
{

    [Serializable]
    public class InEventVars : InEvent
    {
        [Header("Input Variables")]
        public int velocity;
        public int gear;
    }
    public InEventVars carParams;

    bool rotating = false;

    void Awake()
    {
        carParams.StartListening(gameObject, dataHandler);
    }

    void dataHandler(int context)
    {
        if (carParams.velocity > 5)
        {
            rotating = true;
        }
        else
        {
            rotating = false;
        }
    }

    private void Update()
    {
        if (rotating)
        {
            transform.Rotate(new Vector3(1, 0, 0));
        }
    }
}
