using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static State;

public class HannahStateManager : MonoBehaviour
{
    public float speed = 5f;
    public float chasingSpeed = 8f;
    public float chaseDuration;
    public float timeToAttack;
    public float maxTime;
    public float attackRange;
    public float rotationSpeed;
    public bool detected;
    public bool audioDetected;

    Collider[] _targets;
    Collider[] _targetsAudio;

    public static HannahStateManager instance;
    public Animator anim;
    public NavMeshAgent agent;
    public AudioClip steps;
    public AudioClip screamer;

    #region detectionVAR
    [Header("DETECCION")]
    public float visionRange;
    public float visionAngle;
    public float audioRange;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
    public Transform rayOrigin;
    Vector3 _targetDir;
    public Transform target;
    public Transform TransformL, TransformR;
    #endregion
    #region patrolVAR
    [Header("PATRULLAJE")]
    public GameObject[] rooms;
    public GameObject[] waypoints;
    public int roomWayPoints = 0;
    public int maxRoomWP;
    public float counter = 10f;
    public Transform centrePoint;
    public float range;
    public int randomRoom;
    #endregion


    public State currentState;
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

    private void Start()
    {
        currentState = new IdleHannah();
        agent.autoBraking = false;
        agent.velocity = Vector3.zero;
        detected = false;

        agent.speed = speed;
    }
    private void Update()
    {
        currentState = currentState.Process();
    }
    public void Idle()
    {
    }

    #region Detection

    public void Detect()
    {
        DetectCharacter();
        DetectAudio();
        if (detected)
        {
            target = BasicCharacterStateMachine.instance.transform;
        }
    }
    public void DetectCharacter()
    {
        //Guardamos todos los objetos encontrados con el overlap
        _targets = Physics.OverlapSphere(transform.position, visionRange, targetLayer);
        //Si ha encontrado alg�n objeto, la longitud del array es mayor que 0
        if (_targets.Length > 0)
        {
            bool targetDetected = false;

            foreach (Collider target in _targets)
            {
                //Calculamos la direccion hacia el objeto
                _targetDir = target.transform.position - rayOrigin.position;

                //Si esta fuera del angulo de vision, lo ignoramos
                //Se calcula si esta dentro con el angulo que hay entre el forward y la direccion
                //del objetivo. Si este angulo es menor que la mitad del angulo de vision, esta dentro
                if (Vector3.Angle(transform.forward, _targetDir) > visionAngle / 2f)
                {
                    StartCoroutine(stopChasing());
                }
                //Lanzamos un rayo desde el enemigo hacia el jugador para comprobar si esta
                //escondido detras de alguna pared u obstaculo
                //Sumamos un offset al origen en el eje Y para que no lance el rayo desde los pies
                else if (Physics.Raycast(rayOrigin.position, _targetDir.normalized,
                    _targetDir.magnitude, obstacleLayer) == false)
                {
                    targetDetected = true;
                    break;
                }

                //Dibujamos el rayo que comprueba si esta tras un obstaculo
                //Sumamos un offset al origen en el eje Y para que no lance el rayo desde los pies
                Debug.DrawRay(rayOrigin.position, _targetDir, Color.magenta);
            }

            if (targetDetected)
            {
                detected = true;
            }
        }
        //Si el array est� vac�o, no ha encontrado nada
        else
        {
            StartCoroutine(stopChasing());
        }
    }
    public void DetectAudio()
    {
        _targetsAudio = Physics.OverlapSphere(transform.position, audioRange, targetLayer);
        if (_targetsAudio.Length > 0)
        {
            foreach (Collider target in _targetsAudio)
            {
                if (!BasicCharacterStateMachine.instance.sneaking)
                {
                    if (Physics.Raycast(rayOrigin.position, _targetDir.normalized,
                    _targetDir.magnitude, obstacleLayer) == false)
                    {
                        detected = true;
                        break;
                    }
                }
            }
        }
        else
        {
            StartCoroutine(stopChasing());
        }
    }

    public void RoomDetection()
    {
        //Escoge una localizacion dentro del array
        randomRoom = Random.Range(0, rooms.Length);

    }
    #endregion

    #region Patrolling
    public void Patrol()
    {
        if (rooms.Length <= 0)
        {
            return;
        }
        //Se establece el destino del agente
        agent.SetDestination(rooms[randomRoom].transform.position);
        //Se activa la animacion de andar
    }

    public void PlaySteps() { SoundManager.instance.PlayOneshot(1, steps); }

    public void RoomPatrol()
    {
        maxRoomWP = waypoints.Length;
        if (roomWayPoints > 3)
        {
            roomWayPoints = 0;
        }
        agent.SetDestination(waypoints[roomWayPoints].transform.position);
    }
    #endregion

    public void Chase()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            agent.speed = chasingSpeed;
            if (agent.remainingDistance <= 1f)
            {
                agent.velocity = Vector3.zero;
            }
        }
    }
    public void LookAtTarget()
    {
        //Calculamos la direccion con respecto al target
        Vector3 _direction = target.position - transform.position;
        //Hay que poner la Y en 0 para que solo haga el LookAt en el eje Y
        _direction.y = 0;
        //Orientamos al personaje para que mire hacia esa direccion
        Quaternion _rot = Quaternion.LookRotation(_direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, _rot, Time.deltaTime * rotationSpeed);

    }
    IEnumerator stopChasing()
    {
        yield return new WaitForSeconds(chaseDuration);
        target = null;
        detected = false;
    }
    public void AttackSequence()
    {
        StartCoroutine(CRT_AttackSequence());

    }
    IEnumerator CRT_AttackSequence()
    {
        anim.SetTrigger("Attack");
        SoundManager.instance.PlayOneshot(1, screamer);
        yield return new WaitForSeconds(1.7f);
        SceneManager.LoadScene(1);

    }
    IEnumerator RoomPatrolDelay()
    {
        yield return new WaitForSeconds(1f);
        agent.SetDestination(waypoints[roomWayPoints].transform.position);
    }

    public float GetDistanceToTarget()
    {
        //Calculamos la direccion con respecto al target y devolvemos la distancia hacia el
        Vector3 _direction = BasicCharacterStateMachine.instance.transform.position - transform.position;
        return _direction.sqrMagnitude;
    }

    private void OnDrawGizmos()
    {
        //Dibujamos el rango de vision
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        //Dibujamos el cono de vision
        Gizmos.color = Color.green;
        //Rotamos los helper para que tengan la rotacion igual a la mitad del angulo de vision
        //Para dibujar el cono de vision, rotamos dos objetos vacios para luego lanzar un rayo
        //en el forward de cada uno de ellos y dibuje el cono
        TransformL.localRotation = Quaternion.Euler(0f, visionAngle / -2f, 0f);
        TransformR.localRotation = Quaternion.Euler(0f, visionAngle / 2f, 0f);
        Gizmos.DrawRay(TransformL.position, TransformL.forward * visionRange);
        Gizmos.DrawRay(TransformR.position, TransformR.forward * visionRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, audioRange);
    }
}
