using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct GridCube
{
    public int size { get; set; } //Cube size, Time step, Diffusion Rate, Viscosity
    public float dt { get; set; }
    public float diff { get; set; }
    public float visc { get; set; }

    public float[] density { get; set; } //Density, Velocity, Prev. Velocity Arrays 

    public float[] Vx { get; set; }
    public float[] Vy { get; set; }
    public float[] Vz { get; set; }

    public float[] Vx0 { get; set; }
    public float[] Vy0 { get; set; }
    public float[] Vz0 { get; set; }
}

public class FluidLogic : MonoBehaviour
{
    [SerializeField] GameObject meshObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void BeginSimulation()
    {

    }

    GridCube DefineCube(int size, float diffusion, float viscosity, float dt, bool intersect)
    {
        //Implement 
        GridCube newCube = new GridCube();
        return newCube;
    }

    void EnactTimeStep(GridCube cube)
    {
    }

    void diffuse()
    {

    }

    void project()
    {

    }

    void advect()
    {

    }

    void SetColour(double maxVal, double[] values)
    {
        meshObject.GetComponent<TerrainVisualiseLogic>().SetColours(maxVal, values);
    }

    

}


