using System.Collections;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] FloatingText floatingText = null;
    float lastSpawn;
    float spawnDelay = .2f;
    bool wait = false;
    public int numRoutines = 0;

    private void Update()
    {
        if(lastSpawn < spawnDelay)
        {
            lastSpawn += Time.deltaTime;

        }
        else
        {
            wait = false;
        }
    }

    public void SpawnText(string message, Color? color = null)
    {
        if(!wait)
        {
            wait = true;
            lastSpawn = Time.deltaTime;
            FloatingText newText = Instantiate(floatingText, transform.position, Quaternion.identity, transform);
            newText.GenerateText(message, color);
        }
        else
        {
            StartCoroutine(WaitDelay(message, color));
        }
    }

    private IEnumerator WaitDelay(string message, Color? color = null)
    {
        numRoutines++;
        while(wait)
        {
            yield return null;
        }
        SpawnText(message, color);
        numRoutines--;
    }
}
