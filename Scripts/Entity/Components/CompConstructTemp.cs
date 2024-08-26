using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompConstructTemp : BaseComponent
{
    public float buildProgress = 0f;
    public float buildTime;
    public Material buildMat;
    Dictionary<MeshRenderer, Material> matDic = new Dictionary<MeshRenderer, Material>();
    public bool startConstruct = false;
    public override void OnApply(int index)
    {
        
    }

    public override void OnDestroyThis()
    {
        
    }

    //public override void OnTriggerFunction(params object[] obj)
    //{
        
    //}

    // Start is called before the first frame update
    public override void Start()
    {
        //base.Start();
        
    }
    public void SimBuild()
    {
        buildMat = Resources.Load<Material>("Materials/WireFrame");
        var meshRenderers = thisObj.GetComponentsInChildren<MeshRenderer>();

        foreach (var meshRenderer in meshRenderers)
        {
            List<Material> mat = meshRenderer.materials.ToList();
            var thisMaterial = new Material(buildMat);
            mat.Add(thisMaterial);
            meshRenderer.materials = mat.ToArray();
            matDic.Add(meshRenderer, thisMaterial);
        }
    }
    public void InitConstruct()
    {
        this.EP = 20;
        this.MaxEP = 20;
        this.HP = 20;
        this.MaxHP = 20;

        var meshRenderers = thisObj.GetComponentsInChildren<MeshRenderer>();

        matDic.Clear();
        foreach (var meshRenderer in meshRenderers)
        {
            List<Material> mat = meshRenderer.materials.ToList();
            matDic.Add(meshRenderer, mat.LastOrDefault());
        }

        startConstruct = true;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!startConstruct) return;

        if(buildProgress < 1f)
        {
            if (this.EP >= 10f * Time.deltaTime)
            {
                this.EP -= 10f * Time.deltaTime;
                buildProgress += (1f / buildTime) * Time.deltaTime;

                foreach (var item in matDic)
                {
                    item.Value.SetFloat("_progress", buildProgress);
                }
            }
            else
            {
                this.EP = 0;
            }
        }
        if(buildProgress >= 1f)
        {
            Debug.Log("Construction complete");

            foreach (var mesh in matDic)
            {
                var list = mesh.Key.materials.ToList();
                list.Remove(list.Last());
                mesh.Key.materials = list.ToArray();
            }

            thisObj.RemoveDesiredComponent(this);
        }
    }
}
