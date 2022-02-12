using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VRsqrCore;

public class randomizeOptions : ULinkBase
{
    public Renderer[] optRenderers;
    // arrays for specific stim types
    public Object[] aversiveOrigPicTextures;
    public Object[] neutralOrigPicTextures;
    public Object[] origPicTextures;

    
    private string aversiveStr = "aversive";
    private string neutralStr = "neutral";

    // IN FROM BLOCK RANDIMIZETARGETS
    public ULinkInt_In stimInd; // the index of the stimulus to be presented in a specific trial
    public ULinkInt_In valenceInd;

    // OUT TO OBJ Q 
    public ULinkInt_Out correctAnsLoc; // the location of the picture which was presented in trial

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        objQStart();
        //changeOptTexture();
        changeOptTextureNEW();
    }

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

    Object[] LoadPostPics(string picName)
    {
        string p = string.Concat("billboardPics/postTest/"+ picName);
        Object[] res = Resources.LoadAll(p, typeof(Texture2D));
        return res;
    }

    public void objQStart()
    {
        aversiveOrigPicTextures = LoadPics("intact", true);
       
        neutralOrigPicTextures = LoadPics("intact", false);
        // unify 
        origPicTextures = new Object[aversiveOrigPicTextures.Length + neutralOrigPicTextures.Length];
        aversiveOrigPicTextures.CopyTo(origPicTextures, 0);
        neutralOrigPicTextures.CopyTo(origPicTextures, aversiveOrigPicTextures.Length);
    }


    public void changeOptTextureNEW()
    {
        string pictureName;
        System.Random rnd = new System.Random();
        Object[] all = new Object[4];
        Object[] postTextures;
        int j = 1;

        if (valenceInd.val == 1) // aversive, need to take 2 neutral and another 1 aversive
        {

            List<Object> aversiveOrigPicTexturesList = aversiveOrigPicTextures.ToList<Object>();
            pictureName = aversiveOrigPicTextures[stimInd.val].name; //Name of target stim picture

        }
        else // need 2 aversive and 1 more neutral
        {
            List<Object> neutralOrigPicTexturesList = neutralOrigPicTextures.ToList<Object>();
            pictureName = neutralOrigPicTextures[stimInd.val - aversiveOrigPicTextures.Length].name; //Name of target stim picture

        }
        postTextures = LoadPostPics(pictureName); // load the relevant quadruplet belonging to pictureName

        for (int i = 0; i < postTextures.Length; i++)
        {
            if (postTextures[i].name == pictureName)
            {
                all[0] = postTextures[i];
            }
            else
            {
                all[j] = postTextures[i];
                j++;
            }
        }
        var shuffled = Enumerable.Range(0, 4).OrderBy(x => rnd.Next()).Take(4).ToList();


        for (int i = 0; i < 4; i++)
        {
            // shuffle the stimuli : 0 is the leftmost objQ pic and 3 is the rightmost
            optRenderers[shuffled[i]].sharedMaterial.mainTexture = (Texture2D)all[i];
            if (i != 0)
            {
                RecordLog("PicLocation" + "\t" + shuffled[i].ToString() + "\t" + "PicName" + "\t" + ((Texture2D)all[i]).name + "\t" + "DISTRACTOR");
            }
            else
            {
                RecordLog("PicLocation" + "\t" + shuffled[i].ToString() + "\t" + "PicName" + "\t" + ((Texture2D)all[i]).name + "\t" + "TARGET");
            }
        }

        correctAnsLoc.val = shuffled[0]; // send the loc of the right stim
     
    }


    /*
public void changeOptTexture()
{
    System.Random rnd = new System.Random();
    Object[] all = new Object[4];

    if (valenceInd.val == 1) // aversive, need to take 2 neutral and another 1 aversive
    {
        List<Object> aversiveOrigPicTexturesList = aversiveOrigPicTextures.ToList<Object>();
        aversiveOrigPicTexturesList.Remove(aversiveOrigPicTextures[stimInd.val]);
        var randomNumbers = Enumerable.Range(0, aversiveOrigPicTexturesList.Count).OrderBy(x => rnd.Next()).Take(1).ToList();

        var randomNumbers2 = Enumerable.Range(0, neutralOrigPicTextures.Length).OrderBy(x => rnd.Next()).Take(2).ToList();

        all[0] = origPicTextures[stimInd.val];
        all[1] = aversiveOrigPicTexturesList[randomNumbers[0]];
        all[2] = neutralOrigPicTextures[randomNumbers2[0]];
        all[3] = neutralOrigPicTextures[randomNumbers2[1]];

    }
    else // need 2 aversive and 1 more neutral
    {
        List<Object> neutralOrigPicTexturesList = neutralOrigPicTextures.ToList<Object>();
        neutralOrigPicTexturesList.Remove(neutralOrigPicTextures[stimInd.val - aversiveOrigPicTextures.Length]);
        var randomNumbers = Enumerable.Range(0, neutralOrigPicTexturesList.Count).OrderBy(x => rnd.Next()).Take(1).ToList();

        var randomNumbers2 = Enumerable.Range(0, aversiveOrigPicTextures.Length).OrderBy(x => rnd.Next()).Take(2).ToList();

        all[0] = origPicTextures[stimInd.val];
        all[1] = neutralOrigPicTexturesList[randomNumbers[0]];
        all[2] = aversiveOrigPicTextures[randomNumbers2[0]];
        all[3] = aversiveOrigPicTextures[randomNumbers2[1]];
    }

    var shuffled = Enumerable.Range(0, 4).OrderBy(x => rnd.Next()).Take(4).ToList();


    for (int i=0; i<4; i++)
    {
        //optRenderers[i].sharedMaterial.mainTexture = (Texture2D)all[shuffled[i]];
        optRenderers[shuffled[i]].sharedMaterial.mainTexture = (Texture2D)all[i];
    }

    correctAnsLoc.val = shuffled[0]; // send the loc of the right stim
    int sdf = 5;
}*/
}
