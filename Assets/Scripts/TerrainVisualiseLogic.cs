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
    TerMat Matrix;
    [SerializeField] FluidLogic CFDLogic;
    [SerializeField] UIBehaviour UILogic;
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
        sideLength = Convert.ToInt32(Math.Pow(2, sideLengthT)) + 1;
    }

    void Start()
    {
        UILogic = FindObjectOfType<UIBehaviour>();

        //Set up mesh Components
        var MF = gameObject.AddComponent<MeshFilter>();
        var MR = gameObject.AddComponent<MeshRenderer>();
        MR.sharedMaterial = meshMater;

        //Set up Modifiers for Terrain
        modifiers = SetModifiers(Roughness, Steepness);

        //Generate Matrix and assign to mesh filter
        Matrix = new TerMat(sideLengthT, seed, modifiers);
        MF.mesh = CreateMesh(Matrix, sideLength);

        initSimulation();

        UILogic.setCamera();
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

    Mesh CreateMesh(TerMat input, int sideLen)
    {
        Mesh outMesh = new Mesh();

        //Assign New Vertices & Set mesh
        Vector3[] newVertices;
        newVertices = SetVertices(input, sideLen * sideLen, sideLen);
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

    public double matAtXY(int x, int y)
    {
        return Matrix.GetMatrixAtPoint(x, y);
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

    Vector3[] SetVertices(TerMat input, int sideLenTo2, int sideLen)
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
                inputVector.y = (float)input.GetMatrixAtPoint(i, j);
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

    public int getSL()
    {
        return sideLength;
    }

    //FLUID INTERFACE

    void initSimulation()
    {
        CFDLogic.initSimulation(sideLength);
        cubeN = CFDLogic.getCubeCount();
        for (int j = 1; j < cubeN - 1; j++)
        {
            for (int i = 1; i < cubeN - 1; i++)
            {
                CFDLogic.addDToCube(i, cubeN - 2, j, 100);
            }
        }
        Debug.Log("D added");
        
    }

    public void changeSimState(int iterations)
    {
        CFDLogic.addVToCube(cubeN - 1, 0, (-sideLength * Math.Pow(Steepness, 2)), 0);
        CFDLogic.startStop(iterations);

    }


    public double[] findValues(ref double maxVal, TerMat matrix)
    {
        int cubeN = CFDLogic.getCubeCount();
        int cubeN2 = cubeN ^ 2;
        double[,,] densities = recieveD();
        int matY;

        double[] output = new double[cubeN2];
        for (int i = 0; i < sideLength; i++)
        {
            for(int j = 0; j < sideLength; j++)
            {
                matY = (int)Math.Floor(matrix.GetMatrixAtPoint(i, j));
                output[(i * sideLength) + j] = densities[i, matY, j];
            }
        }
        maxVal = output.Max();

        return output;
    }

    public void SetColours(double[,,] values, int N)
    {
        double[] values1D = new double[(sideLength * sideLength) + (4 * sideLength)];
        int pointer = 0;
        int size = CFDLogic.getCubeSize();
        for(int k = 0; k < N; k++)
        {
            for(int j = 0; j < N; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    if (j * size == Math.Ceiling(Matrix.GetMatrixAtPoint(i * size, k * size))) 
                    { 
                        
                        values1D[pointer] = values[i, j, k];
                        pointer += 1;
                    }
                }
            }
        }

        gameObject.GetComponent<MeshFilter>().mesh.colors32 = calcColours(values1D.Max(), values1D);
    }

    Color32[] calcColours(double maxVal, double[] values)
    {
        Color32[] newCols = new Color32[values.Length];
        int pointer = 0;

        foreach(float v in values)
        {
            newCols[pointer] = UnityEngine.Color.HSVToRGB(1 - findDiff(maxVal, v), 0.5f, 1);
            pointer += 1;
        }


        return newCols;
    }

    private float findDiff(double maxVal, double current)
    {
        float output = Convert.ToSingle((maxVal - current) / ((maxVal + current) / 2));
        output = Mathf.Clamp(output, 0.15f, 1);
        return output;
    }

    double[,,] recieveD()
    {
        int cubeN = CFDLogic.getCubeCount();

        double[,,] densOut = new double[cubeN, cubeN, cubeN];


        for (int k = 0; k < cubeN; k++)
        {
            for (int j = 0; j < cubeN; j++)
            {
                for (int i = 0; i < cubeN; i++)
                {
                    densOut[i, j, k] = CFDLogic.getDensityAtCube(i, j, k);
                }
            }
        }

        return densOut;
    }
}

