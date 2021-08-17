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


    GameObject parentModel;
    GameObject tracker;
    List<GameObject> allPieces = new List<GameObject>();
    static float UNITS_PER_DEGREE;

    // Start is called before the first frame update
    void Start()
    {
        parentModel = this.gameObject;
        activeConfig = this.gameObject;
        findPieces();
        findOrigin();
        calculateScale(this.gameObject);
        createTracker();
        setPosition(new Vector4(360, 360, 180, 180));


    }

    private void createTracker()
    {
        tracker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var colorChange = tracker.GetComponent<Renderer>();
        colorChange.material.SetColor("_Color", Color.yellow);
        tracker.transform.Translate(configurationSpaceOrigin);
        tracker.transform.localScale = .3f * Vector3.one;


        //@FIXME Ill use word coordinates after all for maximum accuracy
        axisAEnd = allPieces.Where(go => go.name.Equals("A7")).First().GetComponent<Renderer>().bounds.center;
        axisBEnd = allPieces.Where(go => go.name.Equals("A3")).First().GetComponent<Renderer>().bounds.center;
        axisCEnd = allPieces.Where(go => go.name.Equals("X2")).First().GetComponent<Renderer>().bounds.center;

        print(Vector3.Distance(axisAEnd, configurationSpaceOrigin ));
        print(Vector3.Distance(axisBEnd, configurationSpaceOrigin ));
        print(Vector3.Distance(axisCEnd, configurationSpaceOrigin));
        Debug.DrawRay(configurationSpaceOrigin, axisAEnd - configurationSpaceOrigin, Color.red, 100f);
        Debug.DrawRay(configurationSpaceOrigin, axisBEnd - configurationSpaceOrigin, Color.blue, 100f);
        Debug.DrawRay(configurationSpaceOrigin, axisCEnd - configurationSpaceOrigin, Color.yellow, 100f);
    }

    private void findPieces()
    {
        
        for (int i = 0; i < parentModel.transform.childCount; i++)
        {
            GameObject child = parentModel.transform.GetChild(i).gameObject;
            allPieces.Add(child);
        }
    }

    /// <summary>
    /// Use point X1 and find the its world space
    /// </summary>
    private void findOrigin()
    {   
        configurationSpaceOrigin = allPieces.Where(go => go.name.Equals("X1")).First().GetComponent<Renderer>().bounds.center;
    }

    //Scale of all of them should hopefully be the exact same
    //Will be set up for use for CDD X1 -> A3 should be 360 degrees
    private void calculateScale(GameObject go)
    {
        
        Vector3 A3 = allPieces.Where(go => go.name.Equals("A3")).First().GetComponent<Renderer>().bounds.center;
        UNITS_PER_DEGREE = Vector3.Distance(configurationSpaceOrigin, A3) / 360;
        
        
        
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(configurationSpaceOrigin, 5);

    }
    // Update is called once per frame
    void Update()
    {
        

    }

    public void setPosition(Vector4 currentDegValues) //will use set instead of update to avoid creep over time
    {
        Debug.Log("Setting position");
        //degrees (A,B,C,D) (xyzw)
        checkConfig(currentDegValues);
        Vector4 xyzwDistances = currentDegValues * UNITS_PER_DEGREE;
        float CDVector = (activeConfig == DNotDC ? xyzwDistances.w : xyzwDistances.z); //Y axis changes based off which configuration space we are in
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
        if (currrentDegValues.z < 0 && activeConfig != cNotDD)
        {
            Debug.Log("Switching to CNOTDD");

            switchConfig(cNotDD);

        }
        else if (activeConfig != this.gameObject)// 0< c <180
        {
            Debug.Log("Switching to CDD");

            switchConfig(this.gameObject);
        }
    }
    public void switchConfig(GameObject nextConfig)
    {
        //throwing errors
        this.activeConfig.GetComponent<MeshRenderer>().enabled = false;
        nextConfig.GetComponent<MeshRenderer>().enabled = true; 
        this.activeConfig = nextConfig;
    }
}
