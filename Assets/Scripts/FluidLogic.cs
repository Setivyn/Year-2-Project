using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct GridCube //whole grid structure, each index is smaller cubes of size (int size)
{
    public int size { get; set; } //Cube size, Time step, Diffusion Rate, Viscosity
    public double dt { get; set; }
    public double diff { get; set; }
    public double visc { get; set; }
    public int count { get; set; }

    public double[,,] density { get; set; }  //Density, Prev Density
    public double[,,] d0 { get; set; }

    public double[,,] Vx { get; set; } //V, Prev V
    public double[,,] Vy { get; set; }
    public double[,,] Vz { get; set; }

    public double[,,] Vx0 { get; set; }
    public double[,,] Vy0 { get; set; }
    public double[,,] Vz0 { get; set; }
}

public class FluidLogic : MonoBehaviour
{
    LinkBehaviour linkLogic;
    [SerializeField] [Range(1,5)]int divSizeI = 3;
    [SerializeField] [Range(0f, 1f)] double diffI;
    [SerializeField] [Range(0f, 1f)] double viscI;
    [SerializeField] [Range(1f, 100f)] double dtI;

    GridCube cubes;

    bool runSimulation = false;

    double IVx, IVy, IVz;
    bool addVBool;
    int Iy;

    // Start is called before the first frame update
    void Start()
    {
        linkLogic = FindObjectOfType<LinkBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startStop(int iterations)
    {
        if (runSimulation) { runSimulation = false; linkLogic.setColours(cubes.density, cubes.count); }
        else { runSimulation = true; simulateLoop(iterations); }
            
    }

    public void initSimulation(int meshSize)
    {
        cubes = DefineCube(divSizeI, diffI, viscI, dtI, meshSize);
    }

    void simulateLoop(int iterations)
    {
        if (runSimulation){
            for(int i = 0; i < 100; i ++)
            {
                EnactTimeStep(cubes, iterations);
                //System.Threading.Thread.Sleep((int)Math.Floor(cubes.dt) + 1);
            }
            startStop(iterations);
        }
        
    }

    GridCube DefineCube(int size, double diffusion, double viscosity, double dt, int meshSize)
    {
        int N = meshSize / size;

        GridCube newCube = new GridCube();
        newCube.size = size;
        newCube.dt = dt;
        newCube.diff = diffusion;
        newCube.visc = viscosity;
        newCube.count = N;

        newCube.d0 = new double[N, N, N];
        newCube.density = new double[N, N, N];

        newCube.Vx = new double[N, N, N];
        newCube.Vy = new double[N, N, N];
        newCube.Vz = new double[N, N, N];

        newCube.Vx0 = new double[N, N, N];
        newCube.Vy0 = new double[N, N, N];
        newCube.Vz0 = new double[N, N, N];

        return newCube;
    }

    void EnactTimeStep(GridCube cube, int iterations)
    {
        //{
        double[,,] Vx = cube.Vx;
        double[,,] Vy = cube.Vy;
        double[,,] Vz = cube.Vz;
        double[,,] Vx0 = cube.Vx0;
        double[,,] Vy0 = cube.Vy0;
        double[,,] Vz0 = cube.Vz0;

        double[,,] dens = cube.density;
        double[,,] d0 = cube.d0;

        //} Passer Values, allows transfer of struct properties without annoying compiler

        if (addVBool)
        {
            for (int j = 1; j < cube.count - 1; j++)
            {
                for (int i = 1; i < cube.count - 1; i++)
                {
                    Vx[i, Iy, j] += IVx;
                    Vy[i, Iy, j] += IVy;
                    Vz[i, Iy, j] += IVz;
                }
            }
        }

        //{
        diffuse(1, ref Vx0, ref Vx, cube.diff, cube.dt, iterations, cube.count);
        diffuse(2, ref Vy0, ref Vy, cube.diff, cube.dt, iterations, cube.count);
        diffuse(3, ref Vz0, ref Vz, cube.diff, cube.dt, iterations, cube.count);

        project(ref Vx0, ref Vy0, ref Vz0, ref Vx, ref Vy, iterations, cube.count);

        advect(1, ref Vx, Vx0, Vx, Vy, Vz, cube.dt, cube.count);
        advect(2, ref Vy, Vy0, Vx, Vy, Vz, cube.dt, cube.count);
        advect(3, ref Vz, Vz0, Vx, Vy, Vz, cube.dt, cube.count);

        project(ref Vx, ref Vy, ref Vz, ref Vx0, ref Vy0, iterations, cube.count);
        //} Move Velocities

        //{
        //Debug.Log("d: " + dens[1, cube.count - 2, 1]);
        diffuse(0, ref d0, ref dens, cube.visc, cube.dt, iterations, cube.count);
        //Debug.Log("2: " + dens[1, cube.count - 2, 1]);
        advect(1, ref dens, d0, cube.Vx, cube.Vy, cube.Vz, cube.dt, cube.count);
        //Debug.Log("3: " + dens[1, cube.count - 2, 1]);
        //} Move Dye

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
    }

    void diffuse(int d, ref double[,,] q0, ref double[,,] q, double diff, double dt, int iter, int N)
    {

        double a = dt * diff * (N - 2) * (N - 2);
        // Debug.Log("Constant A: " + a);
        linearSolve(d, ref q0, q, a, 1 + (6 * a), iter, N);
    }

    void project(ref double[,,] Vx1, ref double[,,] Vy1, ref double[,,] Vz1, ref double[,,] p, ref double[,,] div, int iter, int N)
    {
        for (int k = 1; k < N - 1; k++)
        {
            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
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

        resetBounds(0, ref div, N); resetBounds(0, ref p, N);
        linearSolve(0, ref p, div, 1, 6, iter, N);

        for (int k = 1; k < N - 1; k++)
        {
            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    Vx1[i, j, k] -= 0.5f * (p[i + 1, j, k]
                                           -p[i - 1, j, k]) * N;
                    Vy1[i, j, k] -= 0.5f * (p[i, j + 1, k]
                                           -p[i, j - 1, k]) * N;
                    Vz1[i, j, k] -= 0.5f * (p[i, j, k + 1]
                                           -p[i, j, k - 1]) * N;
                }
            }

        }

        resetBounds(1, ref Vx1, N); resetBounds(2, ref Vy1, N); resetBounds(3, ref Vz1, N);

    }

