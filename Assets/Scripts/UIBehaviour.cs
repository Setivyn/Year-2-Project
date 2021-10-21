using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField] TerrainVisualiseLogic VisLogic;
    [SerializeField] FluidLogic CFDLogic;
    int sideLength;

    // Start is called before the first frame update
    void Start()
    {
        Camera camera = FindObjectOfType<Camera>();
        gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Run Simulation";
        camera.transform.position = new Vector3(VisLogic.getSL() / 2, camera.transform.position.y, VisLogic.getSL() * 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setSL(int sideLen)
    {
        sideLength = sideLen;
    }
}
