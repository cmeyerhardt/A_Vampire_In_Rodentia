using UnityEngine;
using UnityEngine.Events;

public class Stamina : MonoBehaviour
{
    public FloatEvent newStaminaValueEvent;
    public UnityEvent staminaDepletedEvent;

    [Header("State")]
    [SerializeField] float currentStamina = 100f;
    [SerializeField] float maxStamina = 300f;
    float staminaDeltaInterval = 2.5f; //for display and update of values
    
    [Header("Debug")]
    [SerializeField] Color displayColor = Color.green;
    [SerializeField] FloatingTextSpawner textSpawner = null;

    // Private
    float incrementCounter = 1f;
    
    private void Awake()
    {
        //currentStamina = maxStamina;
    }

    private void Start()
    {
        newStaminaValueEvent.Invoke(GetStaminaPerc());
    }

    void Update()
    {
        if (incrementCounter >= staminaDeltaInterval)
        {
            incrementCounter = 0f;
            ModifyStamina(GetStaminaModifier());
        }
        else
        {
            incrementCounter += Time.deltaTime;
        }
    }

    public float GetStaminaValue()
    {
        return currentStamina;
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
            
            string changeString = change.ToString();
            if(change > 0f)
            {
                changeString = "+" + changeString;
            }
            textSpawner.SpawnText(change.ToString(), displayColor, true);
        }
    }
     
    public float GetStaminaModifier()
    {
        float delta = 0f;
        //foreach (IStamina provider in GetComponents<IStamina>())
        //{
        //    delta += provider.GetDeltaModifier();
        //}
        return delta;
    }
}
