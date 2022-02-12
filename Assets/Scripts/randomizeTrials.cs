using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRsqrCore;

public class randomizeTrials : ULinkBase
{
    //public ULinkBool_In newTrial; // this is true when the BLOCK gave a command to choose new stim and billboards, i.e new trial

    //public Material[] stimMaterials; // the places in the scene to put images in: ASSUMES THESE WILL ALL PRESENT AN IMAGE,
    // THESE ARE NOT ALL THE OPTIONS IN TOTAL

    public Renderer[] stimRenderers;
    public int aversiveReplayTrialsNum; //number of aversive pictures to present as replay trials - FOR REPLAY PURPOSES
    public int neutralReplayTrialsNum; //number of neutral pictures to present as replay trials - FOR REPLAY PURPOSES

    public int numScrambledPics; // the amt of pictures that will present scrambled stim
    public Object[] origPicTextures; // array that holds original images, assumes identical len + order between intact and scrambled folders

    // arrays for specific stim types
    public Object[] aversiveScrambledPicTextures;
    public Object[] aversiveOrigPicTextures;
    public Object[] neutralScrambledPicTextures;
    public Object[] neutralOrigPicTextures;

    // OUT TO randomizeOptions IN OBJTRIALAWARENESS
    public ULinkInt_Out numScrambMats; // the amt of materials that will present scrambled pictures
    public ULinkInt_Out stimInd; // the index of the stimulus to be presented in a specific trial
    public ULinkInt_Out valenceInd; // the valence of the stim

    // OUT TO BeeMotion
    public ULinkBool_Out isReplay; // - FOR REPLAY PURPOSES

    // OUT TO VALIDATION INSTRUCTIONS TO START FLOW
    public ULinkBool_Out startTrialFlow; // whether to start the next trial flow (which is validation instructions->validation->trial->q's)

    //OUT TO subjectiveQProjection on SubjTrialAwareness
    public ULinkInt_Out currTrialIndexOut;


    // IN FROM VALIDATION - INDICATING TRIAL START
    public ULinkBool_In isTrialStart; // that's the trigger for BLOCK to send OUT the CURRENT STIMULUS NUMBER
    
    // IN FROM GLOBAL SETTINGS
    public ULinkString_In outputPath;
    public ULinkString_In subFolder;


    private string aversiveStr = "aversive";
    private string neutralStr = "neutral";
    private List<int> chosenPics; // the list of the actual pics chosen for the entire experiment (INDICES FROM CONCATENATING THE LISTS OF AVERSIVE AND NEUTRAL)
    private List<int> chosenPicsValence; // for each stim in chosenPics, is it neutral (0) or aversive (1)
    private string stimOrderPath = "";
    private string currStimPath = "";
    
    
    // Start is called before the first frame update
    void Start()
    {
        currTrialIndexOut.val = 0;
    }


    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Loads all the stimuli pictures to an array of textures
    /// </summary>
    /// <param name="folderName">the name of the path UNDER RESOURCES that contains all the releavnt pics</param>
    /// <returns>an array of objects that contains pictures</returns>
    Object[] LoadPics(string folderName, bool isAversive)
    {
        string p = string.Concat("billboardPics/", folderName + "/");
        if (isAversive)
        {
            p = string.Concat(p, aversiveStr);
        }
        else
        {
            p = string.Concat(p, neutralStr);
        }
        Object[] res = Resources.LoadAll(p, typeof(Texture2D)); // by name, change scrambled names to match intact names
        return res;
    }

    /// <summary>
    /// This function chooses the (intact) stim that will appear on the materials in the scene (scrambled stim are yoked to intact ones)
    /// </summary>
    /// <param name="numOfStimTextures">len of origStimTextures: how many pics we pool from </param>
    /// <param name="numOfIntact"> how many pics we want to sample </param>
    /// <returns> an array containing the indices of all (intact) stim that will appear </returns>
    public List<int> SampleTexture(int numOfStimTextures, int numOfIntact)
    {
        System.Random rnd = new System.Random();
        List<int> res;
        var randomNumbers = Enumerable.Range(0, numOfStimTextures).OrderBy(x => rnd.Next()).Take(numOfIntact).ToList();
        res = (from num in randomNumbers select num).ToList();
        return res;
    }


    public void ReadExperiment()
    {
        currTrialIndexOut.val = int.Parse(File.ReadAllText(currStimPath));
        chosenPics = File.ReadLines(stimOrderPath).Select(x => int.Parse(x)).ToList();
    }

