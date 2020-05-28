using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public FloatEvent newHealthValueEvent;
    public StringEvent deathEvent;

    [Header("State")]
    public bool isDead = false;
    [SerializeField] float currentHealth = 100f;
    [SerializeField] float maxHealth = 100f;
    
    [Header("Debug")]
    [SerializeField] Color displayColor = Color.red;
    [SerializeField] FloatingTextSpawner textSpawner = null;

    private void Awake()
    {
        currentHealth = maxHealth;
        newHealthValueEvent.Invoke(GetHealthPerc());
    }

    public void ModifyHealth(float change)
    {
        if (change != 0f)
        {
            currentHealth = Mathf.Clamp(currentHealth + change, 0f, maxHealth);

            // update display
            newHealthValueEvent.Invoke(GetHealthPerc());

            if (currentHealth <= 0f)
            {
                isDead = true;
                deathEvent.Invoke(GetComponent<SavableObject>().GetUniqueIdentifier());
            }
            else
            {
                string changeString = change.ToString();
                if (change > 0f)
                {
                    changeString = "+" + changeString;
                }
                textSpawner.SpawnText(changeString, displayColor, true);
            }
        }
    }

    public float GetHealthPerc()
    {
        return currentHealth / maxHealth;
    }
}
