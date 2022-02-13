using Assets.Scripts.ULinkUtils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRsqrCore;
using VRsqrUtil;

public class Randomizer : ULinkBase
{
    // IN params FROM SESSION
    [SerializeField]
    public ULinkInt_In isNBack; // IF THIS IS AN N-BACK EXP : 1 ELSE (psychophysics): 0
    public ULinkIntList_In stimuliIndBank; // the possible stimuli indexes for this block
    public ULinkIntList_In ISIsBank; // all the possible ISI times to sample from, assuming they are sorted from smallest to largest
    public ULinkInt_In numOfStimPerBlock; // amount of UNIQUE stimuli per block (meaning, raw net amt, not considering n-back repetitions. how many different pics do we want to use?)
    public ULinkInt_In numOfBlocks; 
    public ULinkIntList_In numOfNBrepsPerBlock; // amount of N-back repetitions per block (meaning, how many stimulu will be repeated in a 1-back manner?) 

    //[SerializeField]
    public ULinkInt_In numOfTrials; // number of trial in this block

    public ULinkInt_In nBack; // the n in the n-back paradigm
    public ULinkInt_In numOfNReps; // the amount of stimuli that are repeated in the block (3 reps = 3 stim will be repeated = 6 trials)
    public ULinkBool_In reps; // if indexes that are not n-back can appear again 
    public ULinkBool_In repsExclusive; // if the repeated indexes can appear again 

    // OUT params to SESSION
    public ULinkIntList_Out stimOrder;

    public bool randomize = true;
    public int randomSeed;

