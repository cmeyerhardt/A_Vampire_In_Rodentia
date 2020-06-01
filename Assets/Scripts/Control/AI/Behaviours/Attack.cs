using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : AIBehaviour
{
    [Header("Attack--")]
    [SerializeField] float maxChaseDistance = 40f;
    [SerializeField] [Range(0f, 10f)] float movementFraction = 1f;

    [Header("Attack")]
    [SerializeField] public float attackRange = 4f; //melee ~3.5f
    [SerializeField] public float attackDamage = 3f;
    [SerializeField] public float attackInterval = 3f;
    [SerializeField] public float attackVolume = 10f;

    [Header("Stun")]        
    [SerializeField] public float stunInterval = 5f;
    [SerializeField] public float stunVolume = 10f;
    [SerializeField] public float stunStaminaCost = 10f;

    [Header("Audio")]
    [SerializeField] AudioClip hitSound = null;
    [SerializeField] [Range(0f, 1f)] float hitSoundMaxVolume = 1f;
    [SerializeField] public bool useSecondaryAudioSourceAttackSound = false;

    // Cache
    float globalCooldown = 0f;
    float attackCounter = Mathf.Infinity;
    float stunCounter = Mathf.Infinity;
    [HideInInspector]  public Health player = null;
    bool attacking = false;

    public new void Awake()
    {
        base.Awake();
        globalCooldown = 0f;
        attackCounter = attackInterval;
        stunCounter = stunInterval;
    }

    public new void OnEnable()
    {
        base.OnEnable();
        //startLocation = transform.position;
        player = ai.player.GetComponent<Health>();
        if (player != null && !ai.IsInRange(player.transform.position, attackRange))
        {
            ai.MoveToDestination(player.transform.position, movementFraction);
        }
    }

    public new void Update()
    {
        if (player == null) { return; }
        if (player.isDead || ai.player.isHidden) { doneEvent.Invoke(this); return; }
        if (ai == null) { Debug.LogError("No AI Reference. Attack Behaviour: " + gameObject.name); return; }
        base.Update();

        if (ai.behaviourMap.ContainsKey("Return") && ((GoToLocation)ai.behaviourMap["Return"]).nullableLocation != null)
        {
            if (player == null || (!ai.canSeePlayer && (transform.position - (Vector3)((GoToLocation)ai.behaviourMap["Return"]).nullableLocation).magnitude > maxChaseDistance))
            {
                print("I've left my area");
                doneEvent.Invoke(this);
                //ai.currentState = NPCState.Default;
            }
        }

        if (!ai.canSeePlayer && attacking)
        {
            attacking = false;
            doneEvent.Invoke(this);
            //FaceLocation(ai.lastSeenPlayerLocation);
            //ai.MoveToDestination(ai.lastSeenPlayerLocation, 1f);
        }
        if (ai.behaviourMap.ContainsKey("GoToPlayer"))
        {
            if (!ai.canSeePlayer && !attacking && ai.IsInRange((Vector3)((GoToLocation)ai.behaviourMap["GoToPlayer"]).nullableLocation))
            {
                doneEvent.Invoke(this);
            }
        }
        if (ai.canSeePlayer && !attacking)
        {
            attacking = true;
        }
        if(attacking)
        {
            FaceLocation(player.transform.position);
            CommenceCombat();
        }
    }

    private void CommenceCombat()
    {
        if (globalCooldown > 0f)
        {
            globalCooldown -= Time.deltaTime;
        }
        else
        {
            if (ai.IsInRange(player.transform.position, ai.outgoingStunRange))
            {

                if (stunCounter >= stunInterval)
                {
                    if (ai.stamina.CheckCanAffordCost(stunStaminaCost))
                    {
                        //print("Stunning");
                        ai.StopMoving();
                        stunCounter = 0f;
                        globalCooldown = 1f;
                        ai.StunTarget(ai.player, ai.useSecondaryAudioSourceStunSound);
                        //ai.player.BecomeStunned(ai.outgoingStunDuration);
                        ai.player.makeSoundEvent.Invoke(stunVolume);
                    }
                    else
                    {
                        print(gameObject.name +" Couldn't afford to stun");
                    }
                }
                else
                {
                    stunCounter += Time.deltaTime;
                }
            }

            if (ai.IsInRange(player.transform.position, attackRange))
            {
                ai.StopMoving();
                if (attackCounter >= attackInterval)
                {

                    attackCounter = 0f;
                    globalCooldown = 1f;

                    PerformAttack(true, true);
                }
                else
                {
                    attackCounter += Time.deltaTime;
                }
            }
            else
            {
                ai.MoveToDestination(player.transform.position, movementFraction);
            }
        }
    }

    private void FaceLocation(Vector3 position)
    {
        Vector3 lookVector = position - transform.position;
        lookVector.y = transform.position.y;
        Quaternion newRotation = Quaternion.LookRotation(lookVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 1);
    }

    public virtual void PerformAttack(bool playSound, bool doAttackAnim)
    {
        AnimationEventHitDamage();
        if (doAttackAnim)
        {
            //print("Doing melee attack anim");
            ai.animator.SetInteger("State", 1);
        }

    }

    public void AnimationEventHitDamage()
    {
        player.ModifyHealth(-attackDamage);
        ai.PlaySoundEffect(hitSound, hitSoundMaxVolume, useSecondaryAudioSourceAttackSound);
        ai.player.makeSoundEvent.Invoke(attackVolume);
    }
}
