using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatChanger : MonoBehaviour
{
    public GameObject FirstFloor;
    public GameObject SecondFloor;

    bool floorChanged;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            switch (floorChanged)
            {
                case false:
                    FirstFloor.SetActive(false);
                    SecondFloor.SetActive(true);
                    floorChanged = true;
                    break;
                case true:
                    FirstFloor.SetActive(true);
                    SecondFloor.SetActive(false);
                    floorChanged = false;
                    break;
            }
        }
    }
}