    void advect(int d, ref double[,,] Vq, double[,,] Vq0, double[,,] Vx, double[,,] Vy, double[,,] Vz, double dt, int N)
    {
        double i0, i1, j0, j1, k0, k1;

        double dt0;
        dt0 = dt * (N - 2);

        double s0, s1, t0, t1, u0, u1, x, y, z;

        int i, j, k;

        for(k = 1; k < N - 2; k++)
        {
            for (j = 1; j < N - 2; j++)
            {
                for (i = 1; i < N - 2; i++)
                {

                    x = i - (dt0 * Vx[i, j, k]);
                    y = j - (dt0 * Vy[i, j, k]);
                    z = k - (dt0 * Vz[i, j, k]);

                    x = clampAdv(N, out i0, out i1, x);
                    y = clampAdv(N, out j0, out j1, y);
                    z = clampAdv(N, out k0, out k1, z);

                    calcCoeffAdv(i0, out s0, out s1, x);
                    calcCoeffAdv(j0, out t0, out t1, y);
                    calcCoeffAdv(k0, out u0, out u1, z);

                    int i0I = (int)i0;
                    i0I = i0I > N - 1 ? N - 1 : i0I;
                    int i1I = (int)i1;
                    i1I = i1I > N - 1 ? N - 1 : i1I;
                    int j0I = (int)j0;
                    j0I = j0I > N - 1 ? N - 1 : j0I;
                    int j1I = (int)j1;
                    j1I = j1I > N - 1 ? N - 1 : j1I;
                    int k0I = (int)k0;
                    k0I = k0I > N - 1 ? N - 1 : k0I;
                    int k1I = (int)k1;
                    k1I = k1I > N - 1 ? N - 1 : k1I;

                    

                    Vq[i, j, k] = (
                        s0 * (t0 * (u0 * Vq0[i0I, j0I, k0I]
                                   + u1 * Vq0[i0I, j0I, k1I])
                             + (t1 * (u0 * Vq0[i0I, j1I, k0I]
                                   + u1 * Vq0[i0I, j1I, k1I])))
                       + s1 * (t0 * (u0 * Vq0[i1I, j0I, k0I]
                                   + u1 * Vq0[i1I, j0I, k1I])
                             + (t1 * (u0 * Vq0[i1I, j1I, k0I]
                                   + u1 * Vq0[i1I, j1I, k1I])))
                        );
                }
            }
        }
        resetBounds(d, ref Vq, N);
    }

