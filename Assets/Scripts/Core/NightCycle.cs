using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TimeSegment { None, Sunset, Dusk, Night, Dawn, Sunrise, Day }

public class NightCycle : MonoBehaviour
{
    public TimeEvent timeEvent;
    [Header("Current Time")]
    public TimeSegment currentTimeSegment = TimeSegment.Sunset;
    public TimeSegment lastTimeSegment = TimeSegment.Sunset;
    [SerializeField] [Range(0, 2f)] float nightPercentage = 0f;
    [SerializeField] float currentTime = 0f;
    public float currentHour = 0f;


    [Header("Current Time Settings")]
    public Material currentSky = null;
    public Light currentCelestialBodyLightSource = null;
    public Gradient currentGradient = null;
    public float numGameSecPerRealSec = 0f;

    [Header("Configure Values")]
    [SerializeField] float nightLengthRealSeconds = 100f;
    [SerializeField] int nightLengthGameHours = 8;
    [SerializeField] int startHour = 8;
    //[SerializeField] int endHour = 6;
    [SerializeField] float nightBeginPercentage = .2f;
    [SerializeField] float dawnBeginPercentage = .8f;
    
    [Header("Sun")]
    [SerializeField] AnimationCurve sunLightIntensity = new AnimationCurve();
    [SerializeField] Gradient sunGradient = null;
    [SerializeField] Light sun = null;
    [SerializeField] Material day = null;
    [SerializeField] Quaternion sunSunsetRotation = new Quaternion();
    [SerializeField] Quaternion sunMidnightRotation = new Quaternion();
    [SerializeField] Quaternion sunSunriseRotation = new Quaternion();
    Quaternion currentSunGoal = new Quaternion();
    Quaternion lastSunGoal = new Quaternion();

    [Header("Moon")]
    [SerializeField] AnimationCurve moonLightIntensity = new AnimationCurve();
    [SerializeField] Gradient moonGradient = null;
    [SerializeField] Light moon = null;
    [SerializeField] Material night = null;
    [SerializeField] Quaternion moonSunsetRotation = new Quaternion();
    //[SerializeField] Quaternion moonMidnightRotation = new Quaternion();
    [SerializeField] Quaternion moonSunriseRotation = new Quaternion();
    //Quaternion currentMoonGoal = new Quaternion();
    //Quaternion lastMoonGoal = new Quaternion();

    //Dictionary<TimeSegment, Quaternion> moonRotations = new Dictionary<TimeSegment, Quaternion>();
    //Dictionary<TimeSegment, Quaternion> sunRotations = new Dictionary<TimeSegment, Quaternion>();

    [SerializeField] Gradient timeImageGradient = new Gradient();
    [SerializeField] Image timeImage = null;
    [Header("Debug")]
    //bool morning = false;
    public bool go = false;
    public bool triggerLose = false;

    [Header("Reference")]
    [SerializeField] TextMeshProUGUI timeText = null;
    [SerializeField] TextMeshProUGUI percText = null;

    void Start()
    {
        nightPercentage = 0f;
        currentTime = 0f;

        //lastMoonGoal = moonSunsetRotation;
        //currentMoonGoal = moonMidnightRotation;

        lastSunGoal = sunSunsetRotation;
        currentSunGoal = sunMidnightRotation;   

        sun.transform.rotation = sunSunsetRotation;
        //moon.transform.rotation = moonSunsetRotation;

        numGameSecPerRealSec = (nightLengthGameHours * 60f) / nightLengthRealSeconds;
        SetTimeToDay();

    }

    public void UpdateTimeImage(float percentage)
    {
        timeImage.fillAmount = Mathf.Clamp(percentage, 0f, 1f);
        timeImage.color = timeImageGradient.Evaluate(percentage);
    }

