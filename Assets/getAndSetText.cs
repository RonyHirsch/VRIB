using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class getAndSetText : MonoBehaviour
{

    public InputField subCode; // the inputed subject code 
    public string currSubPath = "C:\\Users\\Liadlab\\Documents\\VR_output\\currSub.txt";  // the path to the file the subject code will be saved into

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setGet()
    {
        string subCodeInput = subCode.text;
        // save the current subject we are working on in a txt file under the subject directory of the entire experiment
        File.WriteAllText(currSubPath, subCodeInput);
        SceneManager.LoadScene("modular_city_1104");  // get the name of the active scene and then load it
    }
}
