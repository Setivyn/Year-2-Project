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
        float[,,] Vx = cube.Vx;
        float[,,] Vy = cube.Vy;
        float[,,] Vz = cube.Vz;
        float[,,] Vx0 = cube.Vx0;
        float[,,] Vy0 = cube.Vy0;
        float[,,] Vz0 = cube.Vz0;

        float[,,] dens = cube.density;
        float[,,] d0 = cube.d0;

        //} Passer Values, allows transfer of struct properties without annoying compiler

        //{
        diffuse(1, Vx0, ref Vx, cube.diff, cube.dt, iterations, cube.count);
        diffuse(2, Vy0, ref Vy, cube.diff, cube.dt, iterations, cube.count);
        diffuse(3, Vz0, ref Vz, cube.diff, cube.dt, iterations, cube.count);

        project(ref Vx0, ref Vy0, ref Vz0, ref Vx, ref Vy, iterations, cube.count);

        advect(1, ref Vx, Vx0, Vx, Vy, Vz, cube.dt, cube.count);
        advect(2, ref Vy, Vy0, Vx, Vy, Vz, cube.dt, cube.count);
        advect(3, ref Vz, Vz0, Vx, Vy, Vz, cube.dt, cube.count);

        project(ref Vx, ref Vy, ref Vz, ref Vx0, ref Vy0, iterations, cube.count);
        //} Move Velocities
        //{
        cube.Vx = Vx;
        cube.Vy = Vy;
        cube.Vz = Vz;
        cube.Vx0 = Vx0;
        cube.Vy0 = Vy0;
        cube.Vz0 = Vz0;

        cube.density = dens;
        cube.d0 = d0;
        //} Pass Back new Values
        //{
        diffuse(0, cube.d0, ref dens, cube.visc, cube.dt, iterations, cube.size);
        advect(1, ref dens, cube.d0, cube.Vx, cube.Vy, cube.Vz, cube.dt, cube.count);
        //} Move Dye
    }

    void diffuse(int d, float[,,] Vq0, ref float[,,] Vq, float diff, float dt, int iter, int count)
    {
        float a = dt * diff * (count - 2) * (count * 2);
        linearSolve(d, ref Vq, Vq0, a, 1 + (6 * a), iter, count); 
    }

    void project(ref float[,,] Vx1, ref float[,,] Vy1, ref float[,,] Vz1, ref float[,,] p, ref float[,,] div, int iter, int N)
    {
        for (int k = 0; k < N - 1; k++)
        {
            for (int j = 0; j < N - 1; j++)
            {
                for (int i = 0; i < N - 1; i++)
                {
                    div[i, j, k] = -0.5f * (
                         Vx1[i + 1, j, k]
                        -Vx1[i - 1, j, k]
                        +Vy1[i, j + 1, k]
                        -Vy1[i, j - 1, k]
                        +Vz1[i, j, k + 1]
                        -Vz1[i, j, k - 1])/ N;
                    p[i, j, k] = 0;
                }
            }

        }

        resetBounds(0, ref div, N);
        resetBounds(0, ref p, N);
        linearSolve(0, ref p, div, 1, 6, iter, N);

        for (int k = 0; k < N - 1; k++)
        {
            for (int j = 0; j < N - 1; j++)
            {
                for (int i = 0; i < N - 1; i++)
                {
                    Vx1[i, j, k] -= 0.5f * (p[i + 1, j, k]
                                           +p[i - 1, j, k]) * N;
                    Vy1[i, j, k] -= 0.5f * (p[i, j + 1, k]
                                           +p[i, j - 1, k]) * N;
                    Vz1[i, j, k] -= 0.5f * (p[i, j, k + 1]
                                           +p[i, j, k - 1]) * N;
                }
            }

        }

        resetBounds(1, ref Vx1, N);
        resetBounds(2, ref Vy1, N);
        resetBounds(3, ref Vz1, N);

    }

    void advect(int d, ref float[,,] Vq, float[,,] Vq0, float[,,] Vx, float[,,] Vy, float[,,] Vz, float dt, int N)
    {

    }

    void linearSolve(int b, ref float[,,] q, float[,,] q0, float a, float c, int iterations, int count)
    {
        float cRecip = 1 / c;
        for(int m = 0; m < iterations; m++)
        {
            for(int k = 0; k < count - 1; k++)
            {
                for(int j = 0; j < count - 1; j++)
                {
                    for(int i = 0; i < count - 1; i++)
                    {
                        q[i, j, k] = (q0[i, j, k]
                            + a * (q[i+1, j, k] +
                                   q[i-1, j, k] +
                                   q[i, j+1, k] +
                                   q[i, j-1, k] +
                                   q[i, j, k+1] +
                                   q[i, j, k-1] )) * cRecip;
                    }
                }
            }
            resetBounds(b, ref q, count);
        }
    }

    void resetBounds(float b, ref float[,,] q, int count)
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

    public int getCubeCount()
    {
        return cubes.count;
    }

    public float getDensityAtCube(int x, int y, int z)
    {
        return cubes.density[x, y, z];
    }

}


