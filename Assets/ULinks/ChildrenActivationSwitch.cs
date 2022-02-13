using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;

public class ChildrenActivationSwitch : ULinkBase
{
    public ULinkBool_In ChildrenActive;

    // Start is called before the first frame update
    void Start()
    {
        ChildrenActive.RegisterHandler(switchActivation);
        switchActivation(); // intialize according to current value
    }

    void switchActivation()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(ChildrenActive.val);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        switchActivation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
