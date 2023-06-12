using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public GameObject[] eventSequence;
    public GameObject visualEvent1, Hannah, Lauren;
    public AudioClip laugh;
    [HideInInspector] public int currentEvent;
    [HideInInspector] public int nextInSequence;


    public static EventManager instance;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }


        for (int i = 0; i < eventSequence.Length; i++)
        {
            eventSequence[i].SetActive(false);
        }
        eventSequence[0].SetActive(true);
        currentEvent = 0;
        Hannah.SetActive(false);
        Lauren.SetActive(false);
        visualEvent1.SetActive(false);
    }

    public void NextEvent()
    {
        for (int i = 0; i < eventSequence.Length; i++)
        {
            eventSequence[i].SetActive(false);
        }
        currentEvent++;
        nextInSequence++;
        eventSequence[nextInSequence].SetActive(true);
    }
}
