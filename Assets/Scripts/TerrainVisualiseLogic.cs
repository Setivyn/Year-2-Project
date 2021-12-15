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
    int cubeN;

    private void Awake()
    {
        linkLogic = FindObjectOfType<LinkBehaviour>();
    }
    void Start()
    {
        
    }

    public void initMeshObject()
    {
        
        //Set up mesh Components

        var MR = gameObject.AddComponent<MeshRenderer>();
        var MF = gameObject.AddComponent<MeshFilter>();

        MF.mesh = CreateMesh(linkLogic.getSL() + 1);

        MR.sharedMaterial = meshMater;

    }
    

    public void finaliseMesh()
    {
        MeshFilter MF = gameObject.GetComponent<MeshFilter>();
        MF.mesh = CreateMesh(linkLogic.getSL() + 1);

        var MC = gameObject.AddComponent<MeshCollider>().sharedMesh = MF.mesh;


        linkLogic.setCamera();
    }

    Mesh CreateMesh(int sideLen)
    {
        Mesh outMesh = new Mesh();

        //Assign New Vertices
        outMesh.vertices = SetVertices(sideLen * sideLen, sideLen);
        //Find and Set triangles
        outMesh.triangles = SetTriangles(sideLen);

        //Ensure Unity Renderers has the additional information necessary to create the mesh in gameSpace.
        outMesh.RecalculateNormals();
        outMesh.RecalculateBounds();
        outMesh.Optimize();

        outMesh.name = "Visualiser Mesh";

        return outMesh;
    }

    int[] SetTriangles(int sideLen)

    {
        int x = sideLen - 1;
        int pointer = 0;
        int[] Triangles = new int[(6 * x * x) + (4 * 6 * x) + 6]; // (sideLen - 1) ^ 2 is number of quads, each quad has 2 triangles with 3 vertices, hence the *6
        int sideLenTo2 = sideLen * sideLen;
        
        //Set quads for terrain. Each is made of reference pointers into the Vertices array.
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
        //Declare and initialise Vector array, for Mesh Vertices
        Vector3[] outVectors = new Vector3[sideLenTo2 + 4 * (sideLen)]; //Vector Array for all points on matrix and the bottom edge pieces for completer mesh.
        Vector3 inputVector;

        //Store each value in Terrain matrix to the array
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

        //"Complete" the cube, with sides and a base, so it looks like a solid object.
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


    public void SetColours(double[,,] values, int N)
    {
        int sideLength = linkLogic.getSL();
        double[] values1D = new double[(sideLength + 1)*(sideLength + 1) + 4 * (sideLength + 1)];
        int pointer = 0;
        int size = linkLogic.getFluidCubeSize();
        int x, y, z;
        double val;
        for (int k = 0; k < sideLength; k++)
        {
            
            for (int i = 0; i < sideLength; i++)
            {
                //Converts 3Dimensional array into a 1Dimensional format.
                x = (int)Math.Floor((double)i / size);
                z = (int)Math.Floor((double)k / size);

                y = (int)linkLogic.matAtXY(i, k) / size;
                //Debug.Log("x: " + i + "; " + x + ", z: " + k + "; " + z + ", y: " + y + ", ptn: " + pointer) ;
                val = values[x, y, z];

                values1D[pointer] = val;

                pointer += 1;
            }
        }
        //Sets the new mesh colours, based on calculated difference
        gameObject.GetComponent<MeshFilter>().mesh.colors32 = calcColours(values1D.Max(), values1D);
    }

    

    Color32[] calcColours(double maxVal, double[] values)
    {
        Color32[] newCols = new Color32[values.Length];
        int pointer = 0;

        foreach(double v in values)
        {
            //Converts Density to a relative value between 1 and 0.15
            newCols[pointer] = UnityEngine.Color.HSVToRGB(1 - findDiff(maxVal, v), 0.5f, 1);
            pointer += 1;
        }


        return newCols;
    }

    private float findDiff(double maxVal, double current)
    {
        //Finds the % difference between a current value and the maximum.
        double diff = maxVal - current;
        float output = Convert.ToSingle(Math.Abs(diff) / maxVal);
        output = Mathf.Clamp(output, 0.15f, 1f);
        return output;
    }


}