    void Update()
    {
        TimeSegment time = DetermineTime(nightPercentage);
        if (time != currentTimeSegment)
        {
            // Initialize cycle settings
            print("Time is now: " + time);
            currentTimeSegment = time;
            timeEvent.Invoke(currentTimeSegment);
            switch (currentTimeSegment)
            {
                case TimeSegment.Sunset:
                    currentTimeSegment = TimeSegment.Dusk;
                    break;
                case TimeSegment.Dusk:
                    break;
                case TimeSegment.Night:
                    SetTimeToNight();
                    break;
                case TimeSegment.Dawn:
                    SetTimeToDay();
                    break;
                case TimeSegment.Sunrise:
                    //print("sunrise");
                    if (triggerLose)
                    {
                        GetComponent<LevelManager>().GameTimeDone();
                    }
                    break;
                case TimeSegment.Day:
                    break;
            }
        }

        if (nightPercentage > 1f)
        {
            if (triggerLose)
            {
                triggerLose = false;
                GetComponent<LevelManager>().GameTimeDone();
            }
        }

        // Update text
        timeText.text = FormatTime(currentTime);
        percText.text = string.Format("{0:0}%", nightPercentage * 100f);
        UpdateTimeImage(nightPercentage);

        //Update time of day settings
        SetColorOfCelestialBody(nightPercentage);
        RotateCelestialBodies(nightPercentage);
        moon.intensity = moonLightIntensity.Evaluate(nightPercentage);
        sun.intensity = sunLightIntensity.Evaluate(nightPercentage);

        if (!go) { return; }

        // Time progresses over time
        //if (currentTime < nightLengthRealSeconds)
        //{
        //    currentTime += numGameSecPerRealSec * Time.deltaTime;
        //}
        currentTime += numGameSecPerRealSec * Time.deltaTime;
        nightPercentage = currentTime / nightLengthRealSeconds;
        if(nightPercentage > 2)
        {
            currentTime = startHour;
            nightPercentage = 0f;
        }

    }

    private void SetTimeToNight()
    {
        ChangeTimeOfNightSettings("night");
    }

    private void SetTimeToDay()
    {
        ChangeTimeOfNightSettings("day");
    }

    // Private Methods

    private TimeSegment DetermineTime(float percentage)
    {
        if (percentage == 0f)
        {
            return TimeSegment.Sunset;
        }
        else if(percentage > 0f && percentage < nightBeginPercentage)
        {
            return TimeSegment.Dusk;
        }
        else if (percentage >= nightBeginPercentage && percentage < dawnBeginPercentage)
        {
            return TimeSegment.Night;
        }
        else if (percentage >= dawnBeginPercentage && percentage < 1f)
        {
            return TimeSegment.Dawn;
        }
        else if(percentage == 1f)
        {
            return TimeSegment.Sunrise;
        }
        else if(percentage > 1f)
        {
            return TimeSegment.Day;
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
        if (percentage < .5f)
        {
            //dusk
            sun.transform.rotation = Quaternion.Lerp(sunSunsetRotation, sunMidnightRotation, percentage * 2f);
        }
        else if (percentage > .5f && percentage < 1f)
        {
            //dawn
            sun.transform.rotation = Quaternion.Lerp(sunMidnightRotation, sunSunriseRotation, (percentage - .5f) *2f);
        }
        else if(percentage > 1f)
        {
            //day
            sun.transform.rotation = Quaternion.Lerp(sunSunriseRotation, sunSunsetRotation, (percentage - 1f));
        }
        else if (percentage == .5f)
        {
            sun.transform.rotation = sunMidnightRotation;
        }

        moon.transform.rotation = Quaternion.Lerp(moonSunsetRotation, moonSunriseRotation, percentage);
    }

    private void ChangeTimeOfNightSettings(string timeOfDay)
    {
        if(timeOfDay == "night")
        {
            ChangeDominantLightSource(moon);
            ChangeDominantSky(night);
            currentGradient = moonGradient;

        }
        else if (timeOfDay == "day")
        {
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
