using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class DetectionTracker : MonoBehaviour
{
    public NPCState highestAlertState = NPCState.Default;
    public NPCState lastState = NPCState.Default;
    [SerializeField] [Range(0f,1f)] float currentMaxDetectedValue = 0f;
    public List<Protector> enemiesNearby = new List<Protector>();

    [Header("Display")]
    [SerializeField] Colorizer indicator = null;
    [SerializeField] Gradient displayGradient = new Gradient();
    [SerializeField] float minYScalar = .35f;
    [SerializeField] float maxYScalar = 1f;

    [Header("Music")]
    public AudioMixer _MasterMixer;
    bool transitionSnapshot = false;

    [Header("Snapshot Settings")]
    public bool transitionUsingSnapshots = true;
    [SerializeField] AudioMixerSnapshot sneakySnapshot = null;
    [SerializeField] AudioMixerSnapshot pensiveSnapshot = null;
    [SerializeField] AudioMixerSnapshot combatSnapshot = null;

    [SerializeField] [Range(0f, 10f)] float sneakyTransitionTime = 1f;
    [SerializeField] [Range(0f, 10f)] float pensiveTransitionTime = 10f;
    [SerializeField] [Range(0f, 10f)] float combatTransitionTime = 10f;

    [Header("AudioSource Settings")]
    public bool changeVolumeOfAudioSourceOnTransition = true;
    [SerializeField] AudioSource sneakyMusic = null;
    [SerializeField] AudioSource pensiveMusic = null;
    [SerializeField] AudioSource combatMusic = null;
    [SerializeField] [Range(0f,1f)] float sneakyMusicMaxVolume = 1f;
    [SerializeField] [Range(0f,1f)] float pensiveMusicMaxVolume = 1f;
    [SerializeField] [Range(0f,1f)] float combatMusicMaxVolume = 1f;

    [SerializeField] [Range(0f,10f)] float sneakyVolumeChangeRate = 1f;
    [SerializeField] [Range(0f,10f)] float pensiveVolumeChangeRate = 1f;
    [SerializeField] [Range(0f, 10f)] float combatVolumeChangeRate = 1f;


    public Dictionary<Detector, float> detectionDict = new Dictionary<Detector, float>();
    public Dictionary<Detector, NPCState> detectionStateDict = new Dictionary<Detector, NPCState>();
    
    private void Start()
    {
        if (transitionUsingSnapshots)
        {
            sneakySnapshot.TransitionTo(0f);
        }
    }

    private void Update()
    {
        UpdateMusicTracks();
        UpdateIndicatorUI();
    }

    //Track nearby enemies
    private void OnTriggerEnter(Collider other)
    {
        Protector prot = other.transform.GetComponent<Protector>();
        //if ((other.gameObject.tag == "Guard" || other.gameObject.tag == "Hunter") && !enemiesNearby.Contains(other.transform.GetComponent<Protector>()))
        if (prot != null && !enemiesNearby.Contains(prot))
        {
            enemiesNearby.Add(prot);
            RecalculateDetection();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Protector prot = other.transform.GetComponent<Protector>();
        //if ((other.gameObject.tag == "Guard" || other.gameObject.tag == "Hunter") && enemiesNearby.Contains(other.transform.GetComponent<Protector>()))
        if (prot != null && enemiesNearby.Contains(prot))
        {
            enemiesNearby.Remove(prot);
            RecalculateDetection();
        }
    }

    
    // Tracking Detectors that are detecting the player
    public float GetCurrentDetectionValue()
    {
        return currentMaxDetectedValue;
    }
    public void RemoveDetectionValue(Detector detector)
    {
        bool reCalculate = false;
        if (detectionDict.ContainsKey(detector))
        {
            detectionDict.Remove(detector);
            reCalculate = true;
        }
        if (detectionStateDict.ContainsKey(detector))
        {
            detectionDict.Remove(detector);
            reCalculate = true;
        }
        if (reCalculate)
        {
            print("Removing " + detector);
            RecalculateDetection();
        }
    }
    public void AddToDetectedValue(Detector detector, NPCState npcState)
    {
        bool reCalculate = false;
        if (detectionDict.ContainsKey(detector))
        {
            print("Modifying " + detector);

            // if already a key, modify value
            if (detectionDict[detector] != detector.detectedPercentage)
            {
                detectionDict[detector] = detector.detectedPercentage;
                reCalculate = true;
            }

        }
        else
        {
            print("Adding " + detector);
            // if not in dictionary, add to it
            detectionDict.Add(detector, detector.detectedPercentage);
            reCalculate = true;
        }

        if (detectionStateDict.ContainsKey(detector))
        {
            // if already a key, modify value
            if(detectionStateDict[detector] != npcState)
            {
                detectionStateDict[detector] = npcState;
                reCalculate = true;
            }
        }
        else
        {
            // if not in dictionary, add to it
            detectionStateDict.Add(detector, npcState);
            reCalculate = true;
        }
        
        if (reCalculate)
        {
            RecalculateDetection();
        }
    }
    // Recalculate detection given all enemies that are detecting the player
    public void RecalculateDetection()
    {
        //print("Recalculating");
        float _value = 0f;
        string _string = "";
        NPCState highestState = NPCState.Default;
        //for each detector with a detection value, get their highest level detection state
        if(detectionDict.Count == 0 || detectionDict.Count == 0)
        {
            highestState = NPCState.Default;
        }
        else
        {
            foreach (KeyValuePair<Detector, float> detector in detectionDict)
            {
                _string += (detector.Key.name + "  " + detector.Value);
                _value = Mathf.Max(_value, detector.Value);
            }
            foreach (KeyValuePair<Detector, NPCState> detector in detectionStateDict)
            {
                _string += (detector.Key.name + "  " + detector.Value);
                if ((int)detector.Value > (int)highestState)
                {
                    highestState = detector.Value;
                }
            }
        }


        if(enemiesNearby.Count > 0)
        {
            if ((int)NPCState.Suspicious > (int)highestState)
            {
                highestState = NPCState.Suspicious;
            }
        }

        if(highestAlertState != highestState)
        {
            lastState = highestAlertState;
            highestAlertState = highestState;
            transitionSnapshot = true;
        }
        if(currentMaxDetectedValue != _value)
        {
            currentMaxDetectedValue = Mathf.Clamp(_value, 0f, 1f);
        }


        //print("Highest Value: " + currentMaxDetectedValue + ", Highest State: " + highestAlertState);
    }
    
    
    // Music
    private void UpdateMusicTracks()
    {
        // Trigger Combat Music
        if (highestAlertState == NPCState.Alert || currentMaxDetectedValue == 1)
        {
            //Snapshot
            if (transitionUsingSnapshots && transitionSnapshot)
            {
                transitionSnapshot = false;
                combatSnapshot.TransitionTo(combatTransitionTime);
            }

            //AudioSource
            if (changeVolumeOfAudioSourceOnTransition)
            {
                //increase combat music volume
                combatMusic.volume = Mathf.Clamp(combatMusic.volume + combatVolumeChangeRate * Time.deltaTime, 0f, combatMusicMaxVolume);
                //decrease other music volume
                pensiveMusic.volume = Mathf.Clamp(pensiveMusic.volume - pensiveVolumeChangeRate * Time.deltaTime, 0f, pensiveMusicMaxVolume);
                sneakyMusic.volume = Mathf.Clamp(sneakyMusic.volume - sneakyVolumeChangeRate * Time.deltaTime, 0f, sneakyMusicMaxVolume);
            }
        }

        // Trigger Pensive Music
        else if (highestAlertState == NPCState.Suspicious || enemiesNearby.Count > 0)
        {
            //Snapshot
            if (transitionUsingSnapshots && transitionSnapshot)
            {
                transitionSnapshot = false;
                pensiveSnapshot.TransitionTo(pensiveTransitionTime);
            }

            //AudioSource
            if (changeVolumeOfAudioSourceOnTransition)
            {
                //increase pensive music volume
                pensiveMusic.volume = Mathf.Clamp(pensiveMusic.volume + pensiveVolumeChangeRate * Time.deltaTime, 0f, pensiveMusicMaxVolume);
                //decrease other music volume
                combatMusic.volume = Mathf.Clamp(combatMusic.volume - combatVolumeChangeRate * Time.deltaTime, 0f, combatMusicMaxVolume);
                sneakyMusic.volume = Mathf.Clamp(sneakyMusic.volume - sneakyVolumeChangeRate * Time.deltaTime, 0f, sneakyMusicMaxVolume);
            }
        }

        // Trigger Sneaky Music
        else if (highestAlertState == NPCState.Default && enemiesNearby.Count == 0)
        {
            //Snapshot
            if (transitionUsingSnapshots && transitionSnapshot)
            {
                transitionSnapshot = false;
                sneakySnapshot.TransitionTo(sneakyTransitionTime);
            }

            //AudioSource
            if (changeVolumeOfAudioSourceOnTransition)
            {
                //increase sneaky music volume
                sneakyMusic.volume = Mathf.Clamp(sneakyMusic.volume + sneakyVolumeChangeRate * Time.deltaTime, 0f, sneakyMusicMaxVolume);
                //decrease other music volume
                combatMusic.volume = Mathf.Clamp(combatMusic.volume - combatVolumeChangeRate * Time.deltaTime, 0f, combatMusicMaxVolume);
                pensiveMusic.volume = Mathf.Clamp(pensiveMusic.volume - pensiveVolumeChangeRate * Time.deltaTime, 0f, pensiveMusicMaxVolume);
            }
        }
    }

    // UI
    private void UpdateIndicatorUI()
    {
        if (indicator == null) { return; }

        if (currentMaxDetectedValue == 0f && indicator.gameObject.activeInHierarchy)
        {
            //print("eye off");
            indicator.gameObject.SetActive(false);
        }
        else if (currentMaxDetectedValue > 0f && !indicator.gameObject.activeInHierarchy)
        {
            //print("eye on");
            indicator.gameObject.SetActive(true);
        }

        indicator.transform.localScale
            = new Vector3(indicator.transform.localScale.x
                          , Mathf.Lerp(minYScalar, maxYScalar, currentMaxDetectedValue)
                          , indicator.transform.localScale.z);
        indicator.Recolor(displayGradient.Evaluate(currentMaxDetectedValue));
    }
}
