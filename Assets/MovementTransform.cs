using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTransform : MonoBehaviour
{
    [SerializeField] float walkingSpeed = 10f;
    [SerializeField] float turnSpeed = 30f;


    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * walkingSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * walkingSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * walkingSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * walkingSpeed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(-Vector3.up * turnSpeed * Time.deltaTime, Space.Self);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime, Space.Self);
        }
    }
}
