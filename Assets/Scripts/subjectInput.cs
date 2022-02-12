using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class subjectInput : MonoBehaviour
{


    Text question;
    Text answer;


    // Start is called before the first frame update
    void Start()
    {
        //question = transform.Find("Question").GetComponent<Text>();
        answer = transform.Find("Answer").GetComponent<Text>();

        gameObject.GetComponent<InputField>().onEndEdit.AddListener(getInput);

    }

    private void getInput(string textInField)
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
