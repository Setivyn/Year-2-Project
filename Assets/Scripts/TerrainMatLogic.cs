using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerMat
{
    protected double[,] altitudeMap;
    protected int[] startCent;
    protected int sideLenIndex, matSize;
    protected int seed;
    protected System.Random rand;
    protected List<int[]> Centers; //List of Centers, reset for each layer

    public TerMat(int SidePow, int seed)
    {
        Centers = new List<int[]>();
        rand = new System.Random(seed); //Generate a new set of random values, global to avoid repeats as using single seed for simulation
        sideLenIndex = Convert.ToInt32(Mathf.Pow(2, SidePow));
        altitudeMap = new double[(sideLenIndex) + 1, (sideLenIndex) + 1];
        matSize = (2 * sideLenIndex);
        startCent = new int[2];



        //Initialise first grid values, as these are set differently.
        startCent[0] = sideLenIndex / 2;
        startCent[1] = startCent[0];
        Centers.Add(startCent);
        SetCorners(Centers[0], sideLenIndex);
        altitudeMap[Centers[0][0], Centers[0][1]] = AveragePointsCenter(sideLenIndex, Centers[0]);


        DiamondSquare(SidePow - 1);
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

        Centers.ForEach(delegate (int[] Center) { //Find & Set edges corresponding to each center
            SetEdgesForCenter(Center, sidePow - 1);
        });

        if (sidePow != 0) {DiamondSquare(sidePow - 1);}
    }

    private List<int[]> FindSubCenters(int[] center, int sidePow)
    {
        List<int[]> SubCenters = new List<int[]>();
        int[] TempCenter = new int[2];
        int Length = Convert.ToInt32(Mathf.Pow(2, sidePow));
        int xMod, yMod;
        double k, j;
        j = 0;

        for(int i = 0; i < 4; i++)
        {
            k = j / 2;
            xMod = (2 * (i % 2)) - 1;
            yMod = 2 * Convert.ToInt32(Math.Floor(k)) - 1;
            TempCenter[0] = center[0] + xMod * Length;
            TempCenter[1] = center[1] + yMod * Length;
            SubCenters.Add(TempCenter);
            j++;

        }

        return SubCenters;
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
        if (x + chunkLen <= sideLenIndex){total += altitudeMap[x + chunkLen, y]; }
        if (y + chunkLen <= sideLenIndex){total += altitudeMap[x, y + chunkLen];}
        if (x - chunkLen >= 0){total += altitudeMap[x - chunkLen, y];}
        if (y - chunkLen >= 0){total += altitudeMap[x, y - chunkLen];}

        total = total / 4;
        return total;
    }

    void SetCenter(int sideLenPow, int[] Center)
    {
        double noise;
        int chunkLen = Convert.ToInt32(Mathf.Pow(2, sideLenPow));
        noise = Math.Floor((rand.NextDouble() - 0.5) * chunkLen * 2); //Smaller grid generate less noise, which means the grid wont experience massive spikes at random.
        altitudeMap[Center[0], Center[1]] = AveragePointsCenter(chunkLen, Center) + noise;
    }

    double AveragePointsCenter(int Len, int[] centerIndex)
    {
        double total = 0;
        int halfLen = Len / 2;
        total += altitudeMap[PointMinusLen(centerIndex, 0, halfLen), PointMinusLen(centerIndex, 1, halfLen)];
        total += altitudeMap[PointMinusLen(centerIndex, 0, halfLen), PointPlusLen(centerIndex, 1, halfLen)];
        total += altitudeMap[PointPlusLen(centerIndex, 0, halfLen), PointMinusLen(centerIndex, 1, halfLen)];
        total += altitudeMap[PointPlusLen(centerIndex, 0, halfLen), PointPlusLen(centerIndex, 1, halfLen)];
        return total / 4;
    }

    void SetCorners(int[] Center, int sideLenIndex)
    {
        int halfLen = sideLenIndex / 2;
        //Sets the Initial Corners
        altitudeMap[PointMinusLen(Center, 0, halfLen), PointMinusLen(Center, 1, halfLen)] = rand.NextDouble() * matSize * 1.5;
        altitudeMap[PointMinusLen(Center, 0, halfLen), PointPlusLen(Center, 1, halfLen)] = rand.NextDouble() * matSize * 1.5;
        altitudeMap[PointPlusLen(Center, 0, halfLen), PointMinusLen(Center, 1, halfLen)] = rand.NextDouble() * matSize * 1.5;
        altitudeMap[PointPlusLen(Center, 0, halfLen), PointPlusLen(Center, 1, halfLen)] = rand.NextDouble() * matSize * 1.5;
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

    public int GetSeed()
    {
        return seed;
    }
}
