using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTransform : MonoBehaviour
{
    [Header("Activated when player is on a surface with no navmesh")]
    float walkingSpeed = 10f;
    float turnSpeed = 30f;
    PlayerController player = null;

    public void Awake()
    {
        player = GetComponent<PlayerController>();
        walkingSpeed = player.baseMovementSpeed;
        turnSpeed = player.turnSpeed;
    }

    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
        {
            Vector3 direction = player.DetermineDirectionOfMovement(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
            player.model.forward = Vector3.Slerp(player.model.transform.forward, direction, turnSpeed * Time.deltaTime);
            transform.position += direction * walkingSpeed * Time.deltaTime;
        }
    }
}
