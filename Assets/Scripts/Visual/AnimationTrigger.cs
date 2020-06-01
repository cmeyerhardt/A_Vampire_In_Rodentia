using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    [SerializeField] float timeToSwitchAnimation = 8f;
    [SerializeField] int[] animationStates = { 0, 6 };
    int currentAnim = 0;
    float timer = 0f;
    Animator animator = null;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(timer >= timeToSwitchAnimation)
        {
            //print("Switching state");
            animator.SetInteger("State", animationStates[currentAnim]);
            GetNextState();
            timer = 0f;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private int GetNextState()
    {
        currentAnim++;
        if(currentAnim >= animationStates.Length)
        {
            currentAnim = 0;
        }
        return currentAnim;
    }
}
