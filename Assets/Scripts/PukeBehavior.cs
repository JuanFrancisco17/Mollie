using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PukeBehavior : MonoBehaviour
{
    public GameObject Puke;
    public bool detected;

    public static PukeBehavior instance;

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
        Puke.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
