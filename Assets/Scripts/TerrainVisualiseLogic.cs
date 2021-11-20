using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Diagnostics.Tracing;
using System.Drawing;
using UnityEditor;
using System.Linq;

public class TerrainVisualiseLogic : MonoBehaviour
{
    LinkBehaviour linkLogic;
    [SerializeField] Material meshMater;
    [SerializeField][Range(0.0f, 2f)] double Roughness;
    [SerializeField][Range(0.0f, 0.8f)] double Steepness;
    [SerializeField][Range(1,4)] int inputLen;
    int sideLength;
    double[] modifiers;
    int cubeN;
    int sideLengthT;
    int seed;

    private void Awake()
    {
        sideLengthT = (inputLen * 2) + 3;
        //forces sidelength to be an odd power of 9 from ^5 up to ^11. these values always have a factor of 3 when 1 is added, allowing fluid sim to be easily calculated
        seed = Guid.NewGuid().GetHashCode();
        //seed = 200000;
        sideLength = Convert.ToInt32(Math.Pow(2, sideLengthT)) + 1;
    }

    void Start()
    {
        linkLogic = FindObjectOfType<LinkBehaviour>();

        //Set up mesh Components
        var MF = gameObject.AddComponent<MeshFilter>();
        var MR = gameObject.AddComponent<MeshRenderer>();
        MR.sharedMaterial = meshMater;

        //Set up Modifiers for Terrain
        modifiers = SetModifiers(Roughness, Steepness);

        //Generate Matrix and assign to mesh filter
        linkLogic.setMat(sideLengthT, seed, modifiers);

        MF.mesh = CreateMesh(sideLength);

        initSimulation();

        linkLogic.setCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private double[] SetModifiers(double roughness, double steepness)
    {
        double[] output = { roughness, steepness };
        return output;
    }

    Mesh CreateMesh(int sideLen)
    {
        Mesh outMesh = new Mesh();

        //Assign New Vertices & Set mesh
        Vector3[] newVertices;
        newVertices = SetVertices(sideLen * sideLen, sideLen);
        outMesh.vertices = newVertices;

        //Find and Set triangles
        outMesh.triangles = SetTriangles(newVertices, sideLen);

        //Standard Recalculations
        outMesh.RecalculateNormals();
        outMesh.RecalculateBounds();
        outMesh.Optimize();

        outMesh.name = "Visualiser Mesh";

        return outMesh;
    }

    
    double findMax(Vector3[] newVertices)
    {
        double max = 0;
        foreach (Vector3 Vertex in newVertices)
        {
            if (Vertex.y > max)
            {
                max = Vertex.y;
            }
        }
        return max;
    }

    int[] SetTriangles(Vector3[] newVertices, int sideLen)

    {
        int x = sideLen - 1;
        int pointer = 0;
        int[] Triangles = new int[(6 * x * x) + (4 * 6 * x) + 6]; // (sideLen - 1) ^ 2 is number of quads, each quad has 2 triangles with 3 vertices, hence the *6
        int sideLenTo2 = sideLen * sideLen;
        //Top
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < x; j++)
            {
                Triangles[pointer + 0] = (sideLen * i) + j;
                Triangles[pointer + 1] = Triangles[pointer + 3] = (sideLen * i) + j + 1;
                Triangles[pointer + 2] = Triangles[pointer + 5] = (sideLen * i) + sideLen + j;
                Triangles[pointer + 4] = (sideLen * i) + sideLen + 1 + j;
                pointer += 6;
            }
        }

        //X = 0
        for (int i = 0; i < x; i++)
        {
            Triangles[pointer] = i;
            Triangles[pointer + 1] = Triangles[pointer + 4] = sideLenTo2 + i;
            Triangles[pointer + 2] = Triangles[pointer + 3] = i + 1;
            Triangles[pointer + 5] = sideLenTo2 + i + 1;
            pointer += 6;
        }

        //Z = max
        for (int i = 0; i < x; i++)
        {
            Triangles[pointer] = (x * (i + 1)) + i;
            Triangles[pointer + 1] = Triangles[pointer + 4] = sideLenTo2 + sideLen + i;
            Triangles[pointer + 2] = Triangles[pointer + 3] = (x * (i + 2)) + i + 1;
            Triangles[pointer + 5] = sideLenTo2 + sideLen + i + 1;
            pointer += 6;
        }

        //X = max
        for (int i = 0; i < x; i++)
        {
            Triangles[pointer] = sideLenTo2 - (i + 1);
            Triangles[pointer + 1] = Triangles[pointer + 4] = sideLenTo2 + (3 * sideLen) - (i + 1);
            Triangles[pointer + 2] = Triangles[pointer + 3] =  sideLenTo2 - (i + 1) - 1;
            Triangles[pointer + 5] = sideLenTo2 + (3* sideLen) - (i + 2); 
            pointer += 6;
            
        }

