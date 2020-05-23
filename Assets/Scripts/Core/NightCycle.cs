using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TimeSegment { None, Sunset, Dusk, Night, Dawn, Sunrise }

public class NightCycle : MonoBehaviour
{
    public TimeEvent timeEvent;
    [Header("Track Values")]
    public TimeSegment currentTimeSegment = TimeSegment.Sunset;
    [SerializeField] [Range(0, 1.2f)] float nightPercentage = 0f;
    [SerializeField] float currentTime = 0f;
    public float currentHour = 0f;
    public float numGameSecPerRealSec = 0f;
    public bool daytime = true;

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
    [SerializeField] Quaternion sunSunsetRotation;
    [SerializeField] Quaternion sunDuskRotation;
    [SerializeField] Quaternion sunMidnightRotation;
    [SerializeField] Quaternion sunDawnRotation;
    [SerializeField] Quaternion sunSunriseRotation;
    Quaternion currentSunGoal;
    Quaternion lastSunGoal;


    [Header("Moon")]
    //public bool moonIsOut = false;
    [SerializeField] Gradient moonGradient = null;
    [SerializeField] Light moon = null;
    [SerializeField] Material night = null;
    //[SerializeField] float switchToMoonPerc = .2f;
    [SerializeField] Quaternion moonSunsetRotation;
    [SerializeField] Quaternion moonDuskRotation;
    [SerializeField] Quaternion moonMidnightRotation;
    [SerializeField] Quaternion moonDawnRotation;
    [SerializeField] Quaternion moonSunriseRotation;
    Quaternion currentMoonGoal;
    Quaternion lastMoonGoal;

    //bool moonRising = true;

    [SerializeField] Vector3 sunStartRotation = new Vector3(-5f, 0f, 0f);
    [SerializeField] Vector3 sunEndRotation = new Vector3(175f, 0f, 0f); //sunrise

    //float totalSunDegrees = 0f;

    //[SerializeField] Vector3 moonStartRotation = new Vector3();
    //[SerializeField] Vector3 moonMidRotation = new Vector3();
    //[SerializeField] Vector3 moonEndRotation = new Vector3();
    //[SerializeField] Vector3 moonGoalV = new Vector3();
    //[SerializeField] Vector3 lastGoalV = new Vector3();
    
    [SerializeField] float nightPerc = .2f;
    [SerializeField] float dawnPerc = .8f;

    //bool morning = false;
    public bool go = false;
    public bool triggerLose = false;

    //[SerializeField] TimeSettings[] timeSettings = null;

    [Header("Reference")]
    [SerializeField] TextMeshProUGUI text = null;
    [SerializeField] TextMeshProUGUI percText = null;

    //Animator animator = null;

    private void Awake()
    {
        //animator = GetComponent<Animator>();
        //animator.SetFloat("duration", nightLengthRealSeconds/10f);
        //animator.SetFloat("duration", nightLengthRealSeconds/10f);
        //animator.Play("CelestialBodiesAnimation", 0, nightLengthRealSeconds);
        //animator.speed = 10f/nightLengthRealSeconds;
    }

