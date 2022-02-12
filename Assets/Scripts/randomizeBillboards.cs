using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRsqrCore;

/*
 * This script handles the randomization and assignment of billboards in a specific trial:
 ---TAKES BILLBOARD MATERIALS --> AND PUTS TEXTURES (PICS) ON THOSE MATERIALS---
 */
public class randomizeBillboards : ULinkBase
{
    //public ULinkBool_In newTrial; // this is true when the BLOCK gave a command to choose new stim and billboards, i.e new trial

    //public Material[] stimMaterials; // the places in the scene to put images in: ASSUMES THESE WILL ALL PRESENT AN IMAGE,
    // THESE ARE NOT ALL THE OPTIONS IN TOTAL

    public Renderer[] stimRenderers;

    //public int numScrambMats; // the amt of materials that will present scrambled pictures

    public Object[] origPicTextures; // array that holds original images, assumes identical len + order between intact and scrambled folders

    // IN FROM BLOCK
    public ULinkInt_In stimInd; // the index of the stimulus to be presented in a specific trial
    public ULinkBool_In isReplay; //IN from RandomizeTrials which is on BLOCK - is this a replay or not - FOR REPLAY PURPOSES

    // IN FROM GLOBAL SETTINGS
    public ULinkString_In outputPath;
    public ULinkString_In subFolder;

    private string billboardStr = "randBillboardSignsPics";

    // Start is called before the first frame update
    void Start()
    {
        TrialStart();
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
    Object[] LoadPics()
    {
        string p = string.Concat(billboardStr + "/");

        Object[] res = Resources.LoadAll(p, typeof(Texture2D)); // by name, change scrambled names to match intact names
        return res;
    }

    /*
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
    */

    private List<int> ParseScrambledMats()
    {
        // Read the file in a format matched WriteScrambledMats - FOR REPLAY PURPOSES
        string subjectPath = outputPath.val + "\\" + subFolder.val;
        string subjectScramblePath = subjectPath + "\\" + stimInd.val.ToString() + "billboards.txt";  // current scramble locations - FOR REPLAY PURPOSES
        string[] lines = File.ReadAllLines(subjectScramblePath);
        foreach (string line in lines)
        {
            string[] numbers = line.Split(' ');
            if (numbers[0] == stimInd.val.ToString()) // Current stim ind is first on the list
            {
                List<int> scrambled = new List<int>();
                for (int i = 1; i < numbers.Length; i++)
                {
                    scrambled.Add(int.Parse(numbers[i]));
                }
                return scrambled;
            }
        }
        return new List<int>();
    }


    private void WriteScrambledMats(List<int> scrambled)
    {
        // Write the file in a format readable by ParseScrambledMats - FOR REPLAY PURPOSES
        string subjectPath = outputPath.val + "\\" + subFolder.val;
        string subjectScramblePath = subjectPath + "\\" + stimInd.val.ToString() + "billboards.txt"; // current scramble locations

        using (TextWriter tw = new StreamWriter(subjectScramblePath))
        {
            tw.Write(stimInd.val + " "); //current stim ind
            tw.WriteLine(string.Join(" ", scrambled));
        }
    }


    /// <summary>
    /// This function chooses the materials that will present the scrambled version of the stim
    /// </summary>
    /// <param name="numOfScrambled"></param>
    /// <param name="totalNumOfMats"></param>
    /// <returns></returns>
    public List<int> ChooseScrambledMats(int numOfScrambled, int totalNumOfMats)
    {
        System.Random rnd = new System.Random();
        List<int> res;
        var randomNumbers = Enumerable.Range(0, totalNumOfMats).OrderBy(x => rnd.Next()).Take(numOfScrambled).ToList();
        res = (from num in randomNumbers select num).ToList();
        if (isReplay.val)
        {
            res = ParseScrambledMats();
        }
        else
        {
            WriteScrambledMats(res);
        }
        RecordLog("ScrambledLocationsInTrial" + "\t" + string.Join(";", res));
        return res;
    }

    public void RandomizeBillboards()
    {
        System.Random rnd = new System.Random();
        List<int> res;
        var randomNumbers = Enumerable.Range(0, origPicTextures.Length).OrderBy(x => rnd.Next()).Take(stimRenderers.Length).ToList();
        res = (from num in randomNumbers select num).ToList();

        if (isReplay.val)
        {
            res = ParseScrambledMats();
        }
        else
        {
            WriteScrambledMats(res);
        }

        for (int i = 0; i < stimRenderers.Length; i++) // stimMaterials.Length; i++)
        {
            stimRenderers[i].sharedMaterial.mainTexture = (Texture2D)origPicTextures[res[i]];
        }

        return;
    }


    public void TrialStart()
    {
        origPicTextures = LoadPics();
    }

    public void stimIndHandler()
    {
        RandomizeBillboards(); // stimMaterials.Length); // choose which materials will present scrambled versions
    }
}
