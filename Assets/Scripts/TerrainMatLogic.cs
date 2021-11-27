using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TerMat
{
    protected double[,] altitudeMap;
    protected int[] startCent;
    protected int sideLenIndex, matSize;
    protected int seed;
    protected System.Random rand;
    protected List<int[]> Centers; //List of Centers, reset for each layer
    protected double noiseMod, heightMod; //Roughness, Steepness
    LinkBehaviour linkLogic;
    int startPow;

    public TerMat(int SidePow, int seed, double[] modifiers, LinkBehaviour linkLog)
    {
        startPow = SidePow;
        linkLogic = linkLog;
        Centers = new List<int[]>();
        rand = new System.Random(seed); //Generate a new set of random values, global to avoid repeats as using single seed for simulation
        sideLenIndex = Convert.ToInt32(Mathf.Pow(2, SidePow));
        altitudeMap = new double[(sideLenIndex) + 1, (sideLenIndex) + 1];
        startCent = new int[2];

        noiseMod = modifiers[0];
        heightMod = modifiers[1];

        //Initialise first grid values, as these are set differently.
        startCent[0] = startCent[1] = sideLenIndex / 2;
        Centers.Add(startCent);
        SetCorners(Centers[0], sideLenIndex);
        altitudeMap[Centers[0][0], Centers[0][1]] = AveragePointsCenter(sideLenIndex, Centers[0]);
        SetEdgesForCenter(Centers[0], SidePow - 1);
    }

    public void startDiSq()
    {
        linkLogic.updateMesh();
        DiamondSquare(startPow - 1);
    }

    void DiamondSquare(int sidePow)
    {

        List<int[]> TempCenters = new List<int[]>(); //Find new Centers, to be operated on for this iteration
        Centers.ForEach(delegate (int[] Center) 
        {
            TempCenters.AddRange(FindSubCenters(Center, sidePow - 1));
        });
        Centers = TempCenters;
        TempCenters = new List<int[]>();


        Centers.ForEach(delegate (int[] Center) //Sets values for all found centers, based on corners given
        {
            SetCenter(sidePow, Center);
        });

        Centers.ForEach(delegate (int[] Center) //Find & Set edges corresponding to each center
        { 
            SetEdgesForCenter(Center, sidePow - 1);
        });


        Thread.Sleep(500);
        linkLogic.updateMesh();

        if (sidePow > 1) { DiamondSquare(sidePow - 1); }
        else 
        {
            for (int y = 0; y <= sideLenIndex; y ++ )
            {
                for (int x = 0; x <= sideLenIndex; x ++)
                {
                    altitudeMap[x, y] = Mathf.Clamp((float)altitudeMap[x, y], linkLogic.getFluidCubeSize() * 2, sideLenIndex);
                }
            }
            linkLogic.finaliseMesh(); 
        }
    }

    private List<int[]> FindSubCenters(int[] center, int sidePow)
    {
        int[][] SubCenters = new int[4][];
        List<int[]> RetCenters = new List<int[]>();
        int Length = Convert.ToInt32(Mathf.Pow(2, sidePow));
        int xMod, yMod;
        double k, j;
        j = 0;

        for(int i = 0; i < 4; i++)
        {
            SubCenters[i] = new int[2];
            k = j / 2;
            xMod = (2 * (i % 2)) - 1;
            yMod = 2 * Convert.ToInt32(Math.Floor(k)) - 1;
            SubCenters[i][0] = center[0] + xMod * Length;
            SubCenters[i][1] = center[1] + yMod * Length;
            j++;

        }
        for(int i = 0; i< 4; i++)
        {
            RetCenters.Add(SubCenters[i]);
        }
        return RetCenters;
    }

    private void SetEdgesForCenter(int[] center, int sidePow)
    {
        int chunkLen = Convert.ToInt32(Mathf.Pow(2, sidePow));
        int x, y;
        for (int i = 0; i < 4; i++)
        {
            x = center[0] + (Convert.ToInt32(Mathf.Sin(Mathf.PI * i / 2)) * chunkLen);
            y = center[1] + (Convert.ToInt32(Mathf.Cos(Mathf.PI * i / 2)) * chunkLen);
            altitudeMap[x, y] = EdgeSum(x, y, chunkLen);
        }
    }

    private double EdgeSum(int x, int y, int chunkLen)
    {
        double total = 0;
        int count = 0;
        if (x + chunkLen <= sideLenIndex){total += altitudeMap[x + chunkLen, y]; count++; }
        if (y + chunkLen <= sideLenIndex){total += altitudeMap[x, y + chunkLen]; count++; }
        if (x - chunkLen >= 0){total += altitudeMap[x - chunkLen, y]; count++; }
        if (y - chunkLen >= 0){total += altitudeMap[x, y - chunkLen]; count++; }

        total = total / count;
        return total;
    }

    void SetCenter(int sideLenPow, int[] Center)
    {
        double noise;
        int chunkLen = Convert.ToInt32(Mathf.Pow(2, sideLenPow));
        noise = Math.Floor((rand.NextDouble() - 0.5) * chunkLen); //Smaller grid generate less noise, which means the grid wont experience massive spikes at random.
        altitudeMap[Center[0], Center[1]] = AveragePointsCenter(chunkLen, Center) + (noise * noiseMod);
        altitudeMap[Center[0], Center[1]] = altitudeMap[Center[0], Center[1]] < 0 ? 0 : altitudeMap[Center[0], Center[1]];
    }

    double AveragePointsCenter(int Len, int[] centerIndex)
    {
        double total = 0;
        int xMod, yMod;
        int halfLen = Len / 2;
        double k, j;
        j = 0;

        for (int i = 0; i < 4; i++)
        {
            k = j / 2;
            xMod = (2 * (i % 2)) - 1;;
            yMod = 2 * Convert.ToInt32(Math.Floor(k)) - 1;
            total += altitudeMap[centerIndex[0] + xMod * halfLen, centerIndex[1] + yMod * halfLen];
            j++;
        }
        return total / 4;
    }

    void SetCorners(int[] Center, int sideLenIndex)
    {
        int halfLen = sideLenIndex / 2;
        //Sets the Initial Corners
        altitudeMap[PointMinusLen(Center, 0, halfLen), PointMinusLen(Center, 1, halfLen)] = rand.NextDouble() * sideLenIndex * heightMod + 6;
        altitudeMap[PointMinusLen(Center, 0, halfLen), PointPlusLen(Center, 1, halfLen)] = rand.NextDouble() * sideLenIndex * heightMod + 6;
        altitudeMap[PointPlusLen(Center, 0, halfLen), PointMinusLen(Center, 1, halfLen)] = rand.NextDouble() * sideLenIndex * heightMod + 6;
        altitudeMap[PointPlusLen(Center, 0, halfLen), PointPlusLen(Center, 1, halfLen)] = rand.NextDouble() * sideLenIndex * heightMod + 6;
    }

    public TerMat(int[,] PreMadeMat)
    {
        //Implementation for later, to allow reused/custom terrain data.
    }

    public double GetCenter()
    {
        return altitudeMap[startCent[0], startCent[1]];
    }

    public double[,] GetWholeMatrix()
    {
        return altitudeMap;
    }

    public double GetMatrixAtPoint(int xPoint, int yPoint)
    {
        return altitudeMap[xPoint, yPoint];
    }

    private int PointMinusLen(int[] point, int index, int len)
    {
        return point[index] - len;
    }

    private int PointPlusLen(int[] point, int index, int len)
    {
        return point[index] + len;
    }

    public int getSL()
    {
        return sideLenIndex;
    }
}