    void Start()
    {
        nightPercentage = 0f;
        currentTime = 0f;

        lastMoonGoal = moonSunsetRotation;
        currentMoonGoal = moonMidnightRotation;

        lastSunGoal = sunSunsetRotation;
        currentSunGoal = sunMidnightRotation;

        //thresholds: sun and moon
        // sunset       -
        // dusk         
        // midnight     
        // dawn     
        // sunrise      

        sun.transform.rotation = sunSunsetRotation;
        moon.transform.rotation = moonSunsetRotation;

        numGameSecPerRealSec = (nightLengthGameHours * 60f) / nightLengthRealSeconds;
        SetTimeToDay();

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
                    break;
                case TimeSegment.Dawn:
                    break;
                case TimeSegment.Sunrise:
                    if (triggerLose)
                    {
                        GetComponent<LevelManager>().GameTimeDone();
                    }
                    break;
            }
        }

        if(daytime && sun.transform.rotation.x < .97f)
        {
            SetTimeToNight();
            timeEvent.Invoke(TimeSegment.Night);
            //print("sun greater than 200");
        }
        else if (!daytime && moon.transform.rotation.x > .1f)
        {
            print("daytime again");
            SetTimeToDay();
            timeEvent.Invoke(TimeSegment.Dawn);
        }
        //if(moon.transform.rotation.x > 200f)
        //{
        //    print("moon greater than 200");
        //}
        //else
        //{
        //    print(moon.transform.rotation.x);
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

        nightPercentage = Mathf.Min(currentTime / nightLengthRealSeconds, 1.1f);
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
        else if(percentage >= 1f)
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
        float adjustedPercentage = percentage * 2f;


        if (percentage == .5f)
        {
            print("half");
        }

        if (percentage >= .5f)
        {
            adjustedPercentage = (percentage - .5f) * 2f;

            //lastMoonGoal = moonMidnightRotation;
            //currentMoonGoal = moonSunriseRotation;

            moon.transform.rotation = moonMidnightRotation;
            sun.transform.rotation = sunMidnightRotation;

            lastMoonGoal = moonMidnightRotation;
            currentMoonGoal = moonSunriseRotation;
            lastSunGoal = sunMidnightRotation;
            currentSunGoal = sunSunriseRotation;
        }

        if (daytime && nightPerc < .5f)
        {
            //dusk
            sun.transform.rotation = Quaternion.Lerp(sunSunriseRotation, sunMidnightRotation, adjustedPercentage);
        }
        else if (daytime && nightPerc > .5f)
        {
            //dawn

        }
        else if (!daytime)
        {
            //lerp moon from dusk to dawn rotation

        }

        //LerpMoon(adjustedPercentage);
        sun.transform.rotation = Quaternion.Lerp(lastSunGoal, currentSunGoal, adjustedPercentage);
        moon.transform.rotation = Quaternion.Lerp(lastMoonGoal, currentMoonGoal, adjustedPercentage);

        transform.Rotate(Time.deltaTime, 0, 0);

        //if(moon.transform.rotation < )

        //if (moon.transform.rotation == currentMoonGoal)
        //{
        //    print("moon goal met");
        //    lastMoonGoal = currentMoonGoal;
        //    currentMoonGoal = moonSunriseRotation;
        //    //moonRising = false;
        //}
        //if(isMoonOut)
        //{
        //    //moon.transform.rotation = Quaternion.Lerp(Quaternion.Euler(lastGoalV), Quaternion.Euler(moonGoalV), moonPerc);
        //    //moon.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Lerp(moonStartRotation.x, -moonEndRotation.x, nightPercentage), 0f, 0f));
        //    moon.transform.rotation = Quaternion.Lerp(lastMoonGoal, currentMoonGoal, adjustedPercentage);
        //}
        //else
        //{
        //    sun.transform.rotation = Quaternion.Lerp(lastSunGoal, currentSunGoal, adjustedPercentage);

        //}

        //moon.transform.rotation = Quaternion.Lerp(lastMoonGoal, currentMoonGoal, moonPerc);



        //sun.transform.rotation = Quaternion.Euler(Vector3.Lerp(sunStartRotation, sunEndRotation, nightPercentage));


        //Vector3 sunRotateThisFrame = new Vector3(sun.transform.rotation.x, 0f, 0f);
        //sunRotateThisFrame.x = totalSunDegrees * nightPercentage;

        //sun.transform.Rotate(sunRotateThisFrame * Time.deltaTime, Space.Self);

        //sun.transform.rotation = Quaternion.Euler(sunRotateThisFrame);

    }

    private void ChangeTimeOfNightSettings(string timeOfDay)
    {

        if(timeOfDay == "night")
        {
            daytime = false;
            ChangeDominantLightSource(moon);
            ChangeDominantSky(night);
            currentGradient = moonGradient;

        }
        else if (timeOfDay == "day")
        {
            daytime = true;
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
