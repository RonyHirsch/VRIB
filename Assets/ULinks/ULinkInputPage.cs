using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRsqrCore;

public class ULinkInputPage : ULinkBase //MonoBehaviour
{
    //public ULinkString_Out subjectInput;
    public ULinkBool_Out gotSubjectDetails;

    // Start is called before the first frame update
    void Start()
    {
 
    }

    public void getInput()
    {
        //subjectInput.val = "";
        foreach (Transform child in transform)
        {
            Transform questionTrans = child.Find("Question");
            Transform answerTrans = child.Find("Answer");

            if (questionTrans != null && answerTrans != null)
            {
                string question = questionTrans.GetComponent<Text>().text;
                string answer = answerTrans.GetComponent<Text>().text;

                RecordLog(question + " " + answer);
            }
        }

        //TODO: add answers verification before proceeding
        gotSubjectDetails.val = true;
        transform.gameObject.SetActive(false); // deactivate current canvas object to hide & disable it 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
