using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBehaviour : MonoBehaviour
{
    FluidLogic fluidLogic;
    public TerMat terrainLogic;
    UIBehaviour UILogic;
    TerrainVisualiseLogic visLogic;

    // Start is called before the first frame update
    void Awake()
    {
        fluidLogic = FindObjectOfType<FluidLogic>();
        UILogic = FindObjectOfType<UIBehaviour>();
        visLogic = FindObjectOfType<TerrainVisualiseLogic>();
    }

    //visLogic links

    public void changeSim(int iterations)
    {
        visLogic.changeSimState(iterations);
    }

    public void setColours(double[,,] values, int count)
    {
        visLogic.SetColours(values, count);
    }

    public int GetSeed()
    {
        return visLogic.getSeed();
    }


    //
    //fluidLogic links
    public void startStopSim(int iterations)
    {
        fluidLogic.startStop(iterations);
    }

    public void initSim(int meshSize)
    {
        fluidLogic.initSimulation(meshSize);
    }

    public void addDensToFluid(int y, double amount)
    {
        fluidLogic.addDToCube(y, amount);
    }

    public void addVelToFluid(int y, double amountX, double amountY, double amountZ)
    {
        fluidLogic.addVToCube(y, amountX, amountY, amountZ);
    }

    public int getFluidCubeCount()
    {
        return fluidLogic.getCubeCount();
    }

    public double getDensAtPoint(int x, int y, int z)
    {
        return fluidLogic.getDensityAtCube(x, y, z);
    }
    public double getDensAtPoint(Vector3 point)
    {
        int size = getFluidCubeSize();
        int x = (int)(point.x / size);
        int y = (int)(point.y / size);
        int z = (int)(point.z / size);
        return fluidLogic.getDensityAtCube(x, y, z);
    }

    public int getFluidCubeSize()
    {
        return fluidLogic.getCubeSize();
    }

    public void stopVAdd()
    {
        fluidLogic.ceaseVelAdd();
    }

    //
    //TerrainMat links
    public double matAtXY(int x, int y)
    {
        return terrainLogic.GetMatrixAtPoint(x, y);
    }

    public int getSL()
    {
        return terrainLogic.getSL();
    }

    public void setMat(int sidePow, int seed, double[] mods)
    {
        terrainLogic = new TerMat(sidePow, seed, mods);
    }
    //
    //UI links

    //
    public void setCamera()
    {
        UILogic.setCamera();
    }
}