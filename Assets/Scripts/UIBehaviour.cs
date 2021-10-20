using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField] TerrainVisualiseLogic VisLogic;
    [SerializeField] FluidLogic CFDLogic;

    // Start is called before the first frame update
    void Start()
    {
        Camera camera = gameObject.GetComponentInChildren<Camera>();
        gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Run Simulation";
        //camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, VisLogic.getSL() / 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
