using UnityEngine;
using UnityEngine.Events;

public class Sight : MonoBehaviour
{
    public BoolEvent playerDetectedEvent;
    [SerializeField] public MeshCollider sightMesh = null;
    public bool playerSighted = false;

    public void SetRange(float z)
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //print("Enter: The player is now in sight range");
            playerSighted = true;
            playerDetectedEvent.Invoke(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && !playerSighted)
        {
            //print("Stay: The player is still in sight range");
            playerDetectedEvent.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && playerSighted)
        {
            //print("Exit: I can't see the player anymore");
            playerSighted = false;
            playerDetectedEvent.Invoke(false);
        }
    }
}
