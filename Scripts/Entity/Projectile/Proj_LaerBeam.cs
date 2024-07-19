using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Proj_LaerBeam : MonoBehaviour
{
    public LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TriggerThis(Vector3 start, Vector3 destination, Color col)
    {
        lineRenderer.material.SetColor("_Color", col);
        StartCoroutine(DisplayBeam(start, destination));
    }
    IEnumerator DisplayBeam(Vector3 start, Vector3 destination)
    {
        var time = 0f;
        if (lineRenderer != null)
        {
            do
            {
                lineRenderer.enabled = true;
                Vector3[] pos = new Vector3[2];
                pos[0] = start;
                pos[1] = destination;
                lineRenderer.SetPositions(pos);
                var width = (0.25f - Mathf.Abs(time - 0.25f)) / 0.25f * 0.06f;
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;

                yield return null;

                time += Time.deltaTime;
            } while (time < 0.5f);

            lineRenderer.enabled = false;

            ObjectPool.Instance.CollectObject(this.gameObject);
        }
    }
}
