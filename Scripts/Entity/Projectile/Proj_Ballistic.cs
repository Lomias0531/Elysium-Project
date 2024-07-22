using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proj_Ballistic : MonoBehaviour
{
    public LineRenderer trail;
    float launchTimeElapsed;
    float damage;
    BaseObj launcher;
    BaseObj target;
    Vector3 StartPos;
    Vector3 Destination;
    float flightTime;
    bool straight;

    Queue<Vector3> trails = new Queue<Vector3>();
    public int maxTrailCount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void InitThis(Vector3 From, Vector3 To, BaseObj origin, BaseObj _target, float flightTimeEstimated, bool isStraight, float _damage)
    {
        StartPos = From;
        Destination = To;
        flightTime = Vector3.Distance(From, To) / flightTimeEstimated;
        straight = isStraight;
        launcher = origin;
        target = _target;
        damage = _damage;

        StartCoroutine(FlightSequence());
    }
    IEnumerator FlightSequence()
    {
        launchTimeElapsed = 0;
        do
        { 
            launchTimeElapsed += Time.deltaTime;
            this.gameObject.transform.position = Tools.GetBezierCurve(StartPos, Destination, launchTimeElapsed / flightTime, straight ? 0 : 2f, true, this.gameObject.transform);

            if(trails.Count < maxTrailCount)
            {
                trails.Enqueue(this.gameObject.transform.position);
            }else
            {
                trails.Dequeue();
                trails.Enqueue(this.gameObject.transform.position);
            }
            trail.SetPositions(trails.ToArray());
            yield return null;
        } while (launchTimeElapsed < flightTime);
        if(target != null)
        {
            target.TakeDamage(damage, CompWeapon.WeaponAttackType.Blast);
        }
        ObjectPool.Instance.CollectObject(this.gameObject);
    }
}
