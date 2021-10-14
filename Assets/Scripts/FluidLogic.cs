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
    public int count { get; set; }

    public float[,,] density { get; set; }  //Density, Prev Density
    public float[,,] d0 { get; set; }

    public float[,,] Vx { get; set; } //V, Prev V
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

    void BeginSimulation(int iterations, int divSize, float diff, float visc, float dt)
    {
        cubes = DefineCube(divSize, diff, visc, dt, meshSize);

        EnactTimeStep(cubes, iterations);
    }

    

    GridCube DefineCube(int size, float diffusion, float viscosity, float dt, int meshSize)
    {
        int N = (meshSize - 1) / size;
        GridCube newCube = new GridCube();
        newCube.size = size;
        newCube.dt = dt;
        newCube.diff = diffusion;
        newCube.visc = viscosity;
        newCube.count = N;

        newCube.d0 = new float[N, N, N];
        newCube.density = new float[N, N, N];

        newCube.Vx = new float[N, N, N];
        newCube.Vy = new float[N, N, N];
        newCube.Vz = new float[N, N, N];

        newCube.Vx0 = new float[N, N, N];
        newCube.Vy0 = new float[N, N, N];
        newCube.Vz0 = new float[N, N, N];

        return newCube;
    }

    void EnactTimeStep(GridCube cube, int iterations)
    {
        //{
        diffuse(1, cube.Vx0, cube.Vx, cube.diff, cube.dt, iterations, cube.size);
        diffuse(2, cube.Vy0, cube.Vy, cube.diff, cube.dt, iterations, cube.size);
        diffuse(3, cube.Vz0, cube.Vz, cube.diff, cube.dt, iterations, cube.size);

        project(cube.Vx0, cube.Vy0, cube.Vz0, cube.Vx, cube.Vy, iterations, cube.count);

        advect(1, cube.Vx, cube.Vx0, cube.Vx, cube.Vy, cube.Vz, cube.dt, cube.count);
        advect(2, cube.Vy, cube.Vy0, cube.Vx, cube.Vy, cube.Vz, cube.dt, cube.count);
        advect(3, cube.Vz, cube.Vz0, cube.Vx, cube.Vy, cube.Vz, cube.dt, cube.count);

        project(cube.Vx, cube.Vy, cube.Vz, cube.Vx0, cube.Vy0, iterations, cube.count);
        //} Move Velocities

        //{
        diffuse(0, cube.d0, cube.density, cube.visc, cube.dt, iterations, cube.size);
        advect(1, cube.density, cube.d0, cube.Vx, cube.Vy, cube.Vz, cube.dt, cube.count);
        //} Move Dye
    }

    void diffuse(int d, float[,,] Vq0, float[,,] Vq, float diff, float dt, int iter, int size)
    {
        float a = dt * diff * (size - 2) * (size * 2);
        linearSolve(d, Vq, Vq0, a, 1 + (6 * a), iter, size); 
    }

    void project(float[,,] Vx0, float[,,] Vy0, float[,,] Vz0, float[,,] p, float[,,] div, int iter, int N)
    {

    }

    void advect(int d, float[,,] Vq, float[,,] Vq0, float[,,] Vx, float[,,] Vy, float[,,] Vz, float dt, int N)
    {

    }

    void linearSolve(int b, float[,,] q, float[,,] q0, float a, float c, int iterations, int size)
    {

    }

    void resetBounds(float b, int[,,] q, int count)
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


