using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallEvent : MonoBehaviour
{
    public GameObject thisTrigger;
    public Rigidbody[] rbs;

    private void Start()
    {
        thisTrigger.SetActive(true);
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].isKinematic = true;
        }
    }

    public void Activate()
    {
        StartCoroutine(CRT_Activate());
        SoundManager.instance.PlayOneshot(0, GameManager.instance.debrisClip);
    }

    IEnumerator CRT_Activate()
    {
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].isKinematic = false;
        }
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].isKinematic = true;
            rbs[i].detectCollisions = false;
        }
        EventManager.instance.NextEvent();
    }
}
