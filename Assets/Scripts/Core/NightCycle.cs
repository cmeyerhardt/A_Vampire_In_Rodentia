using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TimeSegment { None, Sunset, Dusk, Night, Dawn, Sunrise }

public class NightCycle : MonoBehaviour
{
    public TimeEvent timeEvent;
    [Header("Track Values")]
    public TimeSegment currentTimeSegment = TimeSegment.Sunset;
    [SerializeField] [Range(0, 1)] float nightPercentage = 0f;
    [SerializeField] float currentTime = 0f;
    public float currentHour = 0f;
    public float numGameSecPerRealSec = 0f;

    public Material currentSky = null;
    public Light currentCelestialBodyLightSource = null;
    public Gradient currentGradient = null;

    [Header("Configure Values")]
    [SerializeField] float nightLengthRealSeconds = 100f;
    [SerializeField] int nightLengthGameHours = 8;
    [SerializeField] int startHour = 8;
    //[SerializeField] int endHour = 6;
    

    [Header("Sun")]
    //public bool sunIsOut = false;
    [SerializeField] Gradient sunGradient = null;
    [SerializeField] Light sun = null;
    [SerializeField] Material day = null;
    //[SerializeField] float switchToSunPerc = .8f;

    [SerializeField] Quaternion sunSunsetRotation;
    [SerializeField] Quaternion sunSunriseRotation;

    [Header("Moon")]
    //public bool moonIsOut = false;
    [SerializeField] Gradient moonGradient = null;
    [SerializeField] Light moon = null;
    [SerializeField] Material night = null;
    //[SerializeField] float switchToMoonPerc = .2f;
    [SerializeField] Quaternion moonSunsetRotation;
    [SerializeField] Quaternion moonMidnightRotation;
    [SerializeField] Quaternion moonSunriseRotation;
    Quaternion currentMoonGoal;
    Quaternion lastMoonGoal;

    //bool moonRising = true;

    [SerializeField] Vector3 moonStartRotation = new Vector3();
    [SerializeField] Vector3 moonMidRotation = new Vector3();
    [SerializeField] Vector3 moonEndRotation = new Vector3();
    [SerializeField] Vector3 moonGoalV = new Vector3();
    [SerializeField] Vector3 lastGoalV = new Vector3();
    
    [SerializeField] float nightPerc = .2f;
    [SerializeField] float dawnPerc = .8f;

    //bool morning = false;
    public bool go = false;
    public bool triggerLose = false;
    public bool isMoonOut = false;

    //[System.Serializable]
    //public class TimeSettings
    //{
    //    [SerializeField] public float beginPerc;
    //    [SerializeField] public float percDuration;
    //    [SerializeField] public Quaternion sunRotation;
    //    [SerializeField] public Quaternion moonRotation;
    //    [SerializeField] public Color directionalLightColor = Color.white;
    //}

    //Dictionary<TimeOfNight, TimeSettings> timeDictionary = new Dictionary<TimeOfNight, TimeSettings>();
    //[SerializeField] TimeSettings[] timeSettings = null;

    //[SerializeField] Color sunsetColor = Color.white;
    //[SerializeField] Color duskColor = Color.white;
    //[SerializeField] Color nightColor = Color.white;
    //[SerializeField] Color dawnColor = Color.white;
    //[SerializeField] Color sunriseColor = Color.white;

    [Header("Reference")]
    [SerializeField] TextMeshProUGUI text = null;
    [SerializeField] TextMeshProUGUI percText = null;

    void Start()
    {
        nightPercentage = 0f;
        currentTime = 0f;

        lastMoonGoal = moonSunsetRotation;
        currentMoonGoal = moonMidnightRotation;

        lastGoalV = moonStartRotation;
        moonGoalV = moonMidRotation;

        numGameSecPerRealSec = (nightLengthGameHours * 60f) / nightLengthRealSeconds;
        ChangeTimeOfNightSettings("day");

    }

    void Update()
    {
        TimeSegment time = DetermineTime(nightPercentage);
        if (time != currentTimeSegment)
        {
            // Initialize cycle settings
            print("Time is now: " + time);
            currentTimeSegment = time;
            switch (currentTimeSegment)
            {
                case TimeSegment.Sunset:
                    currentTimeSegment = TimeSegment.Dusk;
                    break;
                case TimeSegment.Dusk:
                    break;
                case TimeSegment.Night:
                    timeEvent.Invoke(TimeSegment.Night);
                    ChangeTimeOfNightSettings("night");
                    break;
                case TimeSegment.Dawn:
                    timeEvent.Invoke(TimeSegment.Dawn);
                    ChangeTimeOfNightSettings("day");
                    break;
                case TimeSegment.Sunrise:
                    if (triggerLose)
                    {
                        GetComponent<LevelManager>().GameTimeDone();
                    }
                    break;
            }
        }
        //if(nightPercentage >= .5f)
        //{
        //    lastMoonGoal = moonMidnightRotation;
        //    currentMoonGoal = moonSunriseRotation;
        //}

        // Update text
        text.text = FormatTime(currentTime);
        percText.text = string.Format("{0:0}%", nightPercentage * 100f);
        
        //Update time of day settings
        SetColorOfCelestialBody(nightPercentage);
        RotateCelestialBodies(nightPercentage);
        
        if (!go) { return; }

        // Time progresses over time
        if (currentTime < nightLengthRealSeconds)
        {
            currentTime += numGameSecPerRealSec * Time.deltaTime;
        }

        nightPercentage = Mathf.Min(currentTime / nightLengthRealSeconds, 1f);
    }

