﻿using UnityEngine;

public class Feeder : MonoBehaviour
{
    public FloatEvent feedEvent;
    
    [Header("Configure")]
    [SerializeField] [Tooltip("sec.")][Range(0f,10f)]float feedIntervalWhileLatched = 2f;
    [SerializeField] [Tooltip("sec.")][Range(0f,10f)]float timeToWaitToAvoidInturruption = 1f;
    [SerializeField] [Range(0f,10f)] float feedInturruptionCost = 5f;
    [SerializeField] [Range(0f,1f)] float healthGainRatio = .5f;
    [SerializeField] [Range(0f,1f)] float rangeToBreakFeeding = 5f;

    [Header("Audio")]
    [SerializeField] AudioClip feedingSFX = null;
    [SerializeField] [Range(0f, 1f)] float feedingSFXVolume = 5f;
    [SerializeField] public bool useSecondaryAudioSourceFeedingSound = false;



    // Cache
    bool feeding = false;
    float feedCounter = 0f;
    float totalTimeFeeding = 0f;

    FeedingVictim currentVictim = null;
    Health victimHealth = null;

    Health health = null;
    Stamina stamina = null;

    FloatingTextSpawner textSpawner = null;
    PlayerController playerController = null;

    private void Awake()
    {
        health = GetComponent<Health>();
        stamina = GetComponent<Stamina>();
        textSpawner = GetComponentInChildren<FloatingTextSpawner>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        //feedEvent.AddListener(GetComponent<Stamina>().ModifyStamina);
    }

    private void Update()
    {
        if(feeding)
        {
            if (currentVictim != null)
            {
                if (Vector3.Distance(transform.position, currentVictim.transform.position) <= rangeToBreakFeeding && !victimHealth.isDead)
                {
                    feedCounter += Time.deltaTime;
                    totalTimeFeeding += Time.deltaTime;
                    if (feedCounter >= feedIntervalWhileLatched)
                    {
                        feedCounter = 0f;
                        Feed(currentVictim.GetFedValue());
                    }
                }
                else
                {
                    CancelFeeding();
                }

            }
            else
            {
                CancelFeeding();
            }
        }

    }
    
    public void AssignVictim(FeedingVictim victim)
    {
        currentVictim = victim;
        victimHealth = currentVictim.GetComponent<Health>();
        totalTimeFeeding = 0f;
        feeding = true;
        playerController.PlaySoundEffect(feedingSFX, feedingSFXVolume, useSecondaryAudioSourceFeedingSound);
        //Trigger Feeding Animation
    }

    private void Feed(float value)
    {
        playerController.PlaySoundEffect(feedingSFX, feedingSFXVolume, useSecondaryAudioSourceFeedingSound);
        currentVictim.FeedOn();
        health.ModifyHealth(value);
        stamina.ModifyStamina(value * healthGainRatio);
    }

    public void CancelFeeding()
    {
        if(feeding)
        {
            playerController.primaryAudioSource.Stop();
            //Check if cancel early
            if (totalTimeFeeding < timeToWaitToAvoidInturruption)
            {
                //textSpawner.SpawnText("Penalty", new Color(1f, .5f, 0f));
                stamina.ModifyStamina(-feedInturruptionCost);
            }

            if (currentVictim != null)
            {
                currentVictim.CancelBeingFedOn();
            }

            currentVictim = null;
            GetComponent<PlayerController>().SetState(PlayerState.Idle);
        }

        feeding = false;
        feedCounter = 0f;
        totalTimeFeeding = 0f;
    }
}



