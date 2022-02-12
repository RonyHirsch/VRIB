using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.Linq;

public class endOfTrialQuestionnaire : ULinkBase
{
    private Renderer rend; 
    // IN VARS
    public ULinkBool_In isEndOfTrial;


    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent <Renderer>();
        if (rend != null)
        {
            rend.enabled = false;
        }
        else
        {
            Debug.LogError("renderer not found");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void isEndOfTrialHandler()
    {
        if (isEndOfTrial.val == true)
        {
            rend.enabled = true;
        }
    }
}
