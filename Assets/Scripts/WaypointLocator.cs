using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointLocator : MonoBehaviour
{
    public GameObject[] waypoints;

    public float maximumDistance;
    public float distance;

    public LayerMask WallLayer;

    public Vector3 FirstWayPoint;
    public Vector3 SecondWayPoint;
    public Vector3 ThirdWayPoint;
    public Vector3 FourthWayPoint;

    RaycastHit hit;
    RaycastHit hit2;
    RaycastHit hit3;
    RaycastHit hit4;

    public static WaypointLocator instance;


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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rayCasts();
    }

    public void rayCasts()
    {
        
        Debug.DrawRay(transform.position, FirstWayPoint, Color.red);
        Debug.DrawRay(transform.position, SecondWayPoint, Color.red);
        Debug.DrawRay(transform.position, ThirdWayPoint, Color.red);
        Debug.DrawRay(transform.position, FourthWayPoint, Color.red);

        if (Physics.Raycast(transform.position, FirstWayPoint, out hit, maximumDistance, WallLayer))
        {

        }
        if (Physics.Raycast(transform.position, SecondWayPoint, out hit2, maximumDistance, WallLayer))
        {

        }
        if (Physics.Raycast(transform.position, ThirdWayPoint, out hit3, maximumDistance, WallLayer))
        {

        }
        if (Physics.Raycast(transform.position, FourthWayPoint, out hit4, maximumDistance, WallLayer))
        {

        }

        if (Physics.Raycast(hit.point + -FirstWayPoint * distance, Vector3.down, out RaycastHit _hit, WallLayer)) 
        {
            waypoints[0].transform.position = _hit.point;
        }
        if (Physics.Raycast(hit2.point + -SecondWayPoint * distance, Vector3.down, out RaycastHit _hit2, WallLayer))
        {
            waypoints[1].transform.position = _hit2.point;
        }
        if (Physics.Raycast(hit3.point + -ThirdWayPoint * distance, Vector3.down, out RaycastHit _hit3, WallLayer)) 
        {
            waypoints[2].transform.position = _hit3.point;
        }
        if (Physics.Raycast(hit4.point + -FourthWayPoint * distance, Vector3.down, out RaycastHit _hit4, WallLayer))
        {
            waypoints[3].transform.position = _hit4.point;
        }

    }

    private void OnDrawGizmos()
    {
        
    }
}