    //private float GetMoonPercent(float realPercent)
    //{
    //    float moonStart = nightPerc;
    //    float moonMid = .5f;
    //    float moonEnd = dawnPerc;

    //    if 20, return 0
    //    if 50, return 0, switch goal
    //    if 35, return 50
    //    if 65, reutrn 50


    //    if (realPercent < .5)
    //    {
    //        return (.5f - moonStart);
    //    }
    //}

    // Private Methods

    private TimeSegment DetermineTime(float percentage)
    {
        if (percentage == 0f)
        {
            return TimeSegment.Sunset;
        }
        else if(percentage > 0f && percentage < nightPerc)
        {
            return TimeSegment.Dusk;
        }
        else if (percentage >= nightPerc && percentage < dawnPerc)
        {
            return TimeSegment.Night;
        }
        else if (percentage >= dawnPerc && percentage < 1f)
        {
            return TimeSegment.Dawn;
        }
        else if(percentage == 1f)
        {
            return TimeSegment.Sunrise;
        }
        else
        {
            Debug.LogError("Invalid progression of night: " + percentage);
            return TimeSegment.None;
        }
    }

    private void SetColorOfCelestialBody(float percentage)
    {
        //currentSky.color = currentGradient.Evaluate(percentage);
        currentCelestialBodyLightSource.color = currentGradient.Evaluate(percentage);
    }

    private void RotateCelestialBodies(float percentage)
    {
        float moonPerc = (percentage / (.5f - nightPerc));

        if (percentage == .5f)
        {
            print("half");
        }


        if(percentage == .5f)
        {
            print("Moon descending");
            lastGoalV = moonMidRotation;
            moonGoalV = moonEndRotation;
        }

        if (moon.transform.rotation == currentMoonGoal)
        {
            print("moon goal met");
            lastMoonGoal = currentMoonGoal;
            currentMoonGoal = moonSunriseRotation;
            //moonRising = false;
        }
        if(isMoonOut)
        {
            moon.transform.rotation = Quaternion.Lerp(Quaternion.Euler(lastGoalV), Quaternion.Euler(moonGoalV), moonPerc);
        }

        //moon.transform.rotation = Quaternion.Lerp(lastMoonGoal, currentMoonGoal, moonPerc);


        sun.transform.rotation = Quaternion.Lerp(sunSunsetRotation, sunSunriseRotation, percentage);

        //if(sun.transform.rotation.y < Mathf.Sin(-6))
        //{
        //    print("less than -6 degrees");
        //}

        //if(moon.transform.rotation.y < 0f)
        //{
        //    print("Moon under horizon");
        //}
        //if (sun.transform.rotation.y < 0f)
        //{
        //    print("Sun under horizon");
        //}
    }

    private void ChangeTimeOfNightSettings(string timeOfDay)
    {
        if(timeOfDay == "night")
        {
            //moonRising = true;
            isMoonOut = true;
            ChangeDominantLightSource(moon);
            ChangeDominantSky(night);
            currentGradient = moonGradient;

        }
        else if (timeOfDay == "day")
        {
            isMoonOut = false;
            ChangeDominantLightSource(sun);
            ChangeDominantSky(day);
            currentGradient = sunGradient;
        }
        else
        {
            Debug.LogError("Time of day invalid");
        }
    }
    private void ChangeDominantSky(Material newSky)
    {
        //print("Changing sky to " + newSky);
        currentSky = newSky;
        RenderSettings.skybox = newSky;
    }

    private void ChangeDominantLightSource(Light newLightSource)
    {
        //print("Changing light to " + newLightSource);
        if(currentCelestialBodyLightSource != null)
        {
            currentCelestialBodyLightSource.gameObject.SetActive(false);
        }
        currentCelestialBodyLightSource = newLightSource;
        currentCelestialBodyLightSource.gameObject.SetActive(true);
        RenderSettings.sun = newLightSource;
    }

    private string FormatTime(float currentTime)
    {
        float timeSinceLevelBegan = (currentTime / 60);
        float minutes = startHour + timeSinceLevelBegan;

        float seconds = currentTime % 60;

        string m = "pm";
        if (minutes > 12)
        {
            m = "am";
            minutes = Mathf.Abs(12 - minutes);
        }
        currentHour = minutes;
        return string.Format("{0:00}:{1:00} {2}", minutes, seconds, m);
    }

}
