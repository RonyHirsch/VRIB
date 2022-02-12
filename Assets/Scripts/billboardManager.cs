using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;

/*
 * This script is used by each billboard, and makes a billboard send it's location 1 time only 
 */

public class billboardManager : ULinkBase
{
    public ULinkVector3_Out position;
    private bool sent = false;
    // Start is called before the first frame update
    void Start()

    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!sent)
        {
            sent = true;
            position.val = transform.position;
        }
        
    }
}
