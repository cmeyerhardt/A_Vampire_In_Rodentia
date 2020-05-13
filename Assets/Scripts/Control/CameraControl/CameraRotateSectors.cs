using UnityEngine;
using System.Collections;

public class CameraRotateSectors : MonoBehaviour
{
    [SerializeField] float sectorLength = 5f;
    [SerializeField] [Range(0,50)]float rotateSpeed = 15f;
    bool rotating = false;

    void Update()
    {
        if(!rotating)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                StartCoroutine(LerpRotate(-sectorLength));
                //transform.RotateAround(transform.position, Vector3.up, 90f);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(LerpRotate(sectorLength));
                //transform.RotateAround(transform.position, Vector3.down, 90f);
            }
        }
    }

    IEnumerator LerpRotate(float goal)
    {
        rotating = true;
        float _goal = Mathf.Abs(goal);
        float progress = 0f;

        while (progress < _goal)
        {
            progress += Time.deltaTime * rotateSpeed;
            float tmp = Mathf.Min(Time.deltaTime * rotateSpeed, _goal - progress);

            transform.RotateAround(transform.position, Vector3.up * (goal / _goal), tmp);

            //Debug.Log("Progress " + progress);
            yield return null;
        }
        rotating = false;
    }
}
