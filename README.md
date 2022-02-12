# Virtual-Reality Multi-Trial Inattentional Blindness Paradigm
VR Experiment for Probing Unconscious Processing using Virtual Reality.


Programmed in Unity for usage with HTC VIVE Pro eye headset. Responses are made using the HTC Vive Controllers' trackpad (large pad button at the center of the controller). 

This work was presented in the 9th Israeli Conference on Cognition Research (ISCoP), February 17th, 2022. 


Cite as: 

Hirschhorn, R., Biderman, D., Biderman, N., Yaron, I., Bennet, R., Plotnik, M., & Mudrik, L. (2022). Probing Unconscious Processing using Virtual Reality. Poster presented at the Israeli Conference on Cognition Research of the Israeli Society for Cognitive Psychology (ISCoP), February, online conference.  


![4-13-HIRSCHHORN](https://user-images.githubusercontent.com/38129235/153202110-92b08dfc-3c7d-4261-a8a3-132dec8d57b6.jpg)





## How it works


https://user-images.githubusercontent.com/38129235/153190338-f51027f0-645a-4b81-b03f-c53d0e81dbdb.mp4


The platform contains a module (Unity scene) for collecting subject code, and a module (Unity scene) for the experiment itself.
Within the experiment itself, there is a validation procedure for VIVE Pro eye eye-tracking. It includes an array of nine grey squares. Each time a square turns yellow, the subject is requested to gaze at the square until the color changes back. At the end of the validation procedure, the software reports how many squares were successfully hit. This validation repeats every trial, and can be skipped with the VIVE controller.


Each trial in the experiment includes a 1:03 minute ride across a city street, while three bees are flying in pseudo-random motion in front of the subject's POV (from a front of a moving bus). Note that the experiment was run with background music, yet the music is not included in the programmed experiment and was played in sync with the unity software. 


Experimental stimuli appear on the city's bus stops. There are 10 experimental bus stops: the stimulus image appears scrambled on 7 of them, and intact on the other 3. Each trial contains a single stimulus image. The locations of the intact stimuli is pseudo-randomly drawn at each trial. 
Note that the city contains many posters, billboards, street shops and images. All of these are pseudo-randomly drawn at each trial, to abolish learning and anticipation. 
At the end of each trial, the bees freeze, and subject is to select the target bee by pointing the controller and pressing the trackpad. Based on the bee selection, subjects both gain/lose money (top left corner tracks the accumulated gain), and the bees' speed on the next trial is set (speed increases when the subject is correct and decreases when the subject is wrong). Then, three questions are presented:
- Is the stimulus aversive or neutral
- Rate the stimulus in PAS scale (Ramsoy & Overgaard, 2004)
- Select the stimulus image out of an array of 4 stimuli (2 aversive and 2 neutral)
After 40 trials, 10 trials are randomly selected to be re-played. The replay is accurate, meaning that the bees' speed is unaffected by subjects' response and is played from a recording, as well as all the city environment (is not chosen at random but loaded from pervious trials). The goal in these trials is to answer the questions about the stimulus as accurately as possible. 
 

Contact hirschhorn[at]mail[dot]tau[dot]ac[dot]il for more information
