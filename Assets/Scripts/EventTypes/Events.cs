using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Events : MonoBehaviour
{
    public enum EventType
    {
        AUDIO, VISUAL, FALL, STORY
    }
    public EventType type;
    public AudioClip clip;
    public int dialogueNum;
    public VisualEvent VisualEvent;
    public FallEvent FallEvent;
    Collider coll;
    [HideInInspector] public bool eventStart;

    private void Start()
    {
        coll = gameObject.GetComponent<Collider>();

    }
    // Update is called once per frame
    void Update()
    {
        if (eventStart)
        {
            if (type == EventType.AUDIO)
            {
                SoundManager.instance.PlayOneshot(0, clip);
            }
            else if (type == EventType.VISUAL)
            {
                VisualEvent.Activate();
            }
            else if (type == EventType.FALL)
            {
                FallEvent.Activate();
            }
            else if (type == EventType.STORY)
            {
                BasicCharacterStateMachine.instance.rb.isKinematic = true;
                DialogueUI.instance.ShowDialogue(dialogueNum);
                Destroy(this.gameObject, 0.1f);
            }

            if (type == EventType.VISUAL && dialogueNum == 8)
            {
                SoundManager.instance.PlayOneshot(0, GameManager.instance.boxesClip);
                EventManager.instance.TirarCajas();
                EventManager.instance.NextEvent();
            }
            else if (type == EventType.STORY && dialogueNum == 8)
            {
                //hannah y lauren aparecen
                EventManager.instance.Hannah.SetActive(true);
                EventManager.instance.NextEvent();
            }
            else if (type == EventType.STORY && dialogueNum == 13)
            {
                //plank trigger
                EventManager.instance.plankPickUp.SetActive(true);
            }


            eventStart = false;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            eventStart = true;
            if (type != EventType.STORY || type != EventType.FALL)
            {
                StartCoroutine(CollCooldown());
            }
            EventManager.instance.currentEvent++;
        }
    }


    IEnumerator CollCooldown()
    {
        coll.enabled = false;
        yield return new WaitForSeconds(90f);
        coll.enabled = true;
    }
}
