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
    [SerializeField] Material meshMat;

    void Start()
    {
        int lenPow = 5;
        int lenFull = Convert.ToInt32(Math.Pow(2, lenPow) + 1);
        int seed = Guid.NewGuid().GetHashCode();

        //Set up mesh Components
        var MF = gameObject.AddComponent<MeshFilter>();
        var MR = gameObject.AddComponent<MeshRenderer>();
        MR.sharedMaterial = meshMat;
        //Generate Matrix and assign to mesh filter
        Matrix = new TerMat(lenPow, seed);
        MF.mesh = CreateMesh(Matrix, lenFull);
    }

    // Update is called once per frame
    void Update()
    {

    }

    Mesh CreateMesh(TerMat input, int sideLen)
    {
        Mesh outMesh = new Mesh();
        double maxY;

        //Assign New Vertices & Set mesh
        Vector3[] newVertices;
        newVertices = SetVertices(input, sideLen * sideLen, sideLen);
        outMesh.vertices = newVertices;
        maxY = findMax(newVertices);

        //Find and Set triangles
        outMesh.triangles = SetTriangles(newVertices, sideLen);

        //Set Colours based on height [Temporary, for proof of concept.]
        outMesh.colors32 = SetColours(newVertices, maxY);

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

    Color32[] SetColours(Vector3[] newVertices, double max)
    {
        Color32[] outCol = new Color32[newVertices.Length];
        double diff;
        byte value;

        for (int i = 0; i < newVertices.Length; i++)
        {
            diff = Math.Abs(max - newVertices[i].y) / ((max + newVertices[i].y) / 2); //% difference between current value and maximum
            if (diff > 1)
            {
                diff = 1;
            }
            value = Convert.ToByte(255 - Math.Floor(255 * diff));

            outCol[i].a = 255;
            outCol[i].r = 255;
            outCol[i].g = outCol[i].b = value;
        }

        return outCol;
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

        return Triangles;
    }

    Vector3[] SetVertices(TerMat input, int sideLenTo2, int sideLen)
    {
        //Declare and initialise Vector array
        Vector3[] outVectors = new Vector3[sideLenTo2];

        //Store each value in matrix to the lists
        int pointer = 0;
        for (int i = 0; i < sideLen; i++)
        {
            for (int j = 0; j < sideLen; j++)
            {
                Vector3 inputVector;
                inputVector.x = i;
                inputVector.y = Convert.ToInt32(input.GetMatrixAtPoint(i, j));
                inputVector.z = j;
                outVectors[pointer] = inputVector;
                pointer++;
            }
        }

        return outVectors;
    }
}

