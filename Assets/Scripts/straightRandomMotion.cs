using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRsqrCore;

public class straightRandomMotion : ULinkBase
{
    
    public float speedChangeTimeWindow;

    public bool Car = false;
    public bool SUV = false;
    public bool Taxi = false;


    private float minSpeed;
    private float maxSpeed;


    [SerializeField]
    private float currSpeed;
    private float lastSpeedUpdateTime;

    // IN FROM BUS
    public ULinkFloat_In busSpeed; 

    // Start is called before the first frame update
    void Start()
    {
        updateSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.realtimeSinceStartup - lastSpeedUpdateTime > speedChangeTimeWindow)
            updateSpeed();
        if (Car || Taxi)
        {
            // cars have roataion of (90, 0, 0), or (0, 90, 90). This works on both cases. 
            // "right" since they only need to change X axis to drive in a straight line. -1 multiplies the "right" since the X axis in the city is actually "backwards"
            // taxi has rotation of (90, 0, 0) or (0, 90, 90)
            transform.position = transform.position + (-1 * transform.right) * currSpeed * Time.deltaTime;   
        }
        else if (SUV)
        {
            // SUV have a rotation of (0, 0, -180) or (0, 0, -90). Since - on z axis anyway, no need to multiply by a negative number so they will drive forward
            transform.position = transform.position + (transform.right) * currSpeed * Time.deltaTime;
        }
    }

    void updateSpeed()
    {
        //currSpeed = Random.value * (maxSpeed - minSpeed) + minSpeed;
        currSpeed = busSpeed.val;
        lastSpeedUpdateTime = Time.realtimeSinceStartup;
    }

    public void busSpeedHandler()
    {
        if (busSpeed.val != 0)
        {
            minSpeed = busSpeed.val - 1;
            maxSpeed = busSpeed.val + 1;
        }
        else
        {
            minSpeed = 0;
            maxSpeed = 0;
        }
        
    }

}
