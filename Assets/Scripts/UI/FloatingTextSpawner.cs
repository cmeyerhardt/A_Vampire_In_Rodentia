using System.Collections;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] FloatingText floatingText = null;
    float xVariation = 1f;
    //float lastSpawn;
    //float spawnDelay = .2f;
    //bool wait = false;
    //public int numRoutines = 0;

    //private void Update()
    //{
    //    if(lastSpawn < spawnDelay)
    //    {
    //        lastSpawn += Time.deltaTime;

    //    }
    //    else
    //    {
    //        wait = false;
    //    }
    //}

    public void SpawnText(string message, bool floating, Color? color = null, bool randomPosition = false)
    {
        //if(!wait)
        {
            //wait = true;
            //lastSpawn = Time.deltaTime;

            Vector3 _position = transform.position;

            if (randomPosition)
            {

                //get random x position
                float randomRangeValue = Random.Range(-xVariation, xVariation);

                //get position
                _position = new Vector3(transform.localPosition.x + randomRangeValue, transform.localPosition.y, transform.localPosition.z);
                //print("Get Random Position: " + _position);
            }

            FloatingText _newText = Instantiate(floatingText, _position, Quaternion.identity, transform);
            _newText.GenerateText(message, color);
        }
        //else
        //{
        //    StartCoroutine(WaitDelay(message, color, randomPosition));
        //}
    }

    //private IEnumerator WaitDelay(string message, Color? color = null, bool randomPosition = false)
    //{
    //    numRoutines++;
    //    while(wait)
    //    {
    //        yield return null;
    //    }
    //    SpawnText(message, color, randomPosition);
    //    numRoutines--;
    //}
}
