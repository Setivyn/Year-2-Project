using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct GridCube //whole grid structure, each index is smaller cubes of size (int size)
{
    public int size { get; set; } //Cube size, Time step, Diffusion Rate, Viscosity
    public float dt { get; set; }
    public float diff { get; set; }
    public float visc { get; set; }

    public float[,,] density { get; set; } //Density, Velocity, Prev. Velocity Arrays 

    public float[,,] Vx { get; set; }
    public float[,,] Vy { get; set; }
    public float[,,] Vz { get; set; }

    public float[,,] Vx0 { get; set; }
    public float[,,] Vy0 { get; set; }
    public float[,,] Vz0 { get; set; }
}

public class FluidLogic : MonoBehaviour
{
    [SerializeField] TerrainVisualiseLogic meshObject;
    GridCube cubes;
    int meshSize;

    // Start is called before the first frame update
    void Start()
    {
        meshSize = Convert.ToInt32(Math.Pow(2, meshObject.getSL()) + 1); 
    }

    // Update is called once per frame
    void Update()
    {

    }

    void BeginSimulation()
    {

    }

    GridCube DefineCube(int size, float diffusion, float viscosity, float dt, int meshSize)
    {
        //Implement 
        int N = (meshSize - 1) / size;
        GridCube newCube = new GridCube();
        newCube.size = size;
        newCube.dt = dt;
        newCube.diff = diffusion;
        newCube.visc = viscosity;

        newCube.density = new float[N, N, N];

        newCube.Vx = new float[N, N, N];
        newCube.Vy = new float[N, N, N];
        newCube.Vz = new float[N, N, N];

        newCube.Vx0 = new float[N, N, N];
        newCube.Vy0 = new float[N, N, N];
        newCube.Vz0 = new float[N, N, N];

        return newCube;
    }

    void EnactTimeStep(GridCube cube)
    {
        //{
        diffuse(1, cube.Vx0, cube.Vx, cube.visc, cube.dt, 4, cube.size);
        diffuse(2, cube.Vy0, cube.Vy, cube.visc, cube.dt, 4, cube.size);
        diffuse(3, cube.Vz0, cube.Vz, cube.visc, cube.dt, 4, cube.size);

        project();

        advect();
        advect();
        advect();

        project();
        //} Move Velocities

        //{
        diffuse();
        advect();
        //} Move Dye
    }

    void diffuse(int d, float[,,] Vx0, float[,,] Vx, float visc, float dt, int iter, int size)
    {

    }

    void project()
    {

    }

    void advect()
    {

    }

    void addDToCube(ref GridCube cubes, int x, int y, int z, float amount)
    {
        cubes.density[x,y,z] += amount;
    }

    void addVToCube(ref GridCube cubes, int x, int y, int z, int Vx1, int Vy1, int Vz1)
    {
        cubes.Vx[x, y, z] = Vx1;
        cubes.Vy[x, y, z] = Vy1;
        cubes.Vz[x, y, z] = Vz1;
    }

    void SetColour(double maxVal, double[] values)
    {
        meshObject.GetComponent<TerrainVisualiseLogic>().SetColours(maxVal, values);
    }

    

}


