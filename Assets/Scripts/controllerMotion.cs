using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using Valve.VR;

public class controllerMotion : ULinkBase
{
    //OUT
    public ULinkVector3_Out position;
    public ULinkVector3_Out direction;
    public ULinkBool_Out buttonPress;
    public ULinkBool_Out buttonRelease;
    public ULinkBool_Out cluePress; // OUT to BeeMotion to indicate subject pressed to ask for a clue

    public float projectionDist = 100f;

    //IN
    public ULinkBool_In showLaser;
    public ULinkBool_In isRepeatingET;
    public ULinkBool_In isTrialEnd;
    public ULinkBool_In isTrialStart;

    // ref for SteamVR controllers
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean teleportAction;
    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private int hintCount;

    private bool pressingButton = false;
    private bool countClues = false;

    void Start()
    {
        hintCount = 0;
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        position.val = transform.position;
        Vector3 mouseOrigin = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, projectionDist);
        //direction.val = (mousePosition - mouseOrigin).normalized;
        direction.val = (mousePosition - mouseOrigin); 
        buttonPress.val = Input.GetMouseButtonDown(0);
        */

        bool buttonStatePressed = teleportAction.GetState(handType);

        if (!laser.activeSelf)
        {
            if (!pressingButton && buttonStatePressed)
            {
                buttonPress.val = true;
                pressingButton = true;
                //laser.SetActive(true);
            }
            else // 
            {
                buttonPress.val = false;
                //laser.SetActive(false);
            }

            if (pressingButton && !buttonStatePressed)
            {
                pressingButton = false;
                buttonRelease.val = true;
            }
            else
            {
                buttonRelease.val = false;
            }
            
            if (buttonStatePressed && countClues) // if press is mid-trial, this is a press for a clue
            {
                cluePress.val = true;
            }

            return;
        }
        ShowLaser(controllerPose.transform.position + transform.forward * projectionDist);

        

        if (!pressingButton && buttonStatePressed)
        {
            buttonPress.val = true;
            pressingButton = true;
            //laser.SetActive(true);
        }
        else // 
        {
            buttonPress.val = false;
            //laser.SetActive(false);
        }

        if (pressingButton && !buttonStatePressed)
        {
            pressingButton = false;
            buttonRelease.val = true;
        }
        else
        {
            buttonRelease.val = false;
        }

        position.val = transform.position;
        direction.val = transform.forward;
    }

    private void ShowLaser(Vector3 targetPos)//RaycastHit hit)
    {
        // 1
        laser.SetActive(true);
        // 2
        laserTransform.position = Vector3.Lerp(controllerPose.transform.position, targetPos, .5f);
        // 3
        laserTransform.LookAt(targetPos);
        // 4
        laserTransform.localScale = new Vector3(laserTransform.localScale.x,
                                                laserTransform.localScale.y,
                                                projectionDist);
    }

    private void OnDisable()
    {
        if (laser != null && laser.activeSelf)
            laser.SetActive(false);
        buttonPress.val = false;
        pressingButton = false;
    }

    public void showLaserHandler()
    {
        if (showLaser.val)
        {
            
            if (laser != null && laser.activeSelf)
                laser.SetActive(false);
            buttonPress.val = false;
            pressingButton = false;
            
            //GameObject.Destroy(laser);
        }
        else
        {
            //laser = Instantiate(laserPrefab);
            //laserTransform = laser.transform;
            
            laser.SetActive(true);
            buttonPress.val = false;
            pressingButton = false;
            
        }
    }

    public void isRepeatingETHandler()
    {
        if (isRepeatingET.val)
        {

            if (laser != null && laser.activeSelf)
                laser.SetActive(false);
            buttonPress.val = false;
            pressingButton = false;

            //GameObject.Destroy(laser);
        }
    }

    public void isTrialStartHandler()
    {
        if (isTrialStart.val)
        {
            countClues = true; // trial started, count clues
        }
    }

    public void isTrialEndHandler()
    {
        if (isTrialEnd.val)
        {
            countClues = false; // trial ended
            laser.SetActive(true);
        }
    }

}
