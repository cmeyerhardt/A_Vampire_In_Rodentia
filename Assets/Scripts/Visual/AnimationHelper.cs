using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    Character character = null;
    Attack attack = null;
    Animator animator = null;

    private void Awake()
    {
        character = GetComponentInParent<Character>();
        attack = GetComponentInParent<Attack>();
        animator = GetComponent<Animator>();
    }

    public void AnimationEventMakeFootstepsSoundEffect()
    {
        if(character != null)
        {
            character.AnimationEventMakeFootstepsSoundEffect();
        }
    }

    public void AnimationEventResetAttack()
    {
        if(character != null)
        {
            character.AnimationEventResetAttack();
        }
        else if(animator != null)
        {
            animator.SetInteger("State", 0);
        }
    }

    public void AnimationEventHitSound()
    {
        if (character != null)
        {
            character.AnimationEventHit();
        }
    }

    public void AnimationEventHitDamage()
    {
        if(attack != null)
        {
            attack.AnimationEventHitDamage();
        }
    }
}
