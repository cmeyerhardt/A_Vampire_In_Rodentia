using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    Character character = null;

    private void Awake()
    {
        character = GetComponentInParent<Character>();
    }

    public void AnimationEventMakeFootstepsSoundEffect()
    {
        character.AnimationEventMakeFootstepsSoundEffect();
    }

    public void AnimationEventResetAttack()
    {
        character.AnimationEventResetAttack();
    }

    public void AnimationEventHitSound()
    {
        character.AnimationEventHit();
    }
}
