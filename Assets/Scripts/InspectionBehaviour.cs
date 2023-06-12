using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionBehaviour : MonoBehaviour
{
    public BasicCharacterStateMachine basicCharacterStateMachine;
    public YawController yawController;
    public ThirdPersonCamera thirdPersonCamera;
    public GameObject model;
    [Tooltip("Sensibilidad de la rotacion del objeto en ambos ejes")]
    public Vector2 sens = new Vector2(1f, 1f);
    public bool canRotate = true;

    [HideInInspector]
    public bool interactingThisFrame = false;
    bool isInteracting = false;
    bool cooldown = false;
    Transform inspectionTrans;

    private void Start()
    {
        //coge el transform para inspeccionar objetos que hay en la camara
        //NO CAMBIAR NOMBRE!
        inspectionTrans = Camera.main.gameObject.transform.Find("InspectionTransform");
    }

    void Update()
    {
        if (interactingThisFrame)
        {

            SoundManager.instance.PlayOneshot(0, GameManager.instance.paperClip);
            //desactiva los scripts de movimiento y rotacion de camara
            basicCharacterStateMachine.enabled = false;
            yawController.enabled = false;
            thirdPersonCamera.enabled = false;
            //cambia el bool a falso, de esta manera solo se ejecuta una sola vez
            interactingThisFrame = false;
            isInteracting = true;
            //emparenta y cambia el transform del modelo
            model.transform.parent = inspectionTrans.transform;
            model.transform.position = inspectionTrans.transform.position;
            model.transform.rotation = inspectionTrans.transform.rotation;
            //cooldown necesario para que no ocurra todo en el mismo frame
            StartCoroutine(Cooldown(0.5f));
        }
        else if (isInteracting)
        {
            if (canRotate)
            {
                //coge el transform up de la camara y el right
                Vector3 objectUp = Camera.main.transform.up;
                Debug.DrawRay(model.transform.position, objectUp * 2, Color.blue);
                Vector3 objectRight = Camera.main.transform.right;
                Debug.DrawRay(model.transform.position, objectRight * 2, Color.yellow);
                //y rota el modelo alrededor de un eje
                Quaternion rot = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * sens.x * Time.deltaTime, objectUp) * model.transform.rotation;
                // Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * sens.y * Time.deltaTime, objectRight) *
                model.transform.rotation = rot;
            }

            if (Input.GetButtonDown("Fire1") && (cooldown == false))
            {
                //vuelve a activar los scripts del personaje
                SoundManager.instance.PlayOneshot(0, GameManager.instance.paperClip);
                basicCharacterStateMachine.enabled = true;
                yawController.enabled = true;
                thirdPersonCamera.enabled = true;

                //vuelve a emparentar al objeto con este script y mueve la posicion y rotación para que sea igual
                model.transform.parent = this.transform;
                model.transform.position = this.transform.position;
                model.transform.rotation = this.transform.rotation;
                isInteracting = false;
                //cooldown necesario para que no ocurra todo en el mismo frame
                StartCoroutine(Cooldown(0.5f));
            }
        }
    }

    IEnumerator Cooldown(float sec)
    {
        //no se porque estas leyendo esto. es un cooldown simple
        cooldown = true;
        yield return new WaitForSeconds(sec);
        cooldown = false;
    }
}
