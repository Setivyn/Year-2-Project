using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerMat : MonoBehaviour
{
    protected double[,] altitudeMap;
    protected int[] startCent;
    protected int sideLenIndex, matSize;
    protected int seed;
    protected System.Random rand;

    public TerMat(int SidePow)
    {
        seed = Guid.NewGuid().GetHashCode(); //Stored seed to reference later, for saving purposes, also to maintain consistency throughout code
        rand = new System.Random(seed); //Generate a new set of random values, global to avoid repeats as using single seed for simulation
        sideLenIndex = Convert.ToInt32(Mathf.Pow(2, SidePow));
        altitudeMap = new double[(2 * sideLenIndex) + 1, (2 * sideLenIndex) + 1];
        matSize = (2 * sideLenIndex);
        startCent = new int[2];
        startCent[0] = sideLenIndex;
        startCent[1] = sideLenIndex;
        SetCorners(startCent, sideLenIndex);
        SetCenter(sideLenIndex, startCent);
        DiamondSquare(startCent, SidePow);
    }

    void SetCenter(int sideLenIndex, int[] Center)
    {
        double noise;
        int halfLen = sideLenIndex / 2;
        noise = Math.Floor((rand.NextDouble() - 0.5) * halfLen * 2); //Smaller grid generate less noise, which means the grid wont experience massive spikes at random.
        altitudeMap[Center[0], Center[1]] = AveragePointsCenter(halfLen, Center) + noise;
    }

    private double AveragePointsCenter(int halfLen, int[] centerIndex)
    {
        double total = 0;
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
