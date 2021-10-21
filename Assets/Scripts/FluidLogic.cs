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
    [SerializeField] [Range(1,5)]int divSizeI = 3;
    [SerializeField] [Range(0f, 1f)] float diffI;
    [SerializeField] [Range(0f, 1f)] float viscI;
    [SerializeField] [Range(1f, 100f)] float dtI;

    GridCube cubes;
    int meshSize;

    bool runSimulation;

    // Start is called before the first frame update
    void Start()
    {
        meshSize = meshObject.getSL(); 
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startStop()
    {
        if (runSimulation) { runSimulation = false; }
        else { runSimulation = true; }
            
    }

    public void initSimulation()
    {
        cubes = DefineCube(divSizeI, diffI, viscI, dtI, meshSize);
    }

    public void simulateLoop(int iterations)
    {
        while (runSimulation){
            EnactTimeStep(cubes, iterations);
            System.Threading.Thread.Sleep((int)Math.Floor(cubes.dt) + 1);
        }
    }

    GridCube DefineCube(int size, float diffusion, float viscosity, float dt, int meshSize)
    {
        int N = meshSize / size;
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

    void diffuse(int d, float[,,] Vq0, ref float[,,] Vq, float diff, float dt, int iter, int N)
    {
        float a = dt * diff * (N - 2) * (N * 2);
        linearSolve(d, ref Vq, Vq0, a, 1 + (6 * a), iter, N); 
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
        float i0, i1, j0, j1, k0, k1;

        float dtx, dty, dtz;
        dtx = dty = dtz = dt * (N - 2);

        float s0, s1, t0, t1, u0, u1;
        float tmp1, tmp2, tmp3, x, y, z;

        int i, j, k;

        for(k = 1; k < N -1; k++)
        {
            for (j = 1; j < N - 1; j++)
            {
                for (i = 1; i < N - 1; i++)
                {
                    tmp1 = dtx * Vx[i, j, k];
                    tmp2 = dty * Vy[i, j, k];
                    tmp3 = dtz * Vz[i, j, k];

                    x = i - tmp1;
                    y = j - tmp2;
                    z = k - tmp3;

                    if (x < 0.5f) { x = 0.5f; }
                    if (x > N + 0.5f) { x = N + 0.5f; }
                    i0 = Mathf.Floor(x);
                    i1 = i0 + 1f;
                    if (y < 0.5f) { y = 0.5f; }
                    if (y > N + 0.5f) { y = N + 0.5f; }
                    j0 = Mathf.Floor(y);
                    j1 = j0 + 1f;
                    if (z < 0.5f) { z = 0.5f; }
                    if (z > N + 0.5f) { z = N + 0.5f; }
                    k0 = Mathf.Floor(z);
                    k1 = k0 + 1f;

                    s1 = x - i0;
                    s0 = 1f - s1;
                    t1 = x - j0;
                    t0 = 1f - t1;
                    u1 = x - i0;
                    u0 = 1f - u1;

                    int i0I = (int)i0;
                    int i1I = (int)i1;
                    int j0I = (int)j0;
                    int j1I = (int)j1;
                    int k0I = (int)k0;
                    int k1I = (int)k1;

                    Vq[i, j, k] = (
                        s0 * (t0 * (u0 * Vq0[i0I, j0I, k0I]
                                   +u1 * Vq0[i0I, j0I, k1I])
                             +(t1 * (u0 * Vq0[i0I, j1I, k0I]
                                   + u1 * Vq0[i0I, j1I, k1I])))
                       +s1 * (t0 * (u0 * Vq0[i1I, j0I, k0I]
                                   + u1 * Vq0[i1I, j0I, k1I])
                             + (t1 * (u0 * Vq0[i1I, j1I, k0I]
                                   + u1 * Vq0[i1I, j1I, k1I])))
                        );
                }
            }
        }


        resetBounds(d, ref Vq, N);
    }

    void linearSolve(int b, ref float[,,] q, float[,,] q0, float a, float c, int iterations, int N)
    {
        float cRecip = 1 / c;
        for(int m = 0; m < iterations; m++)
        {
            for(int k = 0; k < N - 1; k++)
            {
                for(int j = 0; j < N - 1; j++)
                {
                    for(int i = 0; i < N - 1; i++)
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
            resetBounds(b, ref q, N);
        }
    }

    void resetBounds(float b, ref float[,,] q, int N)
    {
        for(int j = 1; j < N-1; j ++)
        {
            for(int i = 1; i < N - 1; i++)
            {
                q[i, j, 0    ] = b == 3 ? -q[i, j, 1    ] : q[i, j, 1    ];
                q[i, j, N - 1] = b == 3 ? -q[i, j, N - 2] : q[i, j, N - 2];
            }
        }

        for (int k = 1; k < N - 1; k++)
        {
            for (int i = 1; i < N - 1; i++)
            {
                q[i, 0, k    ] = b == 2 ? -q[i, 1, k    ] : q[i, 1, k    ];
                q[i, N - 1, k] = b == 2 ? -q[i, N - 2, k] : q[i, N - 2, k];
            }
        }

        for (int k = 1; k < N - 1; k++)
        {
            for (int j = 1; j < N - 1; j++)
            {
                q[0, j, k    ] = b == 1 ? -q[1, j, k    ] : q[1, j, k    ];
                q[N - 1, j, k] = b == 1 ? -q[N - 2, j, k] : q[N - 2, j, k];
            }
        }

        q[0, 0, 0]             = 0.33f * (q[1, 0, 0]
                                        + q[0, 1, 0]
                                        + q[0, 0, 1]);
        q[0, N - 1, 0]         = 0.33f * (q[1, N - 1, 0]
                                        + q[0, N - 2, 0]
                                        + q[0, N - 1, 1]);
        q[0, 0, N - 1]         = 0.33f * (q[1, 0, N - 1]
                                        + q[0, 1, N - 1]
                                        + q[0, 0, N - 2]);
        q[0, N - 1, N - 1]     = 0.33f * (q[1, N - 1, N - 1]
                                        + q[0, N - 2, N - 1]
                                        + q[0, N - 2, N - 2]);
        q[N - 1, 0, 0]         = 0.33f * (q[N - 2, 0, 0]
                                        + q[N - 1, 1, 0]
                                        + q[N - 1, 0, 1]);
        q[N - 1, N - 1, 0]     = 0.33f * (q[N - 2, N - 1, 0]
                                        + q[N - 1, N - 2, 0]
                                        + q[N - 1, N - 1, 1]);
        q[N - 1, 0, N - 1]     = 0.33f * (q[N - 2, N - 1, 0]
                                        + q[N - 1, N - 2, 0]
                                        + q[N - 1, N - 1, 1]);
        q[N - 1, N - 1, N - 1] = 0.33f * (q[N - 2, N - 1, N - 1]
                                        + q[N - 1, N - 2, N - 1]
                                        + q[N - 1, N - 1, N - 2]);
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