    // Start is called before the first frame update
    void Start()
    {
        if (randomSeed == 0)
        {
            randomSeed = (int)GlobalSettings.TimeStamp();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (randomize)
        {
            //DebugLog("setting stimOrder");
            //stimOrder.val = NBack(stimuliIndBank.val, nBack.val, numOfNReps.val, reps.val, repsExclusive.val, numOfTrials.val);
            //stimOrder.val = PsychophysicsCurve(stimuliIndBank.val, ISIsBank.val);
            //randomize = false;
        }
        
    }

    public void stimuliIndBankHandler()
    {
        DebugLog("setting stimOrder");
        if (isNBack.val == 1){
            stimOrder.val = NB(stimuliIndBank.val, ISIsBank.val, numOfStimPerBlock.val, numOfBlocks.val, numOfNBrepsPerBlock.val);
        }
        else
        {
            stimOrder.val = PsychophysicsCurve(stimuliIndBank.val, ISIsBank.val);
        }
        randomize = false;
    }

    /// <summary>
    /// randomly samples sampleSize amount of elemnts from inputList, with/without repetition
    /// </summary>
    /// <param name="inputList"> the elemets list to sample from </param>
    /// <param name="sampleSize"> the amount of elements to sample </param>
    /// <param name="rep"> with repetition (default: false) </param>
    /// <returns> sampledList of sampled elements from the original list </returns>
    public List<int> Sample(List<int> inputList, int sampleSize, bool rep = false)
    {
        System.Random rnd = new System.Random(randomSeed);
        List<int> sampledList;
        int n = inputList.Count();
        if (n < sampleSize && rep == false)
        {
            print("sample size smaller than amt of elems. returns the original list in random order");
        }

        if (!rep) // without repetitions
        {
            // take all indexes and order them randomally, then sample the first sampleSize elements
            var randomNumbers = Enumerable.Range(0, n).OrderBy(x => rnd.Next()).Take(sampleSize).ToList();
            sampledList = (from num in randomNumbers select inputList[num]).ToList();
        }

        else // with repetitions
        {
            // for each number in range of sample size, draw a random element-indexed from the list 
            sampledList = (from num in Enumerable.Range(0, sampleSize) select inputList[rnd.Next(n)]).ToList();
        }

        return sampledList;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputList"></param>
    /// <param name="n"> the N of the N-BACK paradigm</param>
    /// <param name="numOfNReps"> number of stimuli that must be repeated in an n-back fassion within the flow </param>
    /// <param name="reps">this parameter mentions if indexes that are not n-back can appear again (for example: if 
    /// "true" then : 1-2-3-3-4-5-3-1..., if "false" then "1" can't appear again </param>
    /// <param name="repsExclusive">this parameter mentions if the repeated indexes can appear again (for example: if 
    /// "false" then : 1-2-3-3-4-5-3-... if "true" then "3" can't appear outside a rep </param>
    /// <param name="outputSize"> this is the length of the returned list of indices </param>
    /// <returns></returns>
    public List<int> NBack(List<int> inputList, int n, int numOfNReps, bool reps, bool repsExclusive, int outputSize)
    {
        // initialize result list with impossible indices
        List<int> nbList = Enumerable.Repeat(-1, outputSize).ToList();

        // use the internal Sample function w/o repetitions to choose the indices that will be repeated:
        List<int> repStimInds = Sample(inputList, numOfNReps, false);

        // the amt of stimuli that are not N-backed:
        int restLen = outputSize - (numOfNReps * 2);
        List<int> restList = new List<int>(restLen);

        // the list of indices we sample from:
        List<int> indList = new List<int>();

        if (repsExclusive) // if the indexes that are n-backed can only appear in the n-back context
        {
            // then the list we can sample from does not include the indexes chosen to be n-backed
            indList = inputList.Except(repStimInds).ToList();

            System.Random randN = new System.Random(randomSeed + 1);
            System.Random randR = new System.Random(randomSeed + 2);

            // place the n-back stimuli in the final result list
            for (int i = 0; i < repStimInds.Count; i++)
            {
                bool placedInd = false;
                while (!placedInd) // while a place wasn't found yet for the current i in the repStimInds array
                {
                    int randInd = randN.Next(0, outputSize - n);
                    if (nbList[randInd] != -1 || nbList[randInd + n] != -1)
                    {
                        continue;
                    }
                    nbList[randInd] = repStimInds[i];
                    nbList[randInd + n] = repStimInds[i];
                    placedInd = true;
                }
            }

            if (!reps) //if in the rest of the indices (not n-backed) there cannot be any reps
            {
                restList = Sample(indList, restLen, false);
                // fill in the rest of the list with the "rest" of the indices:
                int j = 0;
                for (int i = 0; i < nbList.Count; i++)
                {
                    if (nbList[i] == -1)
                    {
                        nbList[i] = restList[j];
                        j++;
                    }
                }
            }

            else // the rest of the indices can appear more than once in the sequence
            {

                for (int i = 0; i < nbList.Count; i++)
                {

                    if (nbList[i] == -1)
                    {
                        bool isNumGood = false; // this marks whether or not the sampled number below is good for the place in the list we're trying to fill
                        while (!isNumGood)
                        {
                            int rand = randR.Next(0, indList.Count - 1);
                            int currNum = indList[rand];

                            // if not adding another n-back, then the number is good - insert it
                            if ((i - n < 0 || nbList[i - n] != currNum) && (i + n > nbList.Count - 1 || nbList[i + n] != currNum))
                            {
                                nbList[i] = currNum;
                                isNumGood = true;
                            }
                        }

                    }
                }
            }

        }
        else // if the indexes that are n-backed can appear not only in the in the n-back context
        {
            indList = inputList.ToList();
        }

        return nbList;
    }

    /// <summary>
    /// This function creates the full stimuli list for a single block (with NBack repetitions)
    /// </summary>
    /// <param name="stimListPerBlock"></param>
    /// <param name="ISIInputList"></param>
    /// <param name="numOfStimPerBlock"></param>
    /// <param name="numOfNBreps"></param>
    /// <returns></returns>
    List<int> NB_Block(List<int> stimListPerBlock, List<int> ISIInputList, int numOfStimPerBlock, int numOfNBreps)
    {
        System.Random seed = new System.Random();
        System.Random rnd = new System.Random(seed.Next(1,1000));
        int i = 0;
        List<int> blockStimList; // a list containing all of this block's stimuli indices (example: [4, 3])
        List<int> outputList = new List<int>();
        List<int> repLocationList; // a list containing all the locations in blockStimList of stimuli that will repeat themselves in a 1-back fashion (example:[0] --> block is [4, 4, 3])
        // if repetition of stimuli between blocks is allowed: 
        blockStimList = Sample(stimListPerBlock, numOfStimPerBlock); // shuffle this blocks' stimuli, so their order will be at random
        // create the list of indices of blockStimList that'll be repeated: 
        repLocationList = Enumerable.Range(0, numOfStimPerBlock).ToList(); // list of all indices between 0 and the length of blockStimList
        repLocationList = Sample(repLocationList, numOfNBreps); // sample so we'll have "numOfReps" repetitions
        repLocationList.Sort();
        // fill the final list of stimuli for this block, including the 1-back repetitions and their according ISIs
        for (int j = 0; j < numOfStimPerBlock; j++)
        {
            outputList.Add(stimListPerBlock[j]); // add the stimulus (by its index)
            // if this stimulus is a repetition (1-back), mak sure the ISIs are 1 of 2 cases that interest us
            if (repLocationList.Contains(j))
            {
                i = rnd.Next(2); // a random number, either 0 or 1 (coin flip) 
                if (i == 0)
                {
                    outputList.Add(ISIInputList[0]); // add the minimal SOA (meaning, the first appearance of the stim-stim couple is masked)
                }
                else
                {
                    outputList.Add(ISIInputList[ISIInputList.Count - 1]); // add the maximal SOA (meaning, both appearences of stimulus are visible)
                }
                outputList.Add(stimListPerBlock[j]); // add the 1-back repetition of this stimulus
                outputList.Add(ISIInputList[ISIInputList.Count - 1]); // add the rep as always visible (maximal ISI)
            }
            else // this is not a 1-back sequence, sample ISI out of all the possible options at random
            {
                outputList.Add(ISIInputList[rnd.Next(0, ISIInputList.Count)]);
            }
        }
        return outputList;
    }

    List<int> NB(List<int> stimInputList, List<int> ISIInputList, int numOfStimPerBlock, int numOfBlocks, List<int> numOfNBrepsPerBlock, bool stimRepBetweenBlocks=false)
    {
        // in any case, outputList is a single list of all stimuli FOR THE ENTIRE SESSION as it was calculated per blocks (with or w/o reps between blocks), 
        // including the 1-b reps. 
        List<int> outputList = new List<int>();
        
        if (stimRepBetweenBlocks) // repetitions of stimuli between different blocks are allowed
        {
            for (int i = 0; i < numOfBlocks; i++)
            {
                outputList.AddRange(NB_Block(stimInputList, ISIInputList, numOfStimPerBlock, numOfNBrepsPerBlock[i]));
            }
        }
        else // there are no repetitions of stimuli between different blocks
        {
            List<int> blockStimList; // create a list of stimuli PER block
            stimInputList = Sample(stimInputList, stimInputList.Count); // randomize stimInputList
            for (int i = 0; i < numOfBlocks; i++) // for each block
            {
                blockStimList = new List<int>(); 
                blockStimList.AddRange(stimInputList.GetRange(i * numOfStimPerBlock, numOfStimPerBlock)); // out of all stimuli, take the needed amount - don't repeat what was already taken
                outputList.AddRange(NB_Block(blockStimList, ISIInputList, numOfStimPerBlock, numOfNBrepsPerBlock[i]));
            }
        }
        return outputList;
    }

    List<int> PsychophysicsCurve(List<int> stimInputList, List<int> ISIInputList)
    {
        List<int> stimLongList = new List<int>();
        List<int> ISILongList = new List<int>();
        List<int> listToReturn = new List<int>();
        // this function will return a single list (array) in which 0, 2, 4, ... indices are stim indices, and 1, 3, 5... are ISIs
        for (int j = 0; j<stimInputList.Count; j++)
        {
            for (int i = 0; i < ISIInputList.Count; i++)
            {
                stimLongList.Add(stimInputList[j]);
                ISILongList.Add(ISIInputList[i]);
            }
        }
        List<int> listToRandomize = Enumerable.Range(0, stimLongList.Count).ToList<int>();
        listToRandomize = Sample(listToRandomize, stimLongList.Count);
        for (int i = 0; i < listToRandomize.Count; i++)
        {
            listToReturn.Add(stimLongList[listToRandomize[i]]);
            listToReturn.Add(ISILongList[listToRandomize[i]]);
        }
        return listToReturn;
    }
}
