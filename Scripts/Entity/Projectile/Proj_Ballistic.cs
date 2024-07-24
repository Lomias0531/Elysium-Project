using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proj_Ballistic : MonoBehaviour
{
    //public LineRenderer trail;
    float launchTimeElapsed;
    float damage;
    Transform StartPos;
    Vector3 Destination;
    float flightTime;
    bool straight;

    //List<Vector3> trails = new List<Vector3>();
    //public int maxTrailCount;

    int blastRange;

    public GameObject blast;

    public TrailRenderer trails;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void InitThis(Transform launcher, Vector3 To, BaseObj origin, float flightTimeEstimated, bool isStraight, float _damage, int _range)
    {
        StartPos = launcher;
        Destination = To;
        flightTime = Vector3.Distance(launcher.position, To) / flightTimeEstimated;
        straight = isStraight;
        damage = _damage;
        blastRange = _range;

        StartCoroutine(FlightSequence());
    }
    IEnumerator FlightSequence()
    {
        //var trail = ObjectPool.Instance.CreateObject("Trail", trails.gameObject, this.transform.position, this.transform.rotation);
        //trail.transform.SetParent(this.transform);

        trails.enabled = true;
        launchTimeElapsed = 0;
        do
        { 
            launchTimeElapsed += Time.deltaTime;

            List<Vector3> midPoints = new List<Vector3>();
            midPoints.Add(StartPos.position + StartPos.forward * (1 + Random.Range(0.1f,0.5f)));
            midPoints.Add((StartPos.position + Destination) / 2 + new Vector3(0, 3f, 0));

            if (launchTimeElapsed > flightTime) break;
            this.gameObject.transform.position = Tools.GetBezierCurve(StartPos.position, Destination,midPoints.ToArray(), launchTimeElapsed / flightTime, true, this.gameObject.transform);
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
        //trails.transform.SetParent(null);
        trails.enabled = false;
        //ObjectPool.Instance.CollectObject(trails, 2f);
        ObjectPool.Instance.CollectObject(this.gameObject);
    }
}
