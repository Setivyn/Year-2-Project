using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIBehaviour : MonoBehaviour
{
    LinkBehaviour linkLogic;
    Camera UIcamera;
    MeshCollider meshCollider;
    GameObject infoPanel;
    GameObject infoPanelBG, infoPanelFG, infoPanelTxt1, infoPanelTxt2;
    Image panelImageBG, panelImageFG;
    Text panelText1, panelText2;

    // Start is called before the first frame update
    void Start()
    {
        linkLogic = FindObjectOfType<LinkBehaviour>();
        UIcamera = FindObjectOfType<Camera>();

        gameObject.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Run Simulation";
        gameObject.AddComponent<BoxCollider2D>();

        infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(gameObject.transform, false);

        int[] pos = new int[3];
        byte[] col = new byte[4];
        int[] size = new int[2];

        pos[0] = -360; pos[1] = 50; pos[2] = 0;
        col[0] = col[1] = col[2] = 200; col[3] = 255;
        size[0] = 250; size[1] = 350;

        InitPanelSegment(ref infoPanelBG, out panelImageBG, pos, col, size, "IfBxBG");

        pos[0] = -298; pos[1] = 200; pos[2] = 0;
        col[0] = col[1] = col[2] = 255; col[3] = 255;
        size[0] = 207; size[1] = 50;

        InitPanelSegment(ref infoPanelFG, out panelImageFG, pos, col, size, "IfBxFG");

        pos[0] = -320; pos[1] = 162; pos[2] = 0;

        InitPanelText(ref infoPanelTxt1, out panelText1, pos, "Flooding Info", "IfTxt1");

        pos[0] = -300; pos[1] = 100; pos[2] = 0;

        InitPanelText(ref infoPanelTxt2, out panelText2, pos, "PH text", "IfTxt1");

        infoPanel.SetActive(false);
    }

    private void InitPanelText(ref GameObject segment, out Text segmentTextField, int[] position, string text, string name)
    {
        segment = new GameObject(name);
        segment.AddComponent<CanvasRenderer>();
        segmentTextField = segment.AddComponent<Text>();

        segment.transform.SetParent(infoPanel.transform, false);

        segmentTextField.text = text;
        segmentTextField.color = new Color(0, 0, 0, 255);
        segmentTextField.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        Vector3 newPos = new Vector3(position[0] + gameObject.transform.position.x / gameObject.transform.localScale.x,
                                    position[1] + gameObject.transform.position.y / gameObject.transform.localScale.y,
                                    position[2] + gameObject.transform.position.z / gameObject.transform.localScale.z);
        segment.transform.position = Vector3.Scale(newPos, gameObject.transform.localScale);
    }

    private void InitPanelSegment(ref GameObject segment, out Image segmentImage, int[] position, byte[] colour, int[] size, string name)
    {
        segment = new GameObject(name);
        segment.AddComponent<CanvasRenderer>();
        segmentImage = segment.AddComponent<Image>();

        segment.transform.SetParent(infoPanel.transform, false);



        segmentImage.color = new Color32(colour[0], colour[1], colour[2], colour[3]);


        Vector3 newPos = new Vector3(position[0] + gameObject.transform.position.x / gameObject.transform.localScale.x,
                                    position[1] + gameObject.transform.position.y / gameObject.transform.localScale.y,
                                    position[2] + gameObject.transform.position.z / gameObject.transform.localScale.z);
        segment.transform.position = Vector3.Scale(newPos, gameObject.transform.localScale);
        RectTransform panelTransf = segment.GetComponent<RectTransform>();
        panelTransf.sizeDelta = new Vector2(size[0], size[1]);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMouseDown()
    {
        RaycastHit hit;
        Ray ray = UIcamera.ScreenPointToRay(Input.mousePosition);

        if (FindObjectOfType<TerrainVisualiseLogic>().GetComponentInParent<MeshCollider>().Raycast(ray, out hit, Mathf.Infinity))
        {
            panelText2.text = ("( " + (int)hit.point.x + ", " + (int)hit.point.y + " ) \n Local Density: " + (linkLogic.getDensAtPoint(hit.point) * 100));

            infoPanel.SetActive(true);
        }
    }

    public void setCamera()
    {
        
        int SLhalf = linkLogic.getSL() / 2;
        float x, y, z;

        x = SLhalf;
        y = (float)linkLogic.matAtXY(SLhalf, SLhalf) + (Mathf.Tan(Mathf.PI / 6) * (SLhalf * 2.5f));
        z = SLhalf * 3.5f;

        UIcamera.transform.position = new Vector3(x, y, z);
        gameObject.transform.position = UIcamera.transform.position;
        gameObject.transform.rotation = UIcamera.transform.rotation;
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(SLhalf * 10,SLhalf * 10);
    }

    public void runSim()
    {
        linkLogic.changeSim(4);
    }

}
