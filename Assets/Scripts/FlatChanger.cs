using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatChanger : MonoBehaviour
{
    public GameObject FirstFloor;
    public GameObject SecondFloor;
    public GameObject secondFloorRoom;
    public GameObject firstFloorRoom;

    public GameObject hannah;

    bool floorChanged;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(ToSecondFloor());
        }
    }

    IEnumerator ToSecondFloor()
    {
        hannah.SetActive(false);
        switch (floorChanged)
        {
            case false:
                FirstFloor.SetActive(false);
                SecondFloor.SetActive(true);
                hannah.transform.position = secondFloorRoom.transform.position;
                floorChanged = true;
                break;
            case true:
                FirstFloor.SetActive(true);
                SecondFloor.SetActive(false);
                hannah.transform.position = firstFloorRoom.transform.position;
                floorChanged = false;
                break;
        }
        yield return new WaitForSeconds(.1f);
        hannah.SetActive(true);

    }
}