    private static void calcCoeffAdv(double i0, out double s0, out double s1, double x)
    {
        s1 = x - i0;
        s0 = 1f - s1;
    }

    private static double clampAdv(int N, out double a0, out double a1, double q)
    {
        if (q < 0.5f) { q = 0.5f; }
        if (q > N + 0.5f) { q = N + 0.5f; }
        a0 = Math.Floor(q);
        a1 = a0 + 1f;
        return q;
    }

    void linearSolve(int dimension, ref double[,,] q, double[,,] q0, double a, double c, int iterations, int N)
    {
        double cRecip = 1 / c;
        for(int m = 0; m < iterations; m++)
        {
            for(int j = N - 2; j > 0; j--)
            {
                for(int k = 1; k < N - 1; k++)
                {
                    for(int i = 1; i < N - 1; i++)
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
            resetBounds(dimension, ref q, N);
        }
    }

    void resetBounds(int dimension, ref double[,,] q, int N)
    {
        
        for(int j = 1; j < N - 2; j ++)
        {
            for(int i = 1; i < N - 2; i++)
            {
                q[i, j, 0    ] = dimension == 3 ? -q[i, j, 1    ] : q[i, j, 1    ];
                q[i, j, N - 1] = dimension == 3 ? -q[i, j, N - 2] : q[i, j, N - 2];
            }
        }

        for (int k = 1; k < N - 2; k++)
        {
            for (int i = 1; i < N - 2; i++)
            {
                q[i, 0, k    ] = dimension == 2 ? -q[i, 1, k    ] : q[i, 1, k    ];
                q[i, N - 1, k] = dimension == 2 ? -q[i, N - 2, k] : q[i, N - 2, k];
                
            }
        }

        for (int k = 1; k < N - 2; k++)
        {
            for (int j = 1; j < N - 2; j++)
            {
                q[0, j, k    ] = dimension == 1 ? -q[1, j, k    ] : q[1, j, k    ];
                q[N - 1, j, k] = dimension == 1 ? -q[N - 2, j, k] : q[N - 2, j, k];
            }
        }

        for (int k = 1; k < N - 2; k++)
        {
            for (int i = 1; i < N - 2; i++)
            {
                int y = (int)linkLogic.matAtXY(i * cubes.size, k * cubes.size) / cubes.size;
                //Debug.Log("y: " + y + "@ x: " + i + ", z: " + k);
                q[i, y, k] = dimension == 1 && directionCheck(q[i, y, k], i, k, dimension) ? q[i + 1, y, k] : -q[i + 1, y, k];
                q[i, y, k] = dimension == 3 && directionCheck(q[i, y, k], i, k, dimension) ? q[i, y, k + 1] : -q[i, y, k + 1];
                //weighted average of velocity and terrain gradient -> needs implementing
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

    private bool directionCheck(double qAtIK, int i, int k, int dimension)
    {
        int iMat = i * cubes.size;
        int kMat = k * cubes.size;
        double matAtIk = dimension == 1 ? linkLogic.matAtXY(iMat - 1, kMat) - linkLogic.matAtXY(iMat + 1, kMat) : linkLogic.matAtXY(iMat, kMat - 1) - linkLogic.matAtXY(iMat, kMat + 1);
        bool boolOut = (qAtIK < 0 && matAtIk < 0 ) | (qAtIK >= 0 && matAtIk >= 0) ? true : false;
        return boolOut;
    }

    public void addDToCube(int x, int y, int z, double amount)
    {
        cubes.density[x,y,z] += amount;
    }

    public void addVToCube(int y, double Vx1, double Vy1, double Vz1)
    {
        addVBool = true;
        IVx = Vx1;
        IVy = Vy1;
        IVz = Vz1;

        Iy = y;
    }

    public void ceaseVelAdd()
    {
        addVBool = false;
    }

    public int getCubeCount()
    {
        return cubes.count;
    }

    public double getDensityAtCube(int x, int y, int z)
    {
        return cubes.density[x, y, z];
    }

    public int getCubeSize()
    {
        return cubes.size;
    }

}


