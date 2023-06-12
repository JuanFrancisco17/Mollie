using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueBox;
    public TMP_Text textLabel;
    public TypewritterEffect TypewritterEffect;
    public DialogueObject[] dialogues;
    private BasicCharacterStateMachine BasicCharacterStateMachine;

    public static DialogueUI instance;
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
    }

    // Start is called before the first frame update
    void Start()
    {
        BasicCharacterStateMachine = FindObjectOfType<BasicCharacterStateMachine>();
        OpenAndCloseDialogueBox(false);
    }

    public void ShowDialogue(int num)
    {
        StartCoroutine(StepThroughDialogue(dialogues[num]));
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        OpenAndCloseDialogueBox(true);
        BasicCharacterStateMachine.moveDirection = Vector3.zero;
        BasicCharacterStateMachine.enabled = false;
        foreach (string dialogue in dialogueObject.Dialogue)
        {
            yield return TypewritterEffect.Run(dialogue, textLabel);
            yield return new WaitUntil(() => Input.GetButtonDown("Fire1"));
        }
        BasicCharacterStateMachine.enabled = true;
        BasicCharacterStateMachine.instance.rb.isKinematic = false;
        OpenAndCloseDialogueBox(false);
    }

    public void OpenAndCloseDialogueBox(bool opened)
    {
        if (opened)
        {
            dialogueBox.SetActive(true);
        }
        else
        {
            dialogueBox.SetActive(false);
            textLabel.text = string.Empty;
        }
    }
}
