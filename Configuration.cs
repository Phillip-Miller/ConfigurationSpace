using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Need to go into blender and reset origin to be where it should be based on shape for each point line etc
/// Vector3 = (C/D,A,B) D will only be used instead of C in the case DnotC (when c is 0)
/// Will calculate the scale of the shape
/// Will Switch between the three glued shapes
/// Will modify any colors pertaining to the configuration space
/// Will control tracking object as well
/// </summary>
public class Configuration : MonoBehaviour
{
    public GameObject cNotDD;
    public GameObject DNotDC;
    public GameObject activeConfig;
    public  Vector3 curserParameters= new Vector3(0, 0, 0);
    private Vector3 configurationSpaceOrigin;
    private Vector3 axisAEnd; //A7
    private Vector3 axisBEnd; //A3
    private Vector3 axisCEnd; //X2


   
    GameObject tracker;
    List<GameObject> allPiecesCDD = new List<GameObject>();
    List<GameObject> allPiecesCnotDD = new List<GameObject>();
    List<GameObject> allPiecesDnotDC = new List<GameObject>();

    static float UNITS_PER_DEGREE;

    // Start is called before the first frame update
    void Start()
    {
        activeConfig = this.gameObject;

        findPieces();
        findOrigin();
        calculateScale(this.gameObject);
        createTracker();
        setPosition(new Vector4(360, 360, 180, 180));
        createConfigParents();

    }
    /// <summary>
    /// Will create invisible parents to rotate each config around
    /// </summary>
    private void createConfigParents()
    {

        
        GameObject parent1 = new GameObject("CDD_Parent");
        parent1.transform.position = configurationSpaceOrigin;
        activeConfig.transform.parent = parent1.transform;

        //for CnotDD
        Vector3 vertex2 = allPiecesCnotDD.Where(go => go.name.Equals("X1")).First().GetComponent<Renderer>().bounds.center;
        GameObject parent2 = new GameObject("CnotDD_Parent");
        parent2.transform.position =vertex2;
        cNotDD.transform.parent = parent2.transform;
       

        //for DnotDC
        Vector3 vertex3 = allPiecesDnotDC.Where(go => go.name.Equals("P1")).First().GetComponent<Renderer>().bounds.center;
        GameObject parent3 = new GameObject("DnotDC_Parent");
        parent3.transform.position = vertex3;
        DNotDC.transform.parent = parent3.transform;

        parent2.transform.position = configurationSpaceOrigin;
        parent2.transform.Translate(-20 * Vector3.up);

        parent3.transform.position = configurationSpaceOrigin;
        parent3.transform.Translate(-20 * Vector3.up);

    }
    private void createTracker()
    {
        tracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var colorChange = tracker.GetComponent<Renderer>();
        colorChange.material.SetColor("_Color", Color.yellow);
        tracker.transform.Translate(configurationSpaceOrigin);
        tracker.transform.localScale = .3f * Vector3.one;


        //@FIXME Ill use word coordinates after all for maximum accuracy
        axisAEnd = allPiecesCDD.Where(go => go.name.Equals("A7")).First().GetComponent<Renderer>().bounds.center;
        axisBEnd = allPiecesCDD.Where(go => go.name.Equals("A3")).First().GetComponent<Renderer>().bounds.center;
        axisCEnd = allPiecesCDD.Where(go => go.name.Equals("X2")).First().GetComponent<Renderer>().bounds.center;

        
        
    }

    private void findPieces()
    {
        //this refering to CDD in defualt state
        for (int i = 0; i < this.gameObject.transform.childCount; i++)
        {
            GameObject child = this.gameObject.transform.GetChild(i).gameObject;
            allPiecesCDD.Add(child);
        }
        //CnotDD
        for (int i = 0; i < cNotDD.transform.childCount; i++)
        {
            GameObject child = cNotDD.transform.GetChild(i).gameObject;
            allPiecesCnotDD.Add(child);
        }
        //DnotDC
        for (int i = 0; i < DNotDC.transform.childCount; i++)
        {
            GameObject child = DNotDC.transform.GetChild(i).gameObject;
            allPiecesDnotDC.Add(child);
        }
    }

    /// <summary>
    /// Use point X1 and find the its world space
    /// </summary>
    private void findOrigin()
    {   
        configurationSpaceOrigin = allPiecesCDD.Where(go => go.name.Equals("X1")).First().GetComponent<Renderer>().bounds.center;
    }

    //Scale of all of them should hopefully be the exact same
    //Will be set up for use for CDD X1 -> A3 should be 360 degrees
    private void calculateScale(GameObject go)
    {
        
        Vector3 A3 = allPiecesCDD.Where(go => go.name.Equals("A3")).First().GetComponent<Renderer>().bounds.center;
        UNITS_PER_DEGREE = Vector3.Distance(configurationSpaceOrigin, A3) / 360;
        
        
        
    }
    
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(configurationSpaceOrigin, 5);

    }
    // Update is called once per frame
    void Update()
    {
        

    }

    public void setPosition(Vector4 currentDegValues) //will use set instead of update to avoid creep over time
    {
        //degrees (A,B,C,D) (xyzw)
        print("CURRENT DEG VALUES: " + currentDegValues);
        checkConfig(currentDegValues); 
        Vector4 xyzwDistances = currentDegValues * UNITS_PER_DEGREE;
        float CDVector;
        if (activeConfig.Equals(cNotDD))//Y axis changes based off which configuration space we are in
        {
            CDVector = xyzwDistances.w;
            
           // print("Changing cd vector" + CDVector * (axisCEnd - configurationSpaceOrigin).normalized);

        }
        else
        {
            CDVector = xyzwDistances.z;
        }
       // print("CD VECTOR: " + CDVector);
        Vector3 finalLocation = configurationSpaceOrigin + xyzwDistances.x * (axisAEnd - configurationSpaceOrigin).normalized +
                                                            xyzwDistances.y*(axisBEnd - configurationSpaceOrigin).normalized + 
                                                            CDVector*(axisCEnd - configurationSpaceOrigin).normalized;
        tracker.transform.position = finalLocation;
    }
    public void checkConfig(Vector4 currrentDegValues)
    {

        if(currrentDegValues.z > 180 && activeConfig != DNotDC)
        {
            Debug.Log("Switching to DNOTDC");
            switchConfig(DNotDC);

        }
        else if (currrentDegValues.w > 180 && activeConfig != cNotDD)
        {
            Debug.Log("Switching to CNOTDD");
            switchConfig(cNotDD);

        }
        else if (currrentDegValues.z >= 0 && currrentDegValues.z <= 180 && activeConfig != this.gameObject && currrentDegValues.w != 0)// 0< c <180
        {
            Debug.Log("Switching to CDD");
            switchConfig(this.gameObject);
        }
    }
    public void switchConfig(GameObject nextConfig)
    {
        print("switch Config");
        this.activeConfig.transform.parent.Translate(Vector3.up * -20);
        nextConfig.transform.parent.Translate(Vector3.up * 20);
        this.activeConfig = nextConfig;
    }
}
