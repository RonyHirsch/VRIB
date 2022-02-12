using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;

/*
 * This is a script that is currently being used by a spherical BreadCrumb object:
 * The game object is set in place somewhere on the road (manually), and sends its location to whoever is listening (breadCrumbPos).
 * Currently (10/20/20), BusBeesAndPlayer object is listening (MotionConnector), and this is the destination is the breadCrumb's location. 
 */

public class breadCrumb : ULinkBase
{
    public ULinkVector3_Out breadCrumbPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // on each frame, the breadcrumb sends its location
        breadCrumbPos.val = transform.position;
    }
}
