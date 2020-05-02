using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementFixedCamera : MonoBehaviour
{
    [SerializeField] [Range(0f, 50f)] float moveSpeed = 10f;
    [SerializeField] [Range(0f, 360f)] float turnSpeed = 30f;
    NavMeshAgent navMeshAgent = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            navMeshAgent.Move(Vector3.forward * moveSpeed * Time.deltaTime);
            //navMeshAgent.SetDestination(transform.position + Vector3.forward * moveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            navMeshAgent.Move(-Vector3.forward * moveSpeed * Time.deltaTime);
            //navMeshAgent.SetDestination(transform.position - Vector3.forward * moveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            navMeshAgent.Move(-Vector3.right * moveSpeed * Time.deltaTime);
            //navMeshAgent.SetDestination(transform.position - Vector3.right * moveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            navMeshAgent.Move(Vector3.right * moveSpeed * Time.deltaTime);
            //navMeshAgent.SetDestination(transform.position + Vector3.right * moveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.down * turnSpeed * Time.deltaTime);
            //transform.RotateAround(transform.position, Vector3.up, 45f);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
            //transform.RotateAround(transform.position, Vector3.down, 45f);
        }
    }
}