        //X = 0
        for (int i = 0; i < x; i++)
        {
            Triangles[pointer] = sideLenTo2 - sideLen - (i * sideLen);
            Triangles[pointer + 1] = Triangles[pointer + 4] = sideLenTo2 + (4 * sideLen) - (i + 1);
            Triangles[pointer + 2] = Triangles[pointer + 3] = sideLenTo2 - (2 * sideLen) - (i * sideLen);
            Triangles[pointer + 5] = sideLenTo2 + (4 * sideLen) - (i + 2);
            pointer += 6;
        }
         
        //BasePlate
        Triangles[pointer] = sideLenTo2;
        Triangles[pointer + 1] = Triangles[pointer + 4] = sideLenTo2 + (4 * sideLen) - 1;
        Triangles[pointer + 2] = Triangles[pointer + 3] = sideLenTo2 + sideLen - 1;
        Triangles[pointer + 5] = sideLenTo2 + (3 * sideLen) -1;

        return Triangles;
    }

    Vector3[] SetVertices(int sideLenTo2, int sideLen)
    {
        //Declare and initialise Vector array
        Vector3[] outVectors = new Vector3[sideLenTo2 + 4 * (sideLen)]; //Vector Array for all points on matrix and the bottom edge pieces for completer mesh.
        Vector3 inputVector;

        //Store each value in matrix to the array
        int pointer = 0;
        for (int i = 0; i < sideLen; i++)
        {
            for (int j = 0; j < sideLen; j++)
            {
                inputVector.x = i;
                inputVector.y = (float)linkLogic.matAtXY(i, j);
                inputVector.z = j;
                outVectors[pointer] = inputVector;
                pointer++;
            }
        }

        //Store completer mesh edges, 
        //CW MOVEMENT FROM 0,0
        pointer = sideLenTo2;
        for(int i = 0; i < sideLen; i++)
        {
            inputVector.z = i;
            inputVector.y = inputVector.x = 0;
            outVectors[pointer] = inputVector;
            pointer += 1;
        }

        for (int i = 0; i < sideLen; i++)
        {
            inputVector.x = i;
            inputVector.y = 0;
            inputVector.z = sideLen - 1;
            outVectors[pointer] = inputVector;
            pointer += 1;
            
        }

        for (int i = 0; i < sideLen; i++)
        {
            inputVector.z = i;
            inputVector.y = 0;
            inputVector.x = sideLen - 1;
            outVectors[pointer] = inputVector;
            pointer += 1;
        }

        for (int i = 0; i < sideLen; i++)
        {
            inputVector.z = 0;
            inputVector.y = 0;
            inputVector.x = i;
            outVectors[pointer] = inputVector;
            pointer += 1;
        }

        return outVectors;
    }


    //FLUID INTERFACE

    void initSimulation()
    {
        linkLogic.initSim(sideLength);
        cubeN = linkLogic.getFluidCubeCount();
        linkLogic.addDensToFluid(cubeN - 2, 100);
        
    }

    public void changeSimState(int iterations)
    {
        linkLogic.addVelToFluid(cubeN - 1, 0, (sideLength * Math.Pow(1 + Steepness, 3)), 0);
        linkLogic.startStopSim(iterations);

    }


    public void SetColours(double[,,] values, int N)
    {
        double[] values1D = new double[(sideLength * sideLength) + (4 * sideLength)];
        int pointer = 0;
        int size = linkLogic.getFluidCubeSize();
        int x, y, z;
        double val;
        for (int k = 0; k < sideLength; k++)
        {
            
            for (int i = 0; i < sideLength; i++)
            {
                x = (int)(i / size);
                z = (int)(k / size);

                y = (int)linkLogic.matAtXY(i, k) / size;

                val = values[x, y, z];

                values1D[pointer] = val;

                pointer += 1;
            }
        }

        gameObject.GetComponent<MeshFilter>().mesh.colors32 = calcColours(values1D.Max(), values1D);
    }

    

    Color32[] calcColours(double maxVal, double[] values)
    {
        Color32[] newCols = new Color32[values.Length];
        int pointer = 0;

        foreach(double v in values)
        {
            newCols[pointer] = UnityEngine.Color.HSVToRGB(1 - findDiff(maxVal, v), 0.5f, 1);
            pointer += 1;
        }


        return newCols;
    }

    private float findDiff(double maxVal, double current)
    {
        double diff = maxVal - current;
        float output = Convert.ToSingle(Math.Abs(diff) / maxVal);
        output = Mathf.Clamp(output, 0.15f, 1f);
        return output;
    }

    public int getSeed()
    {
        return seed;
    }
}

