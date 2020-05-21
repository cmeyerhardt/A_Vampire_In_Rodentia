using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToLocation : AIBehaviour
{
    [Header("Go To Location--")]
    private Vector3? nullableLocation = null;
    public Vector3 location = new Vector3();
    [SerializeField] public string locationString; //(x,y,z)
    public float range = 0f;

    public float distanceToLocation;

    public new void Awake()
    {
        base.Awake();
    }

    public new void OnEnable()
    {
        base.OnEnable();

        range = Mathf.Max(ai.GetStoppingDistance(), range);

        if (nullableLocation == null)
        {
            if (FindLocation(locationString))
            {
                //print(location);
                ai.MoveToDestination(location, 1f);
            }
            else
            {
                print("Unsuccessful, going to current position");
                location = transform.position;
            }
        }
    }

    public new void Start()
    {
        base.Start();
    }

    public new void Update()
    {
        base.Update();
        if(ai.IsInRange(location, range))
        {
            doneEvent.Invoke(this);
        }
    }

    public bool FindLocation(string locationString)
    {
        if(locationString == null) { Debug.Log(this.ToString() + " No Location Data"); return false; }
        if(locationString.Length <= 0) { Debug.Log(this.ToString() + " No Location Data"); return false; }
        if(locationString[0] != '(') { Debug.Log("No Parenthesis Given"); return false; }

        string _string = "";
        float[] _floats = new float[3];
        int _vectorIndex = 0;

        for (int i = 1; i < locationString.Length; i++)
        {
            if (locationString[i] != ')')
            {
                if (locationString[i] != ',')
                {
                    _string += locationString[i];
                }
                else
                {
                    _floats[_vectorIndex] = float.Parse(_string);
                    //print("Parsing " + _string + " to " + _floats[_vectorIndex]);
                    _string = "";
                    _vectorIndex++;
                }
            }
            else
            {
                _floats[_vectorIndex] = float.Parse(_string);
                //print("Parsing " + _string + " to " + _floats[_vectorIndex]);

                if (_vectorIndex == 2)
                {
                    location = new Vector3(_floats[0], _floats[1], _floats[2]);
                    return true;
                }
                else
                {
                    Debug.Log("Unintended Result: " + _string + " " + _floats.Length);
                    return false;
                }
            }
        }
        return false;
    }
}
