using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;

public class ActivationSwitch : ULinkBase
{
    // ULink on/off switch for the scene-hierarchy below this object
    [SerializeField]
    public ULinkBool_In Active;

    // Start is called before the first frame update
    void Start()
    {
        //Active.RegisterHandler(switchActivation);
        switchActivation(); // intialize according to initial value in the editor
    }

    // handler function of the Active boolean ULink variable
    public void ActiveHandler()
    {
        DebugLog("Calling switchActivation");
        switchActivation();
    }

    void switchActivation()
    {
        //transform.gameObject.SetActive(Active.val);
        
        foreach (Transform child in transform)
        {
            DebugLog("Calling SetActive for " + child.gameObject + " with value: " + Active.val);
            child.gameObject.SetActive(Active.val);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate(); // maintain proper ULinks communications also when making changes in editor-mode 
        switchActivation();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
