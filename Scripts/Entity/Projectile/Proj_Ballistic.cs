using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proj_Ballistic : MonoBehaviour
{
    public LineRenderer trail;
    float launchTimeElapsed;
    float damage;
    Vector3 StartPos;
    Vector3 Destination;
    float flightTime;
    bool straight;

    List<Vector3> trails = new List<Vector3>();
    public int maxTrailCount;

    int blastRange;

    public GameObject blast;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void InitThis(Vector3 From, Vector3 To, BaseObj origin, float flightTimeEstimated, bool isStraight, float _damage, int _range)
    {
        StartPos = From;
        Destination = To;
        flightTime = Vector3.Distance(From, To) / flightTimeEstimated;
        straight = isStraight;
        damage = _damage;
        blastRange = _range;

        StartCoroutine(FlightSequence());
    }
    IEnumerator FlightSequence()
    {
        trails.Clear();
        for(int i = 0;i< maxTrailCount; i++)
        {
            trails.Add(this.transform.position);
        }
        launchTimeElapsed = 0;
        do
        { 
            launchTimeElapsed += Time.deltaTime;
            this.gameObject.transform.position = Tools.GetBezierCurve(StartPos, Destination, launchTimeElapsed / flightTime, straight ? 0 : 2f, true, this.gameObject.transform);

            trails.RemoveAt(maxTrailCount - 1);
            trails.Insert(0, this.gameObject.transform.position);

            trail.SetPositions(trails.ToArray());
            yield return null;
        } while (launchTimeElapsed < flightTime);

        var targetTile = Tools.GetTileViaCoord(this.transform.position);
        if(targetTile != null)
        {
            var blastRadius = Tools.GetTileWithinRange(targetTile, blastRange, Tools.IgnoreType.All);
            foreach (var tile in blastRadius)
            {
                var entity = tile.GetEntitynThisTile();
                if(entity != null)
                {
                    entity.TakeDamage(damage, CompWeapon.WeaponAttackType.Blast);
                }

                var spark = ObjectPool.Instance.CreateObject("Blast", blast, tile.gameObject.transform.position, tile.gameObject.transform.rotation);
                var particle = spark.GetComponent<ParticleSystem>();
                if (particle != null) particle.Play();
                ObjectPool.Instance.CollectObject(spark, 2f);
            }
        }
        ObjectPool.Instance.CollectObject(this.gameObject);
    }
}
