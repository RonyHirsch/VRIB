using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;

public class ValidationInstructions : ULinkBase
{
    public ULinkBool_In startButtonPressed;
    public ULinkBool_Out startValidation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startButtonPressedHandler()
    {
        if (startButtonPressed.val)
        {
            startValidation.val = true;
            GetComponent<Renderer>().enabled = false;
        }
    }
}
