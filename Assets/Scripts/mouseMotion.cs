using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using System.Linq;

public class mouseMotion : ULinkBase
{
    //OUT
    public ULinkVector3_Out position;
    public ULinkVector3_Out direction;
    public ULinkBool_Out buttonPress;

    public float projectionDist = 100f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        position.val = transform.position;
        Vector3 mouseOrigin = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, projectionDist);
        //direction.val = (mousePosition - mouseOrigin).normalized;
        direction.val = (mousePosition - mouseOrigin); 
        buttonPress.val = Input.GetMouseButtonDown(0);
    }
}
