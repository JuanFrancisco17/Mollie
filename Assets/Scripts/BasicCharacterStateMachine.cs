using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacterStateMachine : MonoBehaviour
{
    //-------------------VARIABLES-------------------
    #region MovementVAR
    [Header("Movement")]
    [Tooltip("Rigidbody del personaje")]
    public Rigidbody rb;
    [Tooltip("Capsula del personaje")]
    public CapsuleCollider coll;
    public AudioClip stepsClip;
    public Animator anim;
    // [Tooltip("Animator del personaje")]
    // public Animator anim;
    [Tooltip("Velocidad a la que se mueve el personaje")]
    public float moveSpeed;
    [Tooltip("Velocidad a la que se mueve el personaje cuando esta agachado")]
    public float sneakMoveSpeed;
    [HideInInspector]
    public Vector3 moveDirection;
    [Tooltip("Factor de gravedad. Se multiplica con la fuerza de gravedad de Unity")]
    public float gravityScale = 5f;
    [HideInInspector]
    public bool isGrounded;
    #endregion 
    #region RaycastVAR
    [Header("Raycast")]
    [Tooltip("Distancia que mide el rayo de deteccion suelo")]
    public float groundCheckDistance = 1;
    [Tooltip("Offset de la deteccion suelo (Respecto pivote)")]
    public float groundCheckOffset;
    [HideInInspector]
    public RaycastHit camHit;
    [HideInInspector]
    public Ray camRay;
    [Tooltip("Capa suelo. Raycast para diferenciar lo que es suelo de lo que no lo es")]
    public LayerMask groundLayers;
    [Tooltip("Capa con las cosas con las que se puede interactuar, como el peluche")]
    public LayerMask interactLayers;
    #endregion 
    #region MiscVAR
    [Header("Misc")]
    [Tooltip("Game Object de la linterna")]
    public GameObject flashlight;
    [Tooltip("MainCamera del jugador")]
    public Camera FPCamera;
    [Tooltip("MainCamera del jugador")]
    public YawController yawController;
    [HideInInspector]
    public bool flashlightON = false;
    [HideInInspector]
    public bool camRayHit = false;
    [HideInInspector]
    public bool interacting = false;
    [HideInInspector]
    public bool cooldown = false;
    [HideInInspector]
    public bool sneaking = false;
    //[HideInInspector]
    public bool hiding = false;
    //as[HideInInspector]
    public bool canhide = false;
    private Transform hideout;
    private bool hideoutAgacharse;
    public Vector3 lastPosition;
    #endregion 
    [Header("States")]
    [Tooltip("Estado actual del jugador")]
    public States currentState;


    //-------------------FUNCTIONS-------------------
    public void Grounded()
    {
        RaycastHit hit;
        //raycast para detectar el suelo. si lo detecta se pone verde y si no rojo. el raycast depende de la gravedad
        if (Physics.Raycast(transform.position + groundCheckOffset * Vector3.up, Vector3.down, out hit, groundCheckDistance, groundLayers))
        {
            Debug.DrawRay(transform.position + groundCheckOffset * Vector3.up, Vector3.down * groundCheckDistance, Color.green);
            //esta tocando suelo
            isGrounded = true;
            //reseteamos la y cuando esta parado en el suelo para que no haga cosas raras
            moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
        }
        else
        {
            Debug.DrawRay(transform.position + groundCheckOffset * Vector3.up, Vector3.down * groundCheckDistance, Color.red);
            //no esta tocando suelo
            isGrounded = false;
        }
    }

    public void GravityApply()
    {
        //aplica la gravedad al personaje
        //esta multiplicada por un float (gravityScale) para regular mejor la fuerza con la que cae
        moveDirection.y += Physics.gravity.y * Time.deltaTime * gravityScale;
    }

    public void MovementInput(float speed)
    {
        //al principio guarda la y del jugador para que no se sobreescriba y se normalice junto con la x y la z. Luego la vuelve a aplicar al moveDirection y asi se mantiene
        float yStore = moveDirection.y;
        //A la direccion de movimiento se le pasa un vector resultante de la suma de su movimiento en ambos ejes.        
        var input = ((transform.forward * Input.GetAxisRaw("Vertical")) + (transform.right * Input.GetAxisRaw("Horizontal"))).normalized;
        moveDirection = input;

        //se multiplica por la velocidad de movimiento
        moveDirection = moveDirection * speed;
        moveDirection.y = yStore;
    }

    public void Sneak()
    {
        sneaking = true;
        transform.localScale = new Vector3(transform.localScale.x, 0.63f, transform.localScale.z);
    }

    public void ScaleBackToNormal()
    {
        sneaking = false;
        transform.localScale = new Vector3(transform.localScale.x, 1.26f, transform.localScale.z);
    }

    public void Interaction()
    {
        //si está mirando al objeto y hace input, que interactue
        if (camRayHit && Input.GetButtonDown("Fire1") && (cooldown == false))
        {
            interacting = true;

            //...o bien cogiendo el objeto
            if (camHit.transform.gameObject.layer == LayerMask.NameToLayer("PickUp"))
            {
                camHit.transform.GetComponentInChildren<PickUpBehaviour>().interacted = true;
            }
            //...o bien interactuando..
            else if (camHit.transform.gameObject.layer == LayerMask.NameToLayer("Interact"))
            {
                camHit.transform.GetComponentInChildren<InteractionBehaviour>().interacted = true;
            }
            //...o bien inspeccionandolo
            else if (camHit.transform.gameObject.layer == LayerMask.NameToLayer("Inspect"))
            {
                Debug.Log("inspecting");
                camHit.transform.gameObject.GetComponentInChildren<InspectionBehaviour>().interactingThisFrame = true;
            }
            //cooldown necesario para que no ocurra todo en el mismo frame
            StartCoroutine(Cooldown(0.5f));
        }
        else
        {
            interacting = false;
        }
    }

    // public void PickUpOrDrop()
    // {
    //     //si está mirando al objeto y hace input, que lo coja
    //     if (camRayHit && camHit.transform.gameObject.layer == LayerMask.NameToLayer("PickUp"))
    //     {
    //         if (Input.GetButtonDown("Fire1") && (cooldown == false))
    //         {
    //             // Debug.Log("object Picked");
    //             // var obj = hit.collider.gameObject;
    //             // obj.GetComponent<Rigidbody>().isKinematic = true;
    //             // obj.transform.parent = pickUpParent.transform;
    //             // obj.transform.position = pickUpParent.transform.position;
    //             pickingUp = true;
    //             //cooldown necesario para que no ocurra todo en el mismo frame
    //             StartCoroutine(Cooldown(0.5f));

    //         }
    //     }
    //     //si ya tiene un objeto, le da al input y el cooldown se ha acabado, entonces que lo suelte
    //     else if (Input.GetButtonDown("Fire1") && (pickingUp == true) && (cooldown == false))
    //     {
    //         // Debug.Log("object Down");
    //         // Debug.Log("unparented");
    //         // obj.transform.parent = null;
    //         // obj.GetComponent<Rigidbody>().isKinematic = false;
    //         pickingUp = false;
    //     }
    //     //si no está mirando, que el rayo sea rojo y ya esta
    //     else
    //     {
    //         Debug.DrawRay(camRay.origin, camRay.direction * 4f, Color.red);
    //     }
    // }

    public void Hide()
    {
        if (Input.GetKeyDown(KeyCode.E) && (cooldown == false) && (canhide == true))
        {
            hiding = !hiding;

            if (hiding)
            {
                //lo hace chiquito y lo emparenta al escondite             
                lastPosition = transform.position;
                if (hideoutAgacharse)
                {
                    transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }
                StartCoroutine(LeanIn());
                if (!HannahStateManager.instance.detected)
                {
                    coll.enabled = false;
                }

                rb.isKinematic = true;
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
                // transform.position = lastPosition;
                LeanTween.move(this.gameObject, lastPosition, 0.5f).setEase(LeanTweenType.easeOutCirc);
                coll.enabled = true;
                rb.isKinematic = false;
            }
            //cooldown necesario para que no ocurra todo en el mismo frame
            StartCoroutine(Cooldown(0.5f));

        }

    }

    public void FlashlightOnOff()
    {
        flashlightON = !flashlightON;
        if (flashlightON)
        {
            flashlight.SetActive(true);
        }
        else
        {
            flashlight.SetActive(false);
        }
    }

    public void PlaySteps()
    {
        SoundManager.instance.PlayOneshot(0, stepsClip);
    }

    IEnumerator Cooldown(float sec)
    {
        //no se porque estas leyendo esto. es un cooldown simple
        cooldown = true;
        yield return new WaitForSeconds(sec);
        cooldown = false;
    }

    IEnumerator LeanIn()
    {
        LeanTween.move(this.gameObject, hideout.position, 0.5f).setEase(LeanTweenType.easeOutCirc);
        LeanTween.rotate(this.gameObject, hideout.localRotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeOutCirc);
        yield return null;
        // yield return new WaitForSeconds(0.5f);
        transform.rotation = hideout.rotation;
    }



    //-------------------START AND UPDATE-------------
    public static BasicCharacterStateMachine instance;
    private void Awake()
    {
        //si no hay ninguna instancia de este script. este es el script correcto. si hay otro destruye este
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
        //Inicializamos las referencias y variables
        // anim = this.GetComponent<Animator>();
        //Elegimos el estado en el que empieza este personaje
        TransitionToState(new Idle());
        flashlightON = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Si el evento en el que estoy es el de update, hago el método correspondiente
        currentState.Update();


        //Al pulsar la F, enciende o apaga la linterna dependiendo de su estado anterior
        if (Input.GetKeyDown(KeyCode.F))
        {
            FlashlightOnOff();
        }

        //Raycast desde el centro de la camara para comprobar a que está mirando y si puede interactuar o no
        Vector3 pos = new Vector3(960, 540, 0);
        camRay = FPCamera.ScreenPointToRay(pos);
        if (Physics.Raycast(camRay.origin, camRay.direction, out camHit, 4f, interactLayers))
        {
            Debug.DrawRay(camRay.origin, camRay.direction * 4f, Color.green);

            camRayHit = true;
        }
        else
        {
            Debug.DrawRay(camRay.origin, camRay.direction * 4f, Color.red);
            camRayHit = false;
        }

        //revisa en cada frame si se puede interactuar o no
        Interaction();

    }

    public void TransitionToState(States state)
    {
        //termina el estado y pasa al enter del siguiente
        state.Exit();
        currentState = state;
        currentState.Enter();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.layer == LayerMask.NameToLayer("Hideout"))
        {
            canhide = true;
            hideout = other.gameObject.GetComponentInChildren<Escondite>().esconditeTransform;
            hideoutAgacharse = other.gameObject.GetComponentInChildren<Escondite>().agacharse;
        }
        else
        {
            canhide = false;
        }
    }

}
