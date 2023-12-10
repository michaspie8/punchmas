using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//set place where you can add scriptable objects to assets
[CreateAssetMenu(fileName = "New Cutscene", menuName = "Cutscene")]
public class CutsceneSO : ScriptableObject
{
    public string[] pathToSprites;
    public string[] textForFrame;
    public float timeBetweenFrames;
    public float timeBetweenLetters;
    public float timeBetweenSpriteAndText;
    public TypingAudioInfoSO typingAudioInfo;
    public string musicName;
    public float musicDelay;
    //public int[] frameDurations;
}
