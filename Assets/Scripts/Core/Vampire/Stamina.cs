using UnityEngine;
using UnityEngine.Events;

public class Stamina : MonoBehaviour
{
    public FloatEvent newStaminaValueEvent;
    public UnityEvent staminaDepletedEvent;

    [Header("State")]
    [SerializeField] float currentStamina = 100f;
    [SerializeField] float maxStamina = 300f;

    [Header("Debug")]
    [SerializeField] Color displayColor = Color.green;
    [SerializeField] FloatingTextSpawner textSpawner = null;

    // Private
    public float incrementCounter = 0f;
    public float staminaAccumulate = 0f;
    
    private void Awake()
    {
        //currentStamina = maxStamina;
    }

    private void Start()
    {
        newStaminaValueEvent.Invoke(GetStaminaPerc());
    }

    private void Update()
    {
        if (staminaAccumulate != 0f)
        {
            if (incrementCounter >= 1f)
            {
                ShowText(staminaAccumulate);
                staminaAccumulate = 0f;
                incrementCounter = 0f;
            }
            else
            {
                incrementCounter += Time.deltaTime;
            }
        }
    }

    public float GetStaminaValue()
    {
        return currentStamina;
    }

    public bool CheckCanAffordCost(float cost)
    {
        if(currentStamina >= cost)
        {
            return true;
        }
        return false;
    }

    public float GetStaminaPerc()
    {
        return currentStamina / maxStamina;
    }
    
    public void ModifyStamina(float change)
    {
        if(change != 0f)
        {
            currentStamina = Mathf.Clamp(currentStamina + change, 0f, maxStamina);

            // update display
            newStaminaValueEvent.Invoke(currentStamina / maxStamina);

            ShowText(change);
        }
    }

    private void ShowText(float change)
    {
        if (change != 0f)
        {
            string changeString = string.Format("{0:0.0}", change);
            if(change > 0f)
            {
                changeString = "+" + changeString;
            }
            textSpawner.SpawnText(changeString, displayColor, true);
        }
    }

    public void ModifyStaminaShowOnIncrement(float change)
    {
        staminaAccumulate += change;
        currentStamina = Mathf.Clamp(currentStamina + change, 0f, maxStamina);

        // update display
        newStaminaValueEvent.Invoke(currentStamina / maxStamina);
    }
}
