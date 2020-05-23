using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToLocation : AIBehaviour
{
    [Header("Go To Location--")]
    public Vector3? nullableLocation = null;
    public Vector3 location = new Vector3();
    [SerializeField] public string locationString; //(x,y,z)
    [SerializeField] public bool useV3 = false;
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
            if (FindLocation(locationString, out nullableLocation))
            {
                //print(location);
                ai.MoveToDestination((Vector3)nullableLocation, 1f);
            }
            else
            {
                //print("Unsuccessful, going to current position");
                //location = transform.position;
            }
        }

        else
        {
            ai.MoveToDestination((Vector3)nullableLocation, 1f);
        }
    }

    public new void Start()
    {
        base.Start();
    }

    public new void Update()
    {
        base.Update();
        if (nullableLocation != null)
        {
            if (ai.IsInRange((Vector3)nullableLocation, range))
            {
                doneEvent.Invoke(this);
            }
        }
        else
        {
            Debug.Log("No location found to compare distance to.");
            doneEvent.Invoke(this);
        }
    }

    public bool FindLocation(string _locationString, out Vector3? _location)
    {
        _location = null;
        if (useV3)
        {
            //nullableLocation = location;
            _location = location;
            return true;
        }
        else
        {
            if (_locationString == null) { Debug.Log(this.ToString() + " No Location Data"); return false; }
            if (_locationString.Length <= 0) { Debug.Log(this.ToString() + " No Location Data"); return false; }
            if (_locationString[0] != '(') { Debug.Log("No Parenthesis Given"); return false; }

            string _string = "";
            float[] _floats = new float[3];
            int _vectorIndex = 0;

            for (int i = 1; i < _locationString.Length; i++)
            {
                if (_locationString[i] != ')')
                {
                    if (_locationString[i] != ',')
                    {
                        _string += _locationString[i];
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
                        _location = new Vector3(_floats[0], _floats[1], _floats[2]);
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
}
