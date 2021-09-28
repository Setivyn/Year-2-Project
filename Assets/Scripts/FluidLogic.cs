using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidLogic : MonoBehaviour
{
    [SerializeField] GameObject VisObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetColour(double maxVal, double[] values)
    {
        VisObj.GetComponent<TerrainVisualiseLogic>().SetColours(maxVal, values);
    }

    
}
