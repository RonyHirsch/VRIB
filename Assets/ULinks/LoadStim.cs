using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;

/// <summary>
/// the GameObject thiss sctipt is attached to is a Stimulus Object, presenting the current sprite as a stimulus
/// </summary>
//public class LoadStim : MonoBehaviour
[Serializable]
public class LoadStim : ULinkBase
{

    public SpriteRenderer spriteR;
    public Sprite[] expStimuli;
    public int spriteInd = 0;
    public string spriteFileName; // this is the folder within Resources/Sprites to take the stimuli from 

    [SerializeField]
    ULinkInt_In stimInd;
    [SerializeField]
    ULinkBool_In isEnabled;


    // awake is called before start
    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteR = gameObject.GetComponent<SpriteRenderer>(); // get the stimulus' sprite renderer component
        spriteR.enabled = false; //the stimulus isn't shown until there's something relevant there

        // load all stiumli images
        loadStimuli();

        // initialize the first image 
        spriteR.sprite = expStimuli[spriteInd];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// this method loads all the sprites that are found in Resources folder to an array of sprites to be used by an object
    /// </summary>
    void loadStimuli()
    {
        // load all images from Resources folder to an array
        UnityEngine.Object[] loadedStimuli = Resources.LoadAll("Sprites"+"/"+ spriteFileName, typeof(Sprite)); //add folder/name param
        expStimuli = new Sprite[loadedStimuli.Length];
        loadedStimuli.CopyTo(expStimuli, 0);
    }

    /// <summary>
    /// this method updates the current image (sprite) in the stimulus 
    /// </summary>
    /// <param name="context"></param>
    public void isEnabledHandler() // reserved name for <varName>Handler function
    {
        //VRsqrUtil.Debug.Log(LogLevel.Debug, "update stim: " + spriteParamsIn.stimInd);
        VRsqrUtil.Debug.Log(LogLevel.Debug, "update stim: " + stimInd.val);
        if (isEnabled.val && stimInd.val <= expStimuli.Length - 1)
        {
            spriteR.sprite = expStimuli[stimInd.val];
            spriteR.enabled = true;
        }
        else
        {
            spriteR.enabled = false;
        }
        
    }
}
