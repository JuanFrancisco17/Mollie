using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HannahASM : MonoBehaviour
{
    [Header("Hannah Stats")]
    public float speed;
    [Header("Player Detection")]
    public Transform TransformL, TransformR;
    public Transform player;
    public GameObject Hannah;
    public float visionRange;
    public float visionAngle;

    [Header("Patrolling")]    
    public GameObject[] rooms;
    public float counter = 10f;
    public Transform centrePoint;
    public float range;
    public int randomRoom;

    public bool isInsideRoom;

    Animator anim;
    public NavMeshAgent agent;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }
    private void Start()
    {
        agent.autoBraking = false;
        agent.velocity = Vector3.zero;
        rooms = GameObject.FindGameObjectsWithTag("Room");
        isInsideRoom = false;
        agent.speed = speed;
        anim.SetBool("Patrolling", true);
    }

    public void Patrol()
    {
         randomRoom = Random.Range(0, rooms.Length);
        centrePoint.position = rooms[randomRoom].transform.position;
        agent.SetDestination(rooms[randomRoom].transform.position);       
    }
    public void RoomPatrol()
    {
        
        if (agent.remainingDistance <= .1f)
        {
            Vector3 point;
            if (randomPoint(centrePoint.position, range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.red, 1f);
                agent.SetDestination(point);
            }
        }
        
    }
    bool randomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 RandomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(RandomPoint, out hit, 2f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}