    public void ExperimentNew()
    {
        currTrialIndexOut.val = 0;
        // aversive pics and scrambled
        aversiveScrambledPicTextures = LoadPics("scrambled", true);
        aversiveOrigPicTextures = LoadPics("intact", true);
        // neutral pics and scrambled
        neutralScrambledPicTextures = LoadPics("scrambled", false);
        neutralOrigPicTextures = LoadPics("intact", false);
        // unify 
        origPicTextures = new Object[aversiveOrigPicTextures.Length + neutralOrigPicTextures.Length];
        aversiveOrigPicTextures.CopyTo(origPicTextures, 0);
        neutralOrigPicTextures.CopyTo(origPicTextures, aversiveOrigPicTextures.Length);
        // the following function returns INDICES from origPicTextures (unified list) to appear in the experimental trials. 
        chosenPics = SampleTexture(origPicTextures.Length, origPicTextures.Length);

        // Add replay trials to the end of the chosenPics
        List<int> chosenAversiveReplayPics = new List<int>();
        chosenAversiveReplayPics = SampleTexture(aversiveOrigPicTextures.Length, aversiveReplayTrialsNum);
        List<int> chosenNeutralReplayPics = new List<int>();
        chosenNeutralReplayPics = SampleTexture(neutralOrigPicTextures.Length, neutralReplayTrialsNum);
        List<int> unitedReplayPics = new List<int>();
        for (int i = 0; i < chosenNeutralReplayPics.Count; i++)
        {
            chosenNeutralReplayPics[i] += aversiveOrigPicTextures.Length; // Make the neutral stimuli in the correct index
        }

        System.Random rand = new System.Random();
        unitedReplayPics = chosenNeutralReplayPics.Concat(chosenAversiveReplayPics).ToList(); // Unite the lists
        unitedReplayPics = unitedReplayPics.OrderBy(a => rand.Next()).ToList(); // Shuffle the replay list
        chosenPics.AddRange(unitedReplayPics);

        // Now put valence for each trial, both game and replay
        chosenPicsValence = new List<int>(); // 0 is neutral, 1 is aversive
        RecordLog("chosenPicsOrder" + "\t" + string.Join(";", chosenPics));
        for (int i=0; i< chosenPics.Count; i++)
        {
            if (chosenPics[i] < aversiveOrigPicTextures.Length)
            {
                chosenPicsValence.Add(1); // aversive
            }
            else
            {
                chosenPicsValence.Add(0); // neutral
            }
        }

        RecordLog("chosenPicsValence" + "\t" + string.Join(";", chosenPicsValence));
        System.IO.File.WriteAllText(currStimPath, (currTrialIndexOut.val).ToString()); 
        System.IO.File.WriteAllLines(stimOrderPath, chosenPics.Select(x=> string.Join("\n", x)));
    }

    public void ExperimentStart()
    {
        // aversive pics and scrambled
        aversiveScrambledPicTextures = LoadPics("scrambled", true);
        aversiveOrigPicTextures = LoadPics("intact", true);
        // neutral pics and scrambled
        neutralScrambledPicTextures = LoadPics("scrambled", false);
        neutralOrigPicTextures = LoadPics("intact", false);
        // unify 
        origPicTextures = new Object[aversiveOrigPicTextures.Length + neutralOrigPicTextures.Length];
        aversiveOrigPicTextures.CopyTo(origPicTextures, 0);
        neutralOrigPicTextures.CopyTo(origPicTextures, aversiveOrigPicTextures.Length);
        // the following function returns INDICES from origPicTextures (unified list) to appear in the experimental trials. 
        chosenPicsValence = new List<int>(); // 0 is neutral, 1 is aversive
        for (int i = 0; i < chosenPics.Count; i++)
        {
            if (chosenPics[i] < aversiveOrigPicTextures.Length)
            {
                chosenPicsValence.Add(1); // aversive
            }
            else
            {
                chosenPicsValence.Add(0); // neutral
            }
        }
        System.IO.File.WriteAllText(currStimPath, (currTrialIndexOut.val).ToString()); 
    }

    public void isTrialStartHandler()
    {
        if (isTrialStart.val)
        {
            if (currTrialIndexOut.val < chosenPics.Count)
            {
                numScrambMats.val = numScrambledPics;
                stimInd.val = chosenPics[currTrialIndexOut.val];
                valenceInd.val = chosenPicsValence[currTrialIndexOut.val];
            }
        }
    }

    // Handler only to this because this is the second parameter of the two strings
    public void subFolderHandler()
    {
        string subjectPath = outputPath.val + "\\" + subFolder.val;
        stimOrderPath = subjectPath + "\\" + "stimOrder.txt";
        currStimPath = subjectPath + "\\" + "nextStimInd.txt";
        if (!File.Exists(stimOrderPath))
        {
            ExperimentNew();  // randomize all stimuli in exp and set order
        }
        else
        {
            ReadExperiment();
            ExperimentStart();
        }
        // If the current index exceeds the amount of aversive + neutral images, we are in replay
        if (currTrialIndexOut.val >= aversiveOrigPicTextures.Length + neutralOrigPicTextures.Length)
        {
            isReplay.val = true;
        }
        else
        {
            isReplay.val = false;
        }
        startTrialFlow.val = true;

    }
}

