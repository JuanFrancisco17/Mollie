using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puke : MonoBehaviour
{
    public bool detected;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PukeBehavior.instance.detected = true;
            BasicCharacterStateMachine.instance.moveSpeed = 6;
            HannahStateManager.instance.target = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            BasicCharacterStateMachine.instance.moveSpeed = 9;
            PukeBehavior.instance.detected = false;
        }
    }
}
