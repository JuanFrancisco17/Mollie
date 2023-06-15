using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public GameObject[] eventSequence;
    public Rigidbody[] cajasRb;
    public float impulseCajas;
    public GameObject visualEvent1, Hannah;
    public GameObject plankPickUp;
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
        currentEvent = 0;
        Hannah.SetActive(false);
        visualEvent1.SetActive(false);
        plankPickUp.SetActive(false);
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

    [ContextMenu("assdasdasd")]
    public void TirarCajas()
    {
        for (int i = 0; i < cajasRb.Length; i++)
        {
            cajasRb[i].AddForce(-Vector3.forward * impulseCajas, ForceMode.Impulse);
        }
    }
}
