using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[System.Serializable]
public class BoolEvent : UnityEvent<bool> { }

[System.Serializable]
public class StringEvent : UnityEvent<string> { }

[System.Serializable]
public class MessageColorEvent : UnityEvent<string, Color> { }

[System.Serializable]
public class FloatColorEvent : UnityEvent<float, Color> { }

[System.Serializable]
public class TimeEvent : UnityEvent<TimeSegment> { }

