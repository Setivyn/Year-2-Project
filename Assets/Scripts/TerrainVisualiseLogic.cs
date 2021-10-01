using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Diagnostics.Tracing;
using System.Drawing;
using UnityEditor;

public class TerrainVisualiseLogic : MonoBehaviour
{
    TerMat Matrix;
    [SerializeField] Material meshMater;
    [SerializeField][Range(0.0f, 2f)] double Roughness;
    [SerializeField][Range(0.0f, 2f)] double Steepness;
    [SerializeField] int sideLength;
    double[] modifiers;
    Vector3[] vertices;

    void Start()
    {
        int lenFull = Convert.ToInt32(Math.Pow(2, sideLength) + 1);
        int seed = Guid.NewGuid().GetHashCode();

        //Set up mesh Components
        var MF = gameObject.AddComponent<MeshFilter>();
        var MR = gameObject.AddComponent<MeshRenderer>();
        MR.sharedMaterial = meshMater;

        //Set up Modifiers for Terrain
        Debug.Log(Roughness);
        modifiers = SetModifiers(Roughness, Steepness);

        //Generate Matrix and assign to mesh filter
        Matrix = new TerMat(sideLength, seed, modifiers);
        MF.mesh = CreateMesh(Matrix, lenFull);
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
        vertices = newVertices;
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
        int pointer = 0;
        int[] Triangles = new int[6 * (sideLen - 1) * (sideLen - 1)]; // (sideLen - 1) ^ 2 is number of quads, each quad has 2 triangles with 3 vertices, hence the *6

        //Set numbers for each quad (2 triangles)
        for (int i = 0; i < sideLen - 1; i++)
        {
            for (int j = 0; j < sideLen - 1; j++)
            {
                Triangles[pointer + 0] = (sideLen * i) + j;
                Triangles[pointer + 1] = Triangles[pointer + 3] = (sideLen * i) + j + 1;
                Triangles[pointer + 2] = Triangles[pointer + 5] = (sideLen * i) + sideLen + j;
                Triangles[pointer + 4] = (sideLen * i) + sideLen + 1 + j;
                pointer += 6;
            }
        }

        //IMPLEMENT: New Triangles, that connect the base vertices to the edges of the terra above them. 
        //Map out which indices need to be with which. should be similar to above.
        //TRIANGLES ASSIGN ANTICLOCKWISE FROM INSIDE e.g. for a triangle of vertices index 0,1,9, assign in that order
        //use IF from vertices method

        return Triangles;
    }

    Vector3[] SetVertices(TerMat input, int sideLenTo2, int sideLen)
    {
        //Declare and initialise Vector array
        Vector3[] outVectors = new Vector3[sideLenTo2 + 4 * (sideLen-1)]; //Vector Array for all points on matrix and the bottom edge pieces for completer mesh.

        //Store each value in matrix to the array
        int pointer = 0;
        for (int i = 0; i < sideLen; i++)
        {
            for (int j = 0; j < sideLen; j++)
            {
                Vector3 inputVector;
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
            for (int j = 0; j < sideLen; j++) {
                if ((i == 0 || j == 0) || i == sideLen - 1 || j == sideLen - 1) {
                    Vector3 inputVector;
                    Debug.Log(i + ", " + j);
                    inputVector.x = i;
                    inputVector.y = 0;
                    inputVector.z = j;
                    outVectors[pointer] = inputVector;
                    pointer += 1;
                }
            }
        }

        return outVectors;
    }

    public void SetColours(double maxVal, double[] values)
    {
        gameObject.GetComponent<MeshFilter>().mesh.colors32 = calcColours(maxVal, values, vertices, sideLength);
    }

    Color32[] calcColours(double maxVal, double[] values, Vector3[] vertexList, int sidePower)
    {
        throw new NotImplementedException();
    }
}

