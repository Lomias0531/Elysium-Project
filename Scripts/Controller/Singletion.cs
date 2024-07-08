using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singletion<T> : MonoBehaviour where T: MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance is null)
            {
                var obj = GameObject.FindObjectOfType<T>();
                if(obj != null)
                {
                    instance = obj;
                }else
                {
                    GameObject controller = GameObject.Find("Controller");
                    if (controller is null)
                    {
                        controller = new GameObject("Controller");
                        DontDestroyOnLoad(controller);
                    }
                    instance = controller.GetComponent<T>();
                    if (instance is null)
                    {
                        instance = controller.AddComponent<T>();
                    }
                }
            }
            return instance;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DestroyThis()
    {
        Destroy(this);
    }
}
