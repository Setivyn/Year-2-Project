using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    LinkBehaviour linkLogic;

    // Start is called before the first frame update
    void Start()
    {
        linkLogic = FindObjectOfType<LinkBehaviour>();
        gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Run Simulation";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setCamera()
    {
        Camera camera = FindObjectOfType<Camera>();
        int SLhalf = (linkLogic.getSL() - 1) / 2;
        float x, y, z;

        x = SLhalf;
        y = (float)linkLogic.matAtXY(SLhalf, SLhalf) + (Mathf.Tan(Mathf.PI / 6) * (SLhalf * 2.5f));
        z = SLhalf * 3.5f;

        camera.transform.position = new Vector3(x, y, z);
    }

    public void runSim()
    {
        linkLogic.changeSim(4);
    }
}
