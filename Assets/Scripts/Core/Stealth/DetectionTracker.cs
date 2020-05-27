using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MusicState { None, Sneaky, Pensive, Combat }

public class DetectionTracker : MonoBehaviour
{
    public MusicState currentMusicState;
    public MusicState lastMusicState = MusicState.None;

    [SerializeField] [Range(0f,1f)] float currentMaxDetectedValue = 0f;
    public int numEnemiesNearby = 0;

    [Header("Display")]
    [SerializeField] Colorizer indicator = null;
    [SerializeField] Gradient displayGradient = new Gradient();
    [SerializeField] float minYScalar = .35f;
    [SerializeField] float maxYScalar = 1f;

    Dictionary<Detector, float> detectionDict = new Dictionary<Detector, float>();

    [SerializeField] FloatingTextSpawner textSpawner = null;

    public float GetCurrentDetectionValue()
    {
        return currentMaxDetectedValue;
    }

    public void RemoveDetectionValue(Detector detector)
    {
        if (detectionDict.ContainsKey(detector))
        {
            //print("Removing " + detector);
            detectionDict.Remove(detector);
        }
        RecalculateDetection();
    }

    public void AddToDetectedValue(Detector detector)
    {
        //print("Adding " + detector);
        if (detectionDict.ContainsKey(detector))
        {
            // if already a key, modify value
            detectionDict[detector] = detector.detectedPercentage;
        }
        else
        {
            // if not in dictionary, add to it
            detectionDict.Add(detector, detector.detectedPercentage);
        }

        RecalculateDetection();
    }

    // Recalculate detection given all enemies that are detecting the player
    public void RecalculateDetection()
    {
        float _value = 0f;
        string _string = "";
        //for each detector with a detection value
        foreach(KeyValuePair<Detector, float> detector in detectionDict)
        {
            _string += detector.Key.name + "  " + detector.Value;
            _value = Mathf.Max(_value, detector.Value);
        }
        //print(_string);
        currentMaxDetectedValue = Mathf.Clamp(_value, 0f, 1f);


        if (currentMaxDetectedValue == 1)
        {
            currentMusicState = MusicState.Combat;
        }
        else if((currentMaxDetectedValue > 0 && currentMaxDetectedValue < 1) || numEnemiesNearby > 0)
        {
            currentMusicState = MusicState.Pensive;
        }
        else if (currentMaxDetectedValue == 0)
        {
            currentMusicState = MusicState.Sneaky;
        }
    }

    private void Update()
    {
        // if the music is supposed to change
        if(lastMusicState != currentMusicState)
        {
            lastMusicState = currentMusicState;
            //todo Kitarraman - turn off other music

            //play the right type of music
            switch (currentMusicState)
            {
                case MusicState.Combat:
                    print("Playing combat music");
                    textSpawner.SpawnText("Combat", Color.red);
                    //todo Kitarraman - turn on combat music

                    break;
                case MusicState.Pensive:
                    print("Playing pensive music");
                    textSpawner.SpawnText("Pensive", Color.yellow);

                    //todo Kitarraman - turn on pensive music

                    break;
                case MusicState.Sneaky:
                    print("Playing sneaky music");
                    textSpawner.SpawnText("Sneaky", Color.cyan);

                    //todo Kitarraman - turn on sneaky music

                    break;
                case MusicState.None:
                    print("Music is off");
                    break;
            }
        

        }

        switch (currentMusicState)
        {
            case MusicState.Combat:

                //todo Kitarraman - increase volume of combat music

                break;
            case MusicState.Pensive:

                //todo Kitarraman - increase volume of pensive music

                break;
            case MusicState.Sneaky:

                //todo Kitarraman - increase volume of sneaky music

                break;
            case MusicState.None:
                print("Music is off");
                break;
        }

        if (indicator == null) { return; }

        if(currentMaxDetectedValue == 0f && indicator.gameObject.activeInHierarchy)
        {
            print("off");
            indicator.gameObject.SetActive(false);
        }
        else if(currentMaxDetectedValue > 0f && !indicator.gameObject.activeInHierarchy)
        {
            indicator.gameObject.SetActive(true);
        }
        
        indicator.transform.localScale 
            = new Vector3(indicator.transform.localScale.x
                          , Mathf.Lerp(minYScalar, maxYScalar, currentMaxDetectedValue)
                          , indicator.transform.localScale.z);
        indicator.Recolor(displayGradient.Evaluate(currentMaxDetectedValue));
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            numEnemiesNearby++;
            RecalculateDetection();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            numEnemiesNearby--;
            RecalculateDetection();
        }
    }

}
