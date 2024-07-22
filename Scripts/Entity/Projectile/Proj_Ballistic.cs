using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proj_Ballistic : MonoBehaviour
{
    public LineRenderer trail;
    float launchTimeElapsed;
    float damage;
    BaseObj launcher;
    Vector3 StartPos;
    Vector3 Destination;
    float flightTime;

    bool isLaunched = false;
    List<Vector3> trails = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void InitThis(Vector3 From, Vector3 To, float flightTimeEstimated)
    {
        StartPos = From;
        Destination = To;
        flightTime = flightTimeEstimated;

        StartCoroutine(FlightSequence());
    }
    IEnumerator FlightSequence()
    {
        launchTimeElapsed = 0;
        do
        {
            launchTimeElapsed += Time.deltaTime;
            this.gameObject.transform.position = Tools.GetBezierCurve(StartPos, Destination, launchTimeElapsed / flightTime);
            yield return null;
        } while (launchTimeElapsed < flightTime);
        ObjectPool.Instance.CollectObject(this.gameObject);
    }
}
