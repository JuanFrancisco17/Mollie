using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaurenAIBehaviour : MonoBehaviour
{
    public Transform[] rooms;
    public float hearingRange = 5f;
    public float maxDistance = 20;
    public LayerMask targetLayer;
    public LayerMask wallLayer;
    [SerializeField] bool foundPlayer = false;
    [SerializeField] bool playerLeft = true;
    [SerializeField] bool attackPlayer = false;
    float aux;

    public static LaurenAIBehaviour instance;
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

    void Start()
    {
        //spawnea en la habitacion inicial. despues ya selecciona habitacion random
        transform.position = rooms[0].position + new Vector3(0f, 0.15f, 0f);
        ChangeRange();
    }


    // Update is called once per frame
    void Update()
    {
        Collider[] _targets = Physics.OverlapSphere(transform.position, hearingRange, targetLayer);
        //si detecta al jugador que diga que lo ha encontrado
        if (_targets.Length > 0)
        {
            Debug.Log("Hay jugador");
            foundPlayer = true;
            //si el jugador no esta agachado dentro del rango, que lo ataque
            if (!BasicCharacterStateMachine.instance.sneaking && (BasicCharacterStateMachine.instance.moveDirection.x != 0 && BasicCharacterStateMachine.instance.moveDirection.z != 0))
            {
                attackPlayer = true;
                Debug.Log("BOOO");
            }
        }
        //si no lo detecta pero foundPlayer sigue siendo true, significa que justo se ha ido
        else if (foundPlayer)
        {
            Debug.Log("player left");
            playerLeft = true;
            foundPlayer = false;
            StartCoroutine(CRT_ChangeRoom());
        }
    }

    IEnumerator CRT_ChangeRoom()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("room");
        //selecciona habitacion random
        int r = Random.Range(0, rooms.Length);
        transform.position = rooms[r].position + new Vector3(0f, 0.15f, 0f);
        ChangeRange();
    }

    void ChangeRange()
    {

        float minDistance = 9999999;
        for (int i = 0; i < 4; i++)
        {
            Vector3 direction = Vector3.forward;
            switch (i)
            {
                case 0:
                    direction = Vector3.back;
                    break;
                case 1:
                    direction = Vector3.right;
                    break;
                case 2:
                    direction = Vector3.left;
                    break;
                default:
                    break;
            }

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, maxDistance, wallLayer))
            {
                Debug.DrawLine(transform.position, hit.point, Color.red, 30);
                aux = (transform.position - hit.point).sqrMagnitude;
                if (minDistance > aux)
                {
                    minDistance = aux;
                }
            }
        }

        hearingRange = Mathf.Sqrt(minDistance);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
