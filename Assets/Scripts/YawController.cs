using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YawController : MonoBehaviour
{
    [Tooltip("Sensibilidad en la Y (Yaw) de la rotación de la cámara")]
    public float sens = 1f;
    private float yaw;

    // Update is called once per frame
    void Update()
    {
        //segun el imput recibido y su sensibilidad...
        float input = -Input.GetAxis("Mouse Y") * sens;

        yaw += input;
        //...hace un clamp para evitar errores en el movimiento...
        yaw = Mathf.Clamp(yaw, -50, 60);
        //...y rota la camara
        transform.rotation = Quaternion.Euler(yaw, transform.rotation.eulerAngles.y, 0f);
        // transform.rotation = Quaternion.Euler(yaw, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

    }
}
