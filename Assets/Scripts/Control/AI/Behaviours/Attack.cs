using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : AIBehaviour
{
    [SerializeField] float maxChaseDistance = 40f;
    [Header("Attack")]
    [SerializeField] float attackRange = 4f; //melee ~3.5f
    [SerializeField] float attackDamage = 3f;
    [SerializeField] float attackInterval = 3f;

    [Header("Stun")]
    [SerializeField] float stunRange = 5f;
    [SerializeField] float stunInterval = 5f;
    [SerializeField] float stunDuration = 3f;

    // Cache
    [HideInInspector] public Vector3 startLocation = new Vector3();
    float globalCooldown = 1f;
    float attackCounter = Mathf.Infinity;
    float stunCounter = Mathf.Infinity;
    Health player = null;

    private new void OnEnable()
    {
        base.OnEnable();
        startLocation = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        if (player != null && !ai.IsInRange(player.transform.position, attackRange))
        {
            ai.MoveToDestination(player.transform.position, 1f);
        }

        globalCooldown = 1f;
        attackCounter = attackInterval;
        stunCounter = stunInterval;
    }

    public new void Update()
    {
        base.Update();
        if (ai == null) { Debug.LogError("No AI Reference. Attack Behaviour: " + gameObject.name); return; }
        if ((transform.position - startLocation).magnitude > maxChaseDistance || player == null)
        {
            doneEvent.Invoke(this);
        }
        else
        {
            if (globalCooldown > 0f)
            {
                globalCooldown -= Time.deltaTime;
            }
            else
            {
                if (ai.IsInRange(player.transform.position, attackRange))
                {
                    if (attackCounter >= attackInterval)
                    {
                        attackCounter = 0f;
                        globalCooldown = 1f;
                        player.ModifyHealth(-attackDamage);
                    }
                    else
                    {
                        attackCounter += Time.deltaTime;
                    }
                }
                else
                {
                    ai.MoveToDestination(player.transform.position, 1f);
                }
                if (ai.IsInRange(player.transform.position, stunRange))
                {
                    if (stunCounter >= stunInterval)
                    {
                        stunCounter = 0f;
                        globalCooldown = 1f;
                        player.GetComponent<Character>().Stun(stunDuration);
                    }
                    else
                    {
                        stunCounter += Time.deltaTime;
                    }
                }
            }
        }
    }
}
