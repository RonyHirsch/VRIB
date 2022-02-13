using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SubjectSettings", menuName = "Subject Settings", order = 51)]
public class SubjectSettings : ScriptableObject
{
    [SerializeField] private string dataPath;
    public string DataPath { get { return dataPath; } }

    // SUBJECT PARAMETERS:
    [SerializeField] private string subjectCode;
    public string SubjectCode { get { return subjectCode; } }
    [SerializeField] private int subjectAge;
    public int SubjectAge { get { return subjectAge; } }
    [SerializeField] private string subjectDomHand;
    public string SubjectDomHand { get { return subjectDomHand; } }
    [SerializeField] private string subjectGender;
    public string SubjectGender { get { return subjectGender; } }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
