using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionTracker : MonoBehaviour
{
    [SerializeField] [Range(0f,1f)] float currentMaxDetectedValue = 0f;

    [Header("Display")]
    [SerializeField] Colorizer indicator = null;
    [SerializeField] Gradient displayGradient = new Gradient();
    [SerializeField] float minYScalar = .35f;
    [SerializeField] float maxYScalar = 1f;

    Dictionary<Detector, float> detectionDict = new Dictionary<Detector, float>();

    public float GetCurrentDetectionValue()
    {
        return currentMaxDetectedValue;
    }

    public void RemoveDetectionValue(Detector detector)
    {
        if (detectionDict.ContainsKey(detector))
        {
            detectionDict.Remove(detector);
        }
        RecalculateDetection();
    }

    public void AddToDetectedValue(Detector detector)
    {
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

    public void RecalculateDetection()
    {
        float _value = 0f;
        foreach(KeyValuePair<Detector, float> detector in detectionDict)
        {
            _value += detector.Value;
        }
        currentMaxDetectedValue = Mathf.Clamp(_value, 0f, 1f);
    }

    private void Update()
    {
        if(indicator == null) { return; }

        if(currentMaxDetectedValue == 0f)
        {
            indicator.gameObject.SetActive(false);
        }
        else
        {
            indicator.gameObject.SetActive(true);
        }
        
        indicator.transform.localScale 
            = new Vector3(indicator.transform.localScale.x
                          , Mathf.Lerp(minYScalar, maxYScalar, currentMaxDetectedValue)
                          , indicator.transform.localScale.z);
        indicator.Recolor(displayGradient.Evaluate(currentMaxDetectedValue));
    }
}
