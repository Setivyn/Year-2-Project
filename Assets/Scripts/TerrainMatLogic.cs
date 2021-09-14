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
    protected List<int[]> Centers; //List of Centers, reset for each layer

    public TerMat(int SidePow)
    {
        Centers = new List<int[]>();
        seed = Guid.NewGuid().GetHashCode(); //Stored seed to reference later, for saving purposes, also to maintain consistency throughout code
        rand = new System.Random(seed); //Generate a new set of random values, global to avoid repeats as using single seed for simulation
        sideLenIndex = Convert.ToInt32(Mathf.Pow(2, SidePow));
        altitudeMap = new double[(2 * sideLenIndex) + 1, (2 * sideLenIndex) + 1];
        matSize = (2 * sideLenIndex);



        //Initialise first grid values, as these are set differently.
        SetCorners(startCent, sideLenIndex);
        startCent[0] = sideLenIndex / 2;
        startCent[1] = startCent[0];
        Centers.Add(startCent);
        altitudeMap[Centers[0][0], Centers[0][1]] = AveragePointsCenter(sideLenIndex / 2, Centers[0]);


        DiamondSquare(SidePow - 1);
    }

    void DiamondSquare(int sidePow)
    {
        List<int[]> TempCenters = new List<int[]>(); //Find new Centers, to be operated on for this iteration
        Centers.ForEach(delegate (int[] Center) {
            TempCenters = FindSubCenters(Center, sidePow);
        });
        Centers = TempCenters;
        TempCenters = new List<int[]>();


        Centers.ForEach(delegate (int[] Center) //Sets values for all found centers, based on corners given
        {
            SetCenter(sidePow, Center);
        });

        Centers.ForEach(delegate (int[] Center) { //Find & Set edges corresponding to each center
            SetEdgesForCenter(Center, sidePow);
        });

        DiamondSquare(sidePow - 1); 
    }

    private List<int[]> FindSubCenters(int[] center, int sidePow)
    {
        throw new NotImplementedException();
    }

    private void SetEdgesForCenter(int[] center, int sidePow)
    {
        int chunkLen = Convert.ToInt32(Mathf.Pow(2, sidePow));

    }

    void SetCenter(int sideLenPow, int[] Center)
    {
        double noise;
        int chunkLen = Convert.ToInt32(Mathf.Pow(2, sideLenPow));
        noise = Math.Floor((rand.NextDouble() - 0.5) * chunkLen * 2); //Smaller grid generate less noise, which means the grid wont experience massive spikes at random.
        altitudeMap[Center[0], Center[1]] = AveragePointsCenter(chunkLen, Center) + noise;
    }

    double AveragePointsCenter(int halfLen, int[] centerIndex)
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
