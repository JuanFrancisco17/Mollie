using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class State
{
    public enum STATES
    {
        IDLE, PATROL, ROOMPATROL, WAIT, CHASE, ATTACK, PUKECHASE
    }

    public enum EVENTS
    {
        START, UPDATE, EXIT
    }

    public virtual void Start() { stage = EVENTS.UPDATE; }
    public virtual void Update() { stage = EVENTS.UPDATE; }
    public virtual void Exit() { stage = EVENTS.EXIT; }

    public STATES name;
    protected EVENTS stage;
    protected State nextState;
    protected float counter;

    public State()
    {

    }

    public State Process()
    {
        //Si el evento en el que estoy es el de entrada, hago el m�todo correspondiente de entrada
        if (stage == EVENTS.START) Start();
        //Si el evento en el que estoy es el de update, hago el m�todo correspondiente
        if (stage == EVENTS.UPDATE) Update();
        //Si el evento en el que estoy es el de salida, hago el m�todo correspondiente
        if (stage == EVENTS.EXIT)
        {
            Exit();
            //Y devolvemos el siguiente estado al que ir
            return nextState;
        }
        //Devolvemos el resultado del m�todo
        return this;
    }
    public class IdleHannah : State
    {
        public IdleHannah() : base()
        {
            name = STATES.IDLE;
        }

        public override void Start()
        {
            Debug.Log("Idle Hannah");
            HannahStateManager.instance.Idle();
            HannahStateManager.instance.anim.SetBool("Running", false);
            base.Start();
        }

        public override void Update()
        {
            nextState = new Patrol();
            stage = EVENTS.EXIT;
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
    public class Patrol : State
    {
        public Patrol() : base()
        {
            name = STATES.PATROL;
        }

        public override void Start()
        {
            HannahStateManager.instance.roomWayPoints = 0;
            WaypointLocator.instance.FirstWayPoint = new Vector3(Random.Range(-1, 1), 0, 1);
            WaypointLocator.instance.SecondWayPoint = new Vector3(Random.Range(1, -1), 0, -1);
            WaypointLocator.instance.ThirdWayPoint = new Vector3(1, 0, Random.Range(1, -1));
            WaypointLocator.instance.FourthWayPoint = new Vector3(-1, 0, Random.Range(-1, 1));
            HannahStateManager.instance.detected = false;
            HannahStateManager.instance.rooms = GameObject.FindGameObjectsWithTag("Room");
            Vector3 vector3 = HannahStateManager.instance.centrePoint.position =
                HannahStateManager.instance.rooms[HannahStateManager.instance.randomRoom].transform.position + new Vector3(0, 1, 0);
            Debug.Log("Patrol");
            HannahStateManager.instance.anim.SetBool("Running", true);
            base.Start();
        }

        public override void Update()
        {
            Debug.Log("Patrol");
            HannahStateManager.instance.RoomDetection();
            HannahStateManager.instance.Detect();
            HannahStateManager.instance.Patrol();
            HannahStateManager.instance.rooms = GameObject.FindGameObjectsWithTag("Room");
            if (HannahStateManager.instance.agent.remainingDistance <= 1f)
            {
                nextState = new RoomPatrol();
                stage = EVENTS.EXIT;
            }

            if (HannahStateManager.instance.target != null)
            {
                nextState = new Chase();
                stage = EVENTS.EXIT;
            }

            if (PukeBehavior.instance.detected)
            {
                nextState = new Chase();
                stage = EVENTS.EXIT;
            }


        }

        public override void Exit()
        {
            base.Exit();
        }
    }
    public class RoomPatrol : State
    {
        public RoomPatrol() : base()
        {
            name = STATES.ROOMPATROL;
        }

        public override void Start()
        {
            HannahStateManager.instance.detected = false;
            HannahStateManager.instance.waypoints = GameObject.FindGameObjectsWithTag("WayPoints");
            HannahStateManager.instance.RoomPatrol();
            Debug.Log("Room Patrol");
            HannahStateManager.instance.anim.SetBool("Running", true);
            base.Start();
        }

        public override void Update()
        {

            HannahStateManager.instance.Detect();
            HannahStateManager.instance.waypoints = GameObject.FindGameObjectsWithTag("WayPoints");
            if (HannahStateManager.instance.roomWayPoints > 3)
            {
                HannahStateManager.instance.roomWayPoints = 0;
            }
            else if (HannahStateManager.instance.agent.remainingDistance <= .1f)
            {
                nextState = new Wait();
                stage = EVENTS.EXIT;
            }

            if (HannahStateManager.instance.target != null)
            {
                nextState = new Chase();
                stage = EVENTS.EXIT;
            }

            if (PukeBehavior.instance.detected)
            {
                nextState = new Chase();
                stage = EVENTS.EXIT;
            }

        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    public class Wait : State
    {
        float counterWait;
        public Wait() : base()
        {
            name = STATES.WAIT;
        }

        public override void Start()
        {
            HannahStateManager.instance.anim.SetBool("Running", false);
            HannahStateManager.instance.agent.isStopped = true;
            Debug.Log("Wait");
            counterWait = 1.5f;
            base.Start();
        }

        public override void Update()
        {
            if (counterWait >= .1)
            {
                counterWait -= Time.deltaTime;
            }
            else
            {
                HannahStateManager.instance.roomWayPoints++;
                HannahStateManager.instance.agent.isStopped = false;
                nextState = new RoomPatrol();
                stage = EVENTS.EXIT;
            }

        }

        public override void Exit()
        {
            base.Exit();
        }
    }
    public class Chase : State
    {
        public Chase() : base()
        {
            name = STATES.CHASE;
        }

        public override void Start()
        {
            Debug.Log("Chase");
            HannahStateManager.instance.anim.SetBool("Running", true);
            base.Start();
        }
        public override void Update()
        {
            Debug.Log("Chase");

            if (!PukeBehavior.instance.detected)
            {
                HannahStateManager.instance.Detect();
            }
            HannahStateManager.instance.Chase();
            if (HannahStateManager.instance.target == null && !PukeBehavior.instance.detected && !HannahStateManager.instance.detected)
            {
                nextState = new Patrol();
                stage = EVENTS.EXIT;
            }

            if (HannahStateManager.instance.GetDistanceToTarget() < HannahStateManager.instance.attackRange * HannahStateManager.instance.attackRange)
            {
                nextState = new Attack();
                stage = EVENTS.EXIT;
            }


        }

        public override void Exit()
        {
            base.Exit();
        }
    }
    public class Attack : State
    {
        public Attack() : base()
        {
            name = STATES.ATTACK;
        }
        public override void Start()
        {
            Debug.Log("Attack");
            HannahStateManager.instance.timeToAttack = HannahStateManager.instance.maxTime;
            base.Start();
        }
        public override void Update()
        {
            HannahStateManager.instance.Detect();
            HannahStateManager.instance.agent.velocity = Vector3.zero;
            if (HannahStateManager.instance.timeToAttack > 0f)
            {
                HannahStateManager.instance.timeToAttack -= Time.deltaTime;
            }
            if (HannahStateManager.instance.timeToAttack <= 0f)
            {
                HannahStateManager.instance.AttackSequence();
                HannahStateManager.instance.timeToAttack = HannahStateManager.instance.maxTime;
            }

            if (HannahStateManager.instance.GetDistanceToTarget() > HannahStateManager.instance.attackRange * HannahStateManager.instance.attackRange)
            {
                nextState = new Chase();
                stage = EVENTS.EXIT;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

}
