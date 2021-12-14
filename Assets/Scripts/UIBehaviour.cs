using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIBehaviour : MonoBehaviour
{
    LinkBehaviour linkLogic;
    Camera UIcamera;
    MeshCollider meshCollider;
    GameObject settButton, infoButton;
    GameObject infoPanel, settPanel, fSettPanel;
    GameObject infoPanelBG, infoPanelFG, infoPanelTxt1, infoPanelTxt2, settPanelBG, settPanelFG, settPanelTxt1, settPanelTxt2, fSettPanelBG, fSettPanelFG, fSettPanelTxt1, fSettPanelTxt2;
    Image infoPanelImageBG, infoPanelImageFG, settPanelImageBG, settPanelImageFG, fSettPanelImageBG, fSettPanelImageFG;
    Text infoPanelTextObject1, infoPanelTextObject2, settPanelTextObject1, settPanelTextObject2, fSettPanelTextObject1, fSettPanelTextObject2;
    Slider roughSlider, steepSlider, lenSlider, diffSlider, viscSlider, dtSlider;
    Toggle complexButton;
    InputField xCoord, yCoord;

    double Roughness;
    double Steepness;
    int inputLen;
    double risk;

    double[] modifiers;
    int sideLengthT;
    int seed;

    bool infoStateBool, settStateBool;

    private void Awake()
    { 
        
        //sets the Seed for the PseudoRandom Generator used in the terrain gen. Creates a random integer value with large limits.
        seed = Guid.NewGuid().GetHashCode();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Initialise values
        linkLogic = FindObjectOfType<LinkBehaviour>();
        UIcamera = FindObjectOfType<Camera>();

        settButton = GameObject.FindGameObjectWithTag("Settings");
        infoButton = GameObject.FindGameObjectWithTag("SimControl");

        infoButton.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Generate Terrain";
        settButton.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Settings";
        gameObject.AddComponent<BoxCollider2D>();

        //
        //Setup InfoPanel

        infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(gameObject.transform, false);

        int[] pos = new int[3];
        byte[] col = new byte[4];
        int[] size = new int[2];

        pos[0] = -360; pos[1] = 50; pos[2] = 0;
        col[0] = col[1] = col[2] = 200; col[3] = 255;
        size[0] = 250; size[1] = 350;

        InitPanelSegment(infoPanel, ref infoPanelBG, out infoPanelImageBG, pos, col, size, "IfBxBG");

        pos[0] = -298; pos[1] = 200; pos[2] = 0;
        col[0] = col[1] = col[2] = 255; col[3] = 255;
        size[0] = 207; size[1] = 50;

        InitPanelSegment(infoPanel, ref infoPanelFG, out infoPanelImageFG, pos, col, size, "IfBxFG");

        pos[0] = -350; pos[1] = 162; pos[2] = 0;

        InitPanelText(infoPanel, ref infoPanelTxt1, out infoPanelTextObject1, pos, "Flooding Info", "IfTxt1");

        pos[0] = -350; pos[1] = 100; pos[2] = 0;

        InitPanelText(infoPanel, ref infoPanelTxt2, out infoPanelTextObject2, pos, "PH text", "IfTxt1");

        pos[0] = -280; pos[1] = 150; pos[2] = 0;

        InitInput(infoPanel, ref xCoord, pos, "xCoord",  "x", "IfIn1");
        xCoord.onEndEdit.AddListener(delegate { updateInfo(xCoord.text, yCoord.text); });

        pos[0] = -360; pos[1] = 150; pos[2] = 0;

        InitInput(infoPanel, ref yCoord, pos, "yCoord", "y", "IfIn1");
        yCoord.onEndEdit.AddListener(delegate { updateInfo(xCoord.text, yCoord.text); });

        infoPanel.SetActive(false);

        //
        //Set up Settings Panel
        settPanel = new GameObject("SettingsPanel");
        settPanel.transform.SetParent(gameObject.transform, false);

        pos[0] = 360; pos[1] = 50; pos[2] = 0;
        col[0] = col[1] = col[2] = 200; col[3] = 255;
        size[0] = 250; size[1] = 350;

        InitPanelSegment(settPanel, ref settPanelBG, out settPanelImageBG, pos, col, size, "StBxBG");

        pos[0] = 298; pos[1] = 200; pos[2] = 0;
        col[0] = col[1] = col[2] = 255; col[3] = 255;
        size[0] = 207; size[1] = 50;

        InitPanelSegment(settPanel, ref settPanelFG, out settPanelImageFG, pos, col, size, "StBxFG");

        pos[0] = 260; pos[1] = 162; pos[2] = 0;

        InitPanelText(settPanel, ref settPanelTxt1, out settPanelTextObject1, pos, "Settings", "StTxt1");

        pos[0] = 320; pos[1] = 120; pos[2] = 0;
        
        InitPanelText(settPanel, ref settPanelTxt2, out settPanelTextObject2, pos, "Roughness; \n Steepness; \n sideLen;", "StTxt2");

        float[] clamp = { 0, 2f };
        pos[0] = 330; pos[1] = 90; pos[2] = 0;

        InitSlider(settPanel, ref roughSlider, "roughSlider", clamp, pos, "Roughness");
        roughSlider.onValueChanged.AddListener(delegate { updateSetting(1, roughSlider); });

        clamp[0] = 0; clamp[1] = 1f;
        pos[0] = 345; pos[1] = 60; pos[2] = 0;

        InitSlider(settPanel, ref steepSlider, "steepSlider", clamp, pos, "Steepness");
        steepSlider.onValueChanged.AddListener(delegate { updateSetting(2, steepSlider); });

        clamp[0] = 1f; clamp[1] = 6f;
        pos[0] = 355; pos[1] = 30; pos[2] = 0;

        InitSlider(settPanel, ref lenSlider, "lenSlider", clamp, pos, "Length");
        lenSlider.onValueChanged.AddListener(delegate { updateSetting(3, lenSlider); });


        settPanel.SetActive(false);

        //
        // Set Up Fluid Settings Panel
        fSettPanel = new GameObject("FluidSettingsPanel");
        fSettPanel.transform.SetParent(gameObject.transform, false);

        pos[0] = 360; pos[1] = 50; pos[2] = 0;
        col[0] = col[1] = col[2] = 200; col[3] = 255;
        size[0] = 250; size[1] = 350;

        InitPanelSegment(fSettPanel, ref fSettPanelBG, out fSettPanelImageBG, pos, col, size, "FStBxBG");

        pos[0] = 298; pos[1] = 200; pos[2] = 0;
        col[0] = col[1] = col[2] = 255; col[3] = 255;
        size[0] = 207; size[1] = 50;

        InitPanelSegment(fSettPanel, ref fSettPanelFG, out fSettPanelImageFG, pos, col, size, "FStBxFG");

        pos[0] = 260; pos[1] = 162; pos[2] = 0;

        InitPanelText(fSettPanel, ref fSettPanelTxt1, out fSettPanelTextObject1, pos, "Fluid Settings", "FStTxt1");

        pos[0] = 320; pos[1] = 120; pos[2] = 0;

        InitPanelText(fSettPanel, ref fSettPanelTxt2, out fSettPanelTextObject2, pos, "Complexity; \n Diffusion; \n Viscosity; \n Time Step;", "FStTxt2");

        pos[0] = 325; pos[1] = 80; pos[2] = 0;

        InitToggle(fSettPanel, ref complexButton, pos, "complexity", "Complexity");
        complexButton.onValueChanged.AddListener(delegate { changeComplexity(); updateFluidSetting(0, diffSlider); });

        clamp[0] = 0; clamp[1] = 1f;
        pos[0] = 345; pos[1] = 60; pos[2] = 0;

        InitSlider(fSettPanel, ref diffSlider, "diffSlider", clamp, pos, "Diffusion");
        diffSlider.onValueChanged.AddListener(delegate { updateFluidSetting(1, diffSlider); });

        clamp[0] = 0; clamp[1] = 1f;
        pos[0] = 355; pos[1] = 30; pos[2] = 0;

        InitSlider(fSettPanel, ref viscSlider, "viscSlider", clamp, pos, "Viscosity");
        viscSlider.onValueChanged.AddListener(delegate { updateFluidSetting(2, viscSlider); });

        clamp[0] = 1f; clamp[1] = 100f;
        pos[0] = 370; pos[1] = 10; pos[2] = 0;

        InitSlider(fSettPanel, ref dtSlider, "dtSlider", clamp, pos, "Time Step");
        dtSlider.onValueChanged.AddListener(delegate { updateFluidSetting(3, dtSlider); });


        fSettPanel.SetActive(false);

        //
    }

    private void InitToggle(GameObject panel, ref Toggle toggle, int[] position, string tag, string name)
    {
        toggle = GameObject.FindGameObjectWithTag(tag).GetComponent<Toggle>();
        toggle.transform.SetParent(panel.transform, false);

        toggle.name = name;
        Vector3 newPos = new Vector3(position[0] + gameObject.transform.position.x / gameObject.transform.localScale.x,
                                    position[1] + gameObject.transform.position.y / gameObject.transform.localScale.y,
                                    position[2] + gameObject.transform.position.z / gameObject.transform.localScale.z);
        toggle.transform.position = Vector3.Scale(newPos, gameObject.transform.localScale);
    }

    private void InitInput(GameObject panel, ref InputField field, int[] position, string tag, string text, string name)
    {
        field = GameObject.FindGameObjectWithTag(tag).GetComponent<InputField>();
        field.transform.SetParent(panel.transform, false);

        field.name = name;
        Vector3 newPos = new Vector3(position[0] + gameObject.transform.position.x / gameObject.transform.localScale.x,
                                    position[1] + gameObject.transform.position.y / gameObject.transform.localScale.y,
                                    position[2] + gameObject.transform.position.z / gameObject.transform.localScale.z);
        field.transform.position = Vector3.Scale(newPos, gameObject.transform.localScale);

        field.text = text;
    }

    private void InitSlider(GameObject panel, ref Slider sliderObject, string tag, float[] clamp, int[] position, string name)
    {
        sliderObject = GameObject.FindGameObjectWithTag(tag).GetComponent<Slider>();

        sliderObject.transform.SetParent(panel.transform, false);
        sliderObject.name = name;


        Vector3 newPos = new Vector3(position[0] + gameObject.transform.position.x / gameObject.transform.localScale.x,
                                    position[1] + gameObject.transform.position.y / gameObject.transform.localScale.y,
                                    position[2] + gameObject.transform.position.z / gameObject.transform.localScale.z);
        sliderObject.transform.position = Vector3.Scale(newPos, gameObject.transform.localScale);

        sliderObject.minValue = clamp[0]; sliderObject.maxValue = clamp[1];

    }

    private void InitPanelText(GameObject panel, ref GameObject segment, out Text segmentTextField, int[] position, string text, string name)
    {
        segment = new GameObject(name);
        segment.AddComponent<CanvasRenderer>();
        segmentTextField = segment.AddComponent<Text>();

        segment.transform.SetParent(panel.transform, false);

        segmentTextField.text = text;
        segmentTextField.color = new Color(0, 0, 0, 255);
        segmentTextField.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        Vector3 newPos = new Vector3(position[0] + gameObject.transform.position.x / gameObject.transform.localScale.x,
                                    position[1] + gameObject.transform.position.y / gameObject.transform.localScale.y,
                                    position[2] + gameObject.transform.position.z / gameObject.transform.localScale.z);
        segment.transform.position = Vector3.Scale(newPos, gameObject.transform.localScale);

        RectTransform segTransf = segment.GetComponent<RectTransform>();
        segTransf.sizeDelta = new Vector2(200,100);
    }

    private void InitPanelSegment(GameObject panel, ref GameObject segment, out Image segmentImage, int[] position, byte[] colour, int[] size, string name)
    {
        segment = new GameObject(name);
        segment.AddComponent<CanvasRenderer>();
        segmentImage = segment.AddComponent<Image>();

        segment.transform.SetParent(panel.transform, false);

        segmentImage.color = new Color32(colour[0], colour[1], colour[2], colour[3]);


        Vector3 newPos = new Vector3(position[0] + gameObject.transform.position.x / gameObject.transform.localScale.x,
                                    position[1] + gameObject.transform.position.y / gameObject.transform.localScale.y,
                                    position[2] + gameObject.transform.position.z / gameObject.transform.localScale.z);
        segment.transform.position = Vector3.Scale(newPos, gameObject.transform.localScale);
        RectTransform panelTransf = segment.GetComponent<RectTransform>();
        panelTransf.sizeDelta = new Vector2(size[0], size[1]);
    }

    // Update is called once per frame
    

    public void OnMouseDown()
    {
        RaycastHit hit;
        Ray ray = UIcamera.ScreenPointToRay(Input.mousePosition);

        if (FindObjectOfType<TerrainVisualiseLogic>().GetComponentInParent<MeshCollider>().Raycast(ray, out hit, Mathf.Infinity))
        {
            infoPanelTextObject2.text = (" \n Local Density: " + (linkLogic.getDensAtPoint(hit.point) * 100));
            xCoord.text = ((int)hit.point.x).ToString();
            yCoord.text = ((int)hit.point.y).ToString();
            infoPanel.SetActive(true);
        }
    }

    void updateInfo(string xInput, string zInput)
    {
        int CubeLen = linkLogic.getFluidCubeSize();
        int x, z;
        try
        {
            x = Convert.ToInt32(xInput);
        }
        catch
        {
            x = 0;
        }

        try
        {
            z = Convert.ToInt32(zInput);
        }
        catch
        {
            z = 0;
        }

        x = Mathf.Clamp(x, 0, linkLogic.getSL());

        infoPanelTextObject2.text = (" \n Local Density: " + (linkLogic.getDensAtPoint(x / CubeLen, (int)linkLogic.matAtXY(x,z) / CubeLen, z / CubeLen) * 100) + "\n Grid Risk: " + risk);

        xCoord.text = (x).ToString();
        yCoord.text = (z).ToString();
    }

    void updateSetting(int setting, Slider slider)
    {
        if (setting == 1) { Roughness = slider.value; }
        
        else if (setting == 2) { Steepness = slider.value; }
        //forces sidelength to be an odd power of 2 from ^3 up to ^15. these values always have a factor of 3 when 1 is added, allowing fluid dynamics to be less accurate but faster.
        else { sideLengthT = (2 * (int)slider.value) + 1; }
        settPanelTextObject2.text = "Roughness;" + Math.Round(Roughness, 2) + "\n Steepness;" + Math.Round(Steepness, 2) + "\n sideLen; " + (Math.Pow(2, sideLengthT) - 1);
    }

    void updateFluidSetting(int setting, Slider slider)
    {
        int complex = linkLogic.getComplex() == true ? 1 : 3;
        if (setting == 1) {linkLogic.setDiffConst(slider.value); }

        else if (setting == 2) { linkLogic.setViscConst(slider.value); }
        else if (setting == 3) { linkLogic.setDt(slider.value); }
        fSettPanelTextObject2.text = "Complexity;" + complex +  "\n Diffusion;" + Math.Round(linkLogic.getDiff(),2) + " \n Viscosity;" + Math.Round(linkLogic.getVisc(),2) + " \n Time Step; " + Math.Round(linkLogic.getDt(),2);
    }

    public void createTerrain()
    {

        //Set up Modifiers for Terrain
        modifiers = SetModifiers(Roughness, Steepness);

        //Generate Terrain Matrix and assign to mesh filter
        linkLogic.setMat(sideLengthT, seed, modifiers);
        linkLogic.initMesh();
        linkLogic.startGen();

    }

    private double[] SetModifiers(double roughness, double steepness)
    {
        //Convert input modifiers to array format, for the terrain generator
        double[] output = { roughness, steepness };
        return output;
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
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(SLhalf * 10, SLhalf * 10);
    }

    public void runSim()
    {
        if(infoStateBool == false)
        {

            infoStateBool = true;
            createTerrain();
            infoButton.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "Run Simulation";
            settPanel.SetActive(false);
        }
        else
        {
            linkLogic.changeSim(4);
        }
        
    }
    public void setRisk()
    {
        risk = (sideLengthT * Steepness) / (Roughness * inputLen);
    }

    public void openCloseSettings()
    {
        if (infoStateBool == false)
        {
            settPanel.SetActive(!settPanel.activeSelf);
        }
        else
        {
            fSettPanel.SetActive(!fSettPanel.activeSelf);
        }
    }

    public int getSeed()
    {
        return seed;
    }

    public double[] getModifiers()
    {
        return modifiers;
    }

    public double getRisk()
    {
        return risk;
    }
    public void changeComplexity()
    {
        bool Complex = complexButton.isOn;
        changeToggleColour(Complex);
        linkLogic.setDivSize(Complex);

    }

    private void changeToggleColour(bool complex)
    {
        ColorBlock newCol = new ColorBlock();
        newCol.normalColor = complex ? new Color(0,1,0) : new Color(1, 1, 1);
        newCol.pressedColor = complex ? new Color(0, 1, 0) : new Color(1, 1, 1);
        complexButton.colors = newCol;
    }
}
