using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpBehaviour : MonoBehaviour
{
    public enum ObjectType
    {
        BIBERON, TRAIN, KEY, PLANK
    }
    public ObjectType type;
    [HideInInspector] public bool interacted;


    // Update is called once per frame
    void Update()
    {
        if (interacted)
        {
            if (type == ObjectType.BIBERON)
            {
                GameManager.instance.hasBiberon = true;
                GameManager.instance.NotifyEvent("Found Food", "Give Gordon his food");
            }
            else if (type == ObjectType.TRAIN)
            {
                GameManager.instance.hasTrain = true;
                GameManager.instance.NotifyEvent("Found Nina's Train", "Give the train to Nina");

            }
            else if (type == ObjectType.PLANK)
            {
                GameManager.instance.hasPlank = true;
                GameManager.instance.NotifyEvent("Found Floor Plank", "Explore the Second Floor");
            }
            else
            {
                GameManager.instance.hasKey = true;
                GameManager.instance.NotifyEvent("Found Second Floor Key", "Find the Shortcut");

            }
            interacted = false;
            this.gameObject.SetActive(false);
        }
    }
}
