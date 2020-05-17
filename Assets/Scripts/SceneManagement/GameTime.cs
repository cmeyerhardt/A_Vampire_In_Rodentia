using TMPro;
using UnityEngine;

public class GameTime : MonoBehaviour
{
    [SerializeField] float nightDuration = 350f;
    [SerializeField] float timeLeft = 0f;
    [SerializeField] TextMeshProUGUI text = null;

    void Start()
    {
        timeLeft = nightDuration;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;

        text.text = string.Format("{0:0}", timeLeft);
        if (timeLeft <= 0f)
        {
            print("It is now morning");
        }
    }
}
