using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagChanger : MonoBehaviour
{
    public Transform Player;
    public Transform centrePos;
    float radius = 10f;
    bool onCurrentRoom;
    [SerializeField] private SphereCollider collider;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!BasicCharacterStateMachine.instance.hiding)
        {
            transform.position = Player.position;
            collider.radius = 30;
            radius = 10f;
        }
        else
        {
            transform.position = centrePos.position;
            collider.radius = radius;
            StartCoroutine(CurrentRoom());  
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "RoomFarFromPlayer")
        {
            other.gameObject.tag = "Room";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Room")
        {
            other.gameObject.tag = "RoomFarFromPlayer";
        }
    }

    IEnumerator CurrentRoom()
    {
        
        yield return new WaitForSeconds(5f);
        radius = 30;
    }
}
