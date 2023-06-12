using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("sensibilidad en la x de la rotación de la cámara")]
    public float sens = 1;

    private void Update()
    {
        //segun el imput recibido y su sensibilidad rota la camara
        float input = Input.GetAxis("Mouse X") * sens;
        transform.Rotate(0, input, 0);
    }
}
