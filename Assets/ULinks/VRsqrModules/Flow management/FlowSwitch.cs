using UnityEngine;
using System.Collections;
using VRsqrCore;

public class FlowSwitch : ULinkBase
{
    public ULinkBool_In switchOn;
    public ULinkBool_In switchOff;

    public bool inverseSwitch = false;

	// Use this for initialization
	void Start () {

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(inverseSwitch ? !switchOn.val : switchOn.val); // !child.gameObject.activeSelf);
        }
    }

    public void switchOnHandler()
    {
        ////Debug.Log("InitFunction was called!");
        foreach (Transform child in transform)
        {
            if (switchOn.val)
            {
                child.gameObject.SetActive(true);
            }
                
        }
    }

    public void switchOffHandler()
    {
        ////Debug.Log("InitFunction was called!");
        foreach (Transform child in transform)
        {
            if (switchOff.val)
                child.gameObject.SetActive(false);
        }
    }
}
