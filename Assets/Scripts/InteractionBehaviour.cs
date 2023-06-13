using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionBehaviour : MonoBehaviour
{
    public enum ObjectType
    {
        GORDON, NINA, DOOR, PLANK
    }
    public ObjectType type;
    [HideInInspector] public bool interacted;


    // Update is called once per frame
    void Update()
    {
        if (interacted)
        {
            if (type == ObjectType.GORDON)
            {
                if (!GameManager.instance.hasBiberon)
                {
                    EventManager.instance.eventSequence[0].SetActive(true);
                    DialogueUI.instance.ShowDialogue(0);
                    GameManager.instance.NotifyEvent("New Quest", "Find food for Gordon");
                }
                else if (GameManager.instance.biberonGiven)
                {
                    DialogueUI.instance.ShowDialogue(1);
                }
                else
                {
                    GameManager.instance.biberonGiven = true;
                    FatRoll.instance.MoveGordon();
                    SoundManager.instance.PlayGordon();
                    GameManager.instance.NotifyEvent("Gordons's Quest completed", "Explore the Second Floor");
                }
            }
            else if (type == ObjectType.NINA)
            {
                if (!GameManager.instance.hasTrain)
                {
                    GameManager.instance.NotifyEvent("New Quest", "Find Nina's Train");
                    DialogueUI.instance.ShowDialogue(2);
                }
                else if (GameManager.instance.trainGiven)
                {
                    DialogueUI.instance.ShowDialogue(4);
                }
                else
                {
                    DialogueUI.instance.ShowDialogue(3);
                    GameManager.instance.ShowKey();
                    GameManager.instance.trainGiven = true;
                    GameManager.instance.NotifyEvent("Nina's Quest completed", "Find the shortcut");
                }
            }
            else if (type == ObjectType.DOOR)
            {
                if (GameManager.instance.hasKey)
                {
                    GameManager.instance.NotifyEvent("Shortcut Unlocked", "Explore the Second Floor");
                    SoundManager.instance.PlayOneshot(0, GameManager.instance.doorClip);
                    LeanTween.rotate(this.gameObject, new Vector3(-90, 0, 87.5f), 5.5f).setEase(LeanTweenType.easeOutCirc);
                }
            }
            else if (type == ObjectType.PLANK)
            {
                if (GameManager.instance.hasPlank)
                {
                    GameManager.instance.ShowPlank();
                    GameManager.instance.NotifyEvent("Plank placed", "Go to the last class");
                }
            }
            interacted = false;
        }
    }
}
