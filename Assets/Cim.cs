using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cim : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var trans = gameObject.GetComponentsInChildren<Transform>();
        foreach (var t in trans)
        {
            Debug.Log(t.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
