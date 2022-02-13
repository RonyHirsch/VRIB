using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRsqrCore;

public class ULinkInputField : ULinkBase //MonoBehaviour
{
    Text question;
    Text answer;

    public ULinkString_Out subjectResponse;

    // Start is called before the first frame update
    void Start()
    {
        question = transform.Find("Question").GetComponent<Text>();
        answer = transform.Find("Answer").GetComponent<Text>();

        gameObject.GetComponent<InputField>().onEndEdit.AddListener(getInput);
    }

    private void getInput(string textInField)
    {
        print(textInField);
        print("question: " + question.text + " ==> answer: " + answer.text);
        subjectResponse.val = answer.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
