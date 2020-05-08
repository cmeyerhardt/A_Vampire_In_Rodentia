using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stamina : MonoBehaviour
{
    public FloatEvent newStaminaValueEvent;
    [HideInInspector] public UnityEvent deathEvent;

    [Header("Stamina")]
    [SerializeField] float currentStamina = 100f;
    [SerializeField] float maxStamina = 300f;
    [SerializeField] float staminaDelta = -1f;
    [SerializeField] float sprintStaminaDelta = -2f;
    [SerializeField] float staminaDeltaInterval = 5f;
    float incrementCounter = 1f;
    
    [Header("Display")]
    [SerializeField] Color displayColor = Color.green;

    [Header("Debug")]
    [SerializeField] FloatingTextSpawner textSpawner = null;

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    private void Start()
    {
        newStaminaValueEvent.Invoke(currentStamina / maxStamina);
    }

    void Update()
    {
        if (incrementCounter >= staminaDeltaInterval)
        {
            incrementCounter = 0f;
            float delta = staminaDelta;
            if(Input.GetKey(KeyCode.LeftShift))
            {
                delta += sprintStaminaDelta;
            }
            ModifyStamina(staminaDelta);
        }
        else
        {
            incrementCounter += Time.deltaTime;
        }
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
            textSpawner.SpawnText(change.ToString(), displayColor);

            if (currentStamina == 0f)
            {
                deathEvent.Invoke();
            }
        }
    }
}
