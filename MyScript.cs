using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/* No Hinge Pure Math Model
 * Set square with 90 degree angles and faces with non 90
 * @Author Phillip MIller
 * @Date 6/21/2021
 */

class Edge //vertex should be a world vertex
{
    public Vector3 vertex1;
    public Vector3 vertex2;
    public GameObject go;
    public double length;
    
    public Edge(Vector3 vertex1, Vector3 vertex2, GameObject go)
    {
        this.go = go;
        this.vertex1 = vertex1;
        this.vertex2 = vertex2;
        this.length = (vertex1 - vertex2).magnitude;
    }

    public override bool Equals(object obj)
    {
        Edge other = (Edge)obj;
        if (other.vertex1.Equals(vertex1) && other.vertex2.Equals(vertex2))
            return true;
        if (other.vertex1.Equals(vertex2) && other.vertex2.Equals(vertex1))
            return true;
        return false;
    }

    public override int GetHashCode() //no idea if this works
    {
        int hashCode = -1704521559;
        hashCode = hashCode + -1521134295 * vertex1.GetHashCode();
        hashCode = hashCode + -1521134295 * vertex2.GetHashCode();
        return hashCode;
    }

    //if they have one vertex in common
    public bool isConnected(Edge other)
    {
        return (vertex1.Equals(other.vertex1) || vertex1.Equals(other.vertex2) || vertex2.Equals(other.vertex1) || vertex2.Equals(other.vertex2));
    }

    public override string ToString()
    {
        return base.ToString() + this.vertex1.ToString() + this.vertex2.ToString();
    }
}
class Triangle : IEnumerable<Edge>
{
    public Edge edge1;
    public Edge edge2;
    public Edge edge3;
    public double Area;
    public Vector3 Normal;

    public Triangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, GameObject go)
    {
        this.edge1 = new Edge(vertex1, vertex2, go);
        this.edge2 = new Edge(vertex2, vertex3, go);
        this.edge3 = new Edge(vertex3, vertex1, go);
        this.Area = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1).magnitude / 2;
        this.Normal = GetNormal();
    }
    public Triangle(Edge edge1, Edge edge2, Edge edge3)
    {
        this.edge1 = edge1;
        this.edge2 = edge2;
        this.edge3 = edge3;
        this.Area = Vector3.Cross(edge2.vertex1 - edge1.vertex1, edge3.vertex1 - edge1.vertex1).magnitude / 2;
        this.Normal = GetNormal();
    }
    private Vector3 GetNormal()
    {
        Vector3 side1 = edge2.vertex1 - edge1.vertex1;
        Vector3 side2 = edge3.vertex1 - edge1.vertex1;
        return Vector3.Cross(side1, side2).normalized;
    }

    public IEnumerator<Edge> GetEnumerator()
    {
        yield return edge1;
        yield return edge2;
        yield return edge3;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public void Draw()
    {
        Debug.DrawLine(this.edge1.vertex1, this.edge1.vertex2, Color.red, 10f);
        Debug.DrawLine(this.edge2.vertex1, this.edge2.vertex2, Color.red, 10f);
        Debug.DrawLine(this.edge3.vertex1, this.edge3.vertex2, Color.red, 10f);
    }
    public override string ToString()
    {
        return base.ToString() + ": " + edge1.ToString() + edge2.ToString() + edge3.ToString();
    }
}
class Polygon : IEnumerable
{
    private List<Vector3> Vertices = new List<Vector3>(); 
    public List<Edge> EdgeList = new List<Edge>();
    private List<Triangle> TriangleList = new List<Triangle>();
    

    public Polygon(List<Triangle> triangles)
    {
        this.TriangleList = triangles;
    }
    public Polygon()
    {

    }
    /// <summary>
    ///Create triangles from vertex[1]
    /// </summary>
    /// <returns></returns>
    public List<Triangle> createTriangles() 
    {
       
        getVerticies();
        List<Triangle> t = new List<Triangle>();
        foreach(Edge e in this.EdgeList)
        {
            if(e.vertex1 != this.Vertices[0] && e.vertex2 != this.Vertices[0])
            { 
                t.Add(new Triangle(this.Vertices[0], e.vertex1, e.vertex2, e.go));
            }
        }
        
        return t; 
    }
    public List<Vector3> getVerticies() 
    {
        if (this.Vertices.Count > 3)
            return this.Vertices;
        else
        {
            foreach(Edge edge in EdgeList)
            {
                this.Vertices.Add(edge.vertex1);
            } 
        }
        return this.Vertices;
    }
    public Vector3 getNormal()
    {
        return (new Triangle(EdgeList[0], EdgeList[1], EdgeList[2])).Normal;
    }
    public double getArea()
    {
        if (this.TriangleList == null)
            createTriangles();
        double area = 0;
        foreach (Triangle tri in TriangleList)
        {
            area += tri.Area;
        }
        return area;
    }
    public Vector3 getMiddle()
    {
        var verts = this.getVerticies();
        Vector3 avg = Vector3.zero;

        foreach(Vector3 vert in verts)
        {
            avg += vert;
        }
        avg /= verts.Count;
        return avg;
    }
    public IEnumerator GetEnumerator()
    {
        return EdgeList.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Equals(object obj) 
    {
        Polygon p = (Polygon) obj;
        return this.getVerticies().All(p.getVerticies().Contains) && this.getVerticies().Count == p.getVerticies().Count; 
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
class MyHinge //everything in here is in world space !! 
    //hinge normal 1 and 2 will be continously updated as go1, and go2 are rotated respectively
{
    public readonly Vector3 origin;
    public readonly Polygon polygon1;
    public readonly Polygon polygon2;
    public readonly Vector3 moveDirection;
    

    //Updated during runtime
    public Vector3 axis;
    public Vector3 anchor;
    public Vector3 axisPointA;
    public Vector3 axisPointB;
    public Vector3 orthogonalV1;
    public Vector3 orthogonalV2;
    public List<MyHinge> connectedHinges = new List<MyHinge>();

    public static List<Polygon> lockedFaces = new List<Polygon>();
    public GameObject go1;
    public GameObject go2;

    public float updatedAngle;
    
    public MyHinge(Polygon Polygon1, Polygon Polygon2,Vector3 axisPointA, Vector3 axisPointB)
    {
        polygon1 = Polygon1;
        polygon2 = Polygon2;
        this.go1 = Polygon1.EdgeList[0].go;
        this.go2 = Polygon2.EdgeList[0].go;
        this.axis = axisPointA - axisPointB; //should I actually be subtracting here
        this.anchor = (axisPointA + axisPointB) / 2;
        this.origin = this.anchor;
        this.axisPointA = axisPointA;
        this.axisPointB = axisPointB;
        calculateOrthogonalHinge();
        this.updatedAngle = this.GetAngle();
        this.moveDirection = ((orthogonalV1 + orthogonalV2) / 2).normalized;
    }
    public void updateGo()
    {
        this.go1 = polygon1.EdgeList[0].go;
        this.go2 = polygon2.EdgeList[0].go;

    }
        
    private GameObject GetTranslateParent(bool firstGo, Vector3 altAnchor) //for use in rotating around a different gameObject's hinge...apears the defualt values must be compile time constant so I will overload this function
    {
        
        //@TODO: need to move the other one back in addition to just adjusting the angle
        GameObject child;
        if (firstGo)
        {
            if (this.go1.transform.parent != null && this.go1.transform.parent.name.Equals("TrackingParent"))
            {

                return this.go1.transform.parent.gameObject;
            }
            child = this.go1;

        }
        else
        {
            if (this.go2.transform.parent != null && this.go2.transform.parent.name.Equals("TrackingParent"))
                return this.go2.transform.parent.gameObject;
            child = this.go2;
        }
        GameObject parent = new GameObject("TrackingParent");
        parent.transform.Translate(altAnchor);
        child.transform.parent = parent.transform;
        return parent; //TODO: this could cause issues with a gameObject having two parents
    }
    private GameObject GetTranslateParent(bool firstGo)
    {

        GameObject child;
        GameObject oldParent;
        if (this.go1.transform.parent != null && this.go1.transform.parent.name != "TrackingParent") 
        {
            oldParent = this.go1.transform.parent.gameObject;
            this.go1.transform.parent = null;
            GameObject.Destroy(oldParent);
        }
        if (this.go2.transform.parent != null && this.go2.transform.parent.name != "TrackingParent")
        {
            oldParent = this.go2.transform.parent.gameObject;
            this.go2.transform.parent = null;
            GameObject.Destroy(oldParent);
        }
        if (firstGo)
        {
            if (this.go1.transform.parent != null) {
                
                return this.go1.transform.parent.gameObject;
            }
            child = this.go1;

        }
        else 
        { 
            if (this.go2.transform.parent != null)
                return this.go2.transform.parent.gameObject;
            child = this.go2;
        }
        GameObject parent = new GameObject("PARENT");
        parent.transform.Translate(this.anchor);
        child.transform.parent = parent.transform;
        return parent;
        
    }
    private void calculateOrthogonalHinge() //want to find orthogonal line between axis and point on polygon. Might want to calculate the axis inside of this function
    {
        this.orthogonalV1 = this.polygon1.getMiddle() - this.anchor;
        this.orthogonalV2 = this.polygon2.getMiddle() - this.anchor;
        
       
    }

    private float GetAngle()
    {
        
        double magAxB = orthogonalV1.magnitude * orthogonalV2.magnitude;
        double dotProduct = Vector3.Dot(orthogonalV1, orthogonalV2);
        //Vector3.SignedAngle(orthogonalV1, orthogonalV2); has weird result where it will always return value less than 180
        return (float)(Math.Acos((dotProduct / magAxB)) * (180 / Math.PI));
    }
    public override bool Equals(object obj)
    {
        MyHinge other = (MyHinge)obj; //sees if the anchor is the same
        if (this.anchor.Equals(other.anchor))
            return true;
        return false;
    }
    public bool sharesPolygon(MyHinge hinge)
    {
       
        return this.polygon1.Equals(hinge.polygon1) || this.polygon1.Equals(hinge.polygon2) || this.polygon2.Equals(hinge.polygon1) || this.polygon2.Equals(hinge.polygon2);
    }
    /// <summary>
    /// Moves GO's, updates 
    /// </summary>
    /// <param name="rad"></param>
    public void updateAngle(float deg)  //need to adjust the flaps here too...locked faces not really used
    {
        this.updatedAngle += deg;

        if (lockedFaces.Contains(this.polygon1))
        {
            Debug.Log("Locked Face1");
            RotateAroundPivot(false, deg);
        }
        if (lockedFaces.Contains(this.polygon2))
        {
            Debug.Log("Locked Face2");
            RotateAroundPivot(true, deg);
        }
        else
        {
            RotateAroundPivot(true, deg/2);
            RotateAroundPivot(false,-1*deg/2);
        }
    }
    
    private void RotateAroundPivot(bool firstGo, float deg) 
    {
        //update hinge.connectedHinges here! @ TODO: something @FIXME 
        

            //you have to track the flaps here
        GameObject parent = this.GetTranslateParent(firstGo);

        parent.transform.Rotate(this.axis, deg);
        Quaternion rotation = Quaternion.AngleAxis(deg, this.axis);

        if ((this.go1.name.Contains("abcd") || this.go2.name.Contains("abcd")) && firstGo) //added first go to make sure this only gets executed one time
        {
            foreach (MyHinge hinge in this.connectedHinges) //connectedHinges is having some wonky behavior
            {
                //@TODO: update the orthogonal values when rotating this to match
                bool connectedFirstGo = !this.go1.name.Contains("abcd"); //we want to move the non abcd face (flapA and flapB)
                hinge.GetTranslateParent(connectedFirstGo, this.anchor).transform.Rotate(this.axis, -deg); //@FIXME not sure why this has to be negative
                if (connectedFirstGo) //update ortho
                {
                    hinge.orthogonalV1 = rotation.normalized * hinge.orthogonalV1; //order matters 
                }
                else
                {
                    hinge.orthogonalV2 = rotation.normalized * hinge.orthogonalV1; //order matters 
                }
            }
        }

        if (firstGo) //update ortho
        {
            orthogonalV1 = rotation.normalized * orthogonalV1; //order matters 
        }
        else
        {
            orthogonalV2 = rotation.normalized * orthogonalV1; //order matters 
        }

        

    }
    public void TranslateHinge(float dX) //couple things wrong here you are translating whole hinge not just one side, the tracking needs to be done somewhere else, might need overloaded translate parent, this.move direction is completely broken
    {
        
        dX *= -1;
        orthogonalV1 += ((orthogonalV1 + orthogonalV2) / 2).normalized * dX; 
        orthogonalV2 += ((orthogonalV2 + orthogonalV2) / 2).normalized * dX;
        this.axisPointA += this.moveDirection * dX;
        this.axisPointB += this.moveDirection * dX;
        this.axis = axisPointA - axisPointB; //should I actually be subtracting here
        this.anchor = (axisPointA + axisPointB) / 2;
        this.GetTranslateParent(true).transform.Translate(this.moveDirection * dX, Space.World);
        this.GetTranslateParent(false).transform.Translate(this.moveDirection * dX, Space.World);
        if (this.go1.name.Contains("a") || this.go2.name.Contains("a"))
        { //then we will have to adjust other hinges

            foreach (MyHinge hinge in this.connectedHinges)
            {
                orthogonalV1 += this.moveDirection * dX;
                orthogonalV2 += this.moveDirection * dX;
                hinge.axisPointA += this.moveDirection * dX;
                hinge.axisPointB += this.moveDirection * dX;
                hinge.axis = axisPointA - axisPointB; //should I actually be subtracting here
                hinge.anchor = (axisPointA + axisPointB) / 2;
                bool first = !hinge.go1.name.Equals("abcd");
                hinge.GetTranslateParent(first,this.anchor).transform.Translate(this.moveDirection * dX, Space.World);
            }
            
        }
    }

    /// <summary>
    /// Draws in green
    /// </summary>
    public void Draw() //need to update these points more often
    {
        Debug.DrawLine(axisPointA, axisPointB, Color.green, 5f);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
/// <summary>
/// 
/// </summary>
public class MyScript : MonoBehaviour
{
    List<MyHinge> uniqueHinges;
    private GameObject parentModel;
    public bool autoUnpack;
    public bool useGravity;
    private double hingeTolerance = .001;
    public double sideArea;

    private double squareLength;
    private List<GameObject> ColliderGoList;
    GameObject[] allGameObj;
    List<MyHinge> interiorHinges = new List<MyHinge>();
    MyHinge flapA;
    MyHinge flapB;
    MyHinge hingeC;
    MyHinge hingeD;

    public float degTracker;
    [System.NonSerialized]
    public bool gameObjectDestroyed;



    void Start()
    {
        //myPlygon changes go but hinges are made before that....

        parentModel = this.gameObject;
        allGameObj = FindAllGameObjects(); //find and curate list of all viewable faces
        ConfigureGameObjects(allGameObj);//Randomly assigns colour
        List<Polygon> myPolygons = FindFacePolygons(allGameObj, null);//finds all outside edges of shape using shared edges and area methods
        List<Edge[]> matchingEdges = FindMatchingEdges(ref myPolygons); //edits ref myPolygons to just the inner polygons by finding the matching edges
        foreach (GameObject go in allGameObj)
            go.transform.parent = null;
        reSizeShape(ref myPolygons);
        foreach (MyHinge hinge in uniqueHinges)
        {
            hinge.updateGo();
        }
        print("Number of hinges: " + matchingEdges.Count);
        print(myPolygons[0].EdgeList.Count + "-gon");



        squareLength = ((uniqueHinges[0].axisPointA - uniqueHinges[0].axisPointB).magnitude);

        Time.fixedDeltaTime = 0.01f;
        //c and angle across from it
        hingeC = uniqueHinges.Where(hinge => hinge.go1.name.Contains("c") && hinge.go2.name.Contains("c")).First(); //hopefully there is only 1
        flapA = uniqueHinges.Where(hinge => hinge.go1.name.Contains("a") && hinge.go2.name.Contains("a")).First();
        flapB = uniqueHinges.Where(hinge => hinge.go1.name.Contains("b") && hinge.go2.name.Contains("b")).First();
        hingeD = uniqueHinges.Where(hinge => hinge.go1.name.Contains("d") && hinge.go2.name.Contains("d")).First();

        interiorHinges.Add(hingeC);
        foreach (MyHinge hinge in uniqueHinges)
        {
            if (hinge.go1.name.Contains("opp") && hinge.go2.name.Contains("opp")) //helper hinge...not getting triggered
            {
                interiorHinges.Add(hinge);
            }
        }
        //@FIXME adding nulls here
        hingeC.connectedHinges.Add(flapA);
        hingeC.connectedHinges.Add(flapB);




    }
    private void Update()
    {

    }
    void FixedUpdate()
    {
        foreach (MyHinge item in uniqueHinges)
        {
            item.Draw();
        }

        //move bound condition logic into here to decide wheather to call angleD or angle C
       // UpdateAngleA(.1f);
        //only called if C is at zero
        UpdateAngleC(.1f);

    }
    public void UpdateAngleA(float deg)
    {
        UpdateFlap( deg, flapA);
    }
    public void UpdateAngleB(float deg)
    {
        UpdateFlap( deg, flapB);
    }
    private void UpdateFlap(float deg, MyHinge hinge)
    {
        //figure out which side
        if (hinge.go1.name.Equals("abcd"))
            MyHinge.lockedFaces.Add(hinge.polygon1);
        else
            MyHinge.lockedFaces.Add(hinge.polygon2);
        hinge.updateAngle(deg);
        MyHinge.lockedFaces.RemoveAt(MyHinge.lockedFaces.Count - 1); //removes the polyygon we just added

    }
    void UpdateAngleD(float deg)
    {
        hingeC.updateAngle(-deg);
        interiorHinges[1].updateAngle(deg);
        
    }

    float UpdateAngleC(float deg) //distance apart is length*cos(pheta/2)-> dX/dPheta = length*-sin(pheta/2) *.5
                                  //double translateAmmount = -1* (-.5 * squareLength * Math.Sin(Mathf.Deg2Rad*1*deg/2));

    //if the angle of c is either 0 or 180 then we dont want to call the translate function
    //I will have interior angles[0] always be c
    {
//TODO: fix logic here and put in main update instead of in angle C
        float angleC = hingeC.updatedAngle;
        //print(angleC);
        if (angleC + deg > 360) 
        {
            print("BOUND CONDITION 1");
            deg = 360 - angleC;
        }
        if (angleC + deg < 0)
        {
            print("BOUND CONDITION 2");
            deg = 0 - angleC;
        }

        if (angleC + deg > 180) //this rotation looks slightly wonky the edges are not perfectly lined up somehow
        { //the interior hinges must preform opposite movements

            hingeC.updateAngle(deg);
            interiorHinges[1].updateAngle(-deg);
        }
        else
        {
            foreach (MyHinge hinge in interiorHinges)
            {
                hinge.updateAngle(deg);
                float dX = (float)(squareLength * Math.Cos(Mathf.Deg2Rad * hinge.updatedAngle / 2)) - (float)(squareLength * Math.Cos(Mathf.Deg2Rad * (hinge.updatedAngle - deg) / 2));
                if (angleC > 1 && angleC < 179)
                {
                    hinge.TranslateHinge(dX);
                    print("HERE");
                }
            }
        }
        return interiorHinges[0].updatedAngle;
    }

  

    /// <summary>
    ///Returns a list of all game objects under the parented object <parentModel> (parent object not included)
    /// </summary>
    GameObject[] FindAllGameObjects()
    {
        //TODO: autounpack doesnt appear to be working any more
        GameObject[] gameObjectArray = new GameObject[parentModel.transform.childCount];
        if (!autoUnpack)
        {
            for (int i = 0; i < parentModel.transform.childCount; i++)
            {
                GameObject child = parentModel.transform.GetChild(i).gameObject;
                gameObjectArray[i] = child;
            }

            return gameObjectArray;
        }

       // PrefabUtility.UnpackPrefabInstance(parentModel, PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);
        int childCount = parentModel.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            gameObjectArray[i] = parentModel.transform.GetChild(i).transform.GetChild(0).transform.GetChild(0).gameObject;
            
        }
        return gameObjectArray;
        
    }
    /// <summary>
    /// applies ridgid body, enables is kinematic,applys random colors 
    /// </summary>
    /// <param name="allGameObjects"> allGameObject to be configured </param>
    void ConfigureGameObjects(GameObject[] allGameObjects) //TODO: have an assigned list that way colors dont get reused
    {
        
        //bool first = true; //we want the first one to be kinematic such that it stays in place (equivalent of grounding in fusion360)

        foreach (GameObject go in allGameObjects)
        { 
            var colorChange = go.GetComponent<Renderer>(); //randomizing the color attached to get easy to view multicolor faces
            colorChange.material.SetColor("_Color", UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
        }
    }

    /// <summary>
    /// Finds all the outside edges of a given face 2*ngon (front and back)
    /// </summary>
    /// <param name="GameObjects"></param>
    /// <param name="collider"></param> Used to skip half the verticies on a two faced plane to aid in making hinges
    /// <returns></returns>
    List<Polygon> FindFacePolygons(GameObject[] GameObjects, List<Polygon> myPolygons)
    {
        Mesh mesh;
        var facePolygons = new List<Polygon>();
        for (int i = 0; i < GameObjects.Length; i++) //foreach game object
        {
            mesh = GameObjects[i].GetComponent<MeshFilter>().mesh;
            Vector3[] localVertices = mesh.vertices;
            Vector3[] worldVertices = new Vector3[localVertices.Length];

            int k = 0;
            foreach (Vector3 vert in localVertices) //Convert all local verts to world verts
            {
                worldVertices[k] = GameObjects[i].transform.TransformPoint(vert);
                k++;
            }
            
            int[] triangles = mesh.GetTriangles(0);
            List<Triangle> worldTriangles = new List<Triangle>();
            int loopStop = triangles.Length;
            for (int j = 0; j < loopStop - 2; j += 3) //all triangles in world space
            {
                worldTriangles.Add(new Triangle(worldVertices[triangles[j]], worldVertices[triangles[j + 1]], worldVertices[triangles[j + 2]], GameObjects[i]));
            }
            
            List<Edge> allEdges = FindOutsideEdges(worldTriangles); //find all the edges of a shape
            List<Polygon> realPolygons = FindConnectedEdges(allEdges); //this list should always be of size two

            foreach (Polygon poly in realPolygons) 
            {
                facePolygons.Add(poly);
            }

        }
        return facePolygons;
    }
    List<Edge> FindOutsideEdges(List<Triangle> worldTriangles)
    {
        List<Edge> allEdges = new List<Edge>();
        //I am going to iterate through every single edge and find which edge is unique
        int sideTriangleCount = 0;
        for (int i = 0; i < worldTriangles.Count; i++) //List of all edges
        {
            if (worldTriangles[i].Area < sideArea) //get rid of the side edges
            {
                sideTriangleCount++;
                continue;
            }
            foreach (Edge e in worldTriangles[i])
            {
                allEdges.Add(e);
            }
        }
        for (int j = 0; j < allEdges.Count; j++) //if a duplicate exists, remove both as they are interior triangles
        {
            Edge findMatch = allEdges[j];
            for (int k = j + 1; k < allEdges.Count; k++)
            {
                if (findMatch.Equals(allEdges[k]))
                {
                    while (allEdges.Contains(findMatch))
                        allEdges.Remove(findMatch);
                    j--;
                    break;
                }
            }
        }
        return allEdges; 
    }

    /// <summary>
    /// Iterates through all the edges to create polygon objects from scratch by seeing if they have a connected vertex
    /// </summary>
    /// <param name="allEdges"></param>
    /// <returns></returns>
    List<Polygon> FindConnectedEdges(List<Edge> allEdges)
    {
        var finalizedPolygons = new List<Polygon>();
        while (allEdges.Count != 0)
        {
            Polygon myPolygon = new Polygon();
            myPolygon.EdgeList.Add(allEdges[0]);

            for (int j = 1; j < allEdges.Count; j++) //build up polygon
            {
                foreach (Edge e in myPolygon)
                {
                    if (e.isConnected(allEdges[j]))
                    {
                        myPolygon.EdgeList.Add(allEdges[j]);
                        break;
                    }
                }
            }
            foreach (Edge edge in myPolygon)
            {
                allEdges.Remove(edge);
            }
            finalizedPolygons.Add(myPolygon);
        }
        return finalizedPolygons;
    }

    /// <summary>
    /// Finds matching edges using global hingeTolerance, and finding edges that match in length. This also determines which polygons are inward facing replacing the old findInside method.
    /// </summary>
    /// <param name="realPolygons"> List of finalized polygons </param>
    List<Edge[]> FindMatchingEdges(ref List<Polygon> realPolygons) 
    {
        uniqueHinges = new List<MyHinge>();
        //index out of bounds on first iteration....realPolygons has bad input
        var returnList = new List<Edge[]>();
        HashSet<Polygon> insideFacePolygons = new HashSet<Polygon>(); //to avoid duplicates
        for (int i = 0; i < realPolygons.Count; i++)//pick a shape
        {
            for (int j = i + 1; j < realPolygons.Count; j++)//check other shapes
            {
                foreach (Edge edge1 in realPolygons[i])
                {
                    foreach (Edge edge2 in realPolygons[j])
                    {
                        if ((((edge1.vertex1 - edge2.vertex1).magnitude < hingeTolerance && (edge1.vertex2 - edge2.vertex2).magnitude < hingeTolerance) ||
                            ((edge1.vertex1 - edge2.vertex2).magnitude < hingeTolerance && (edge1.vertex2 - edge2.vertex1).magnitude < hingeTolerance))) 
                        {
                            //if (Vector3.Cross(realPolygons[i].getNormal(),realPolygons[j].getNormal()).Equals(Vector3.zero)) //If vectors are parallel their cross product should be 0
                            //    print("shared Normal"); //probably never gets called because of shared edges algorithm
                            returnList.Add(new Edge[] { edge1, edge2 });
                            insideFacePolygons.Add(realPolygons[i]);
                            insideFacePolygons.Add(realPolygons[j]);

                            MyHinge hinge = new MyHinge(realPolygons[i], realPolygons[j], edge1.vertex1, edge1.vertex2);
                            
                            if(!uniqueHinges.Contains(hinge)) //slow but oh well
                                uniqueHinges.Add(hinge);
                        }
                    }
                }

            }
        }
        realPolygons = insideFacePolygons.ToList();
        return returnList;
    }
    void reSizeShape(ref List<Polygon> myPolygons) // List<Edge> edges
    {
        print(myPolygons.Count);
        foreach (Polygon p in myPolygons)
        {
            List<Vector3> verticiesList = p.getVerticies();
            print("verts: "+ verticiesList.Count);
            int[] triangles = CreateTriangleArray(p,verticiesList).ToArray();
            Vector3[] vertices = p.getVerticies().ToArray();
            GameObject newGo = new GameObject(p.EdgeList[0].go.name, typeof(MeshFilter), typeof(MeshRenderer));
            var oldPlace = p.EdgeList[0].go;
            Mesh mesh = new Mesh();
            newGo.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            print(triangles);
            foreach (Edge e in p)
            {
                e.go = newGo;
            }
            var colorChange = newGo.GetComponent<Renderer>(); //randomizing the color attached to get easy to view multicolor faces
            colorChange.material.SetColor("_Color", UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
            Destroy(oldPlace);

        }
    }

    List<int> CreateTriangleArray(Polygon p, List<Vector3> verticies) //Assuming winding order does not matter for colliders -- if needed uncomment out the flipped code to make figure double sided
    {
        List<Triangle> tList = p.createTriangles();
        List<int> indexList = new List<int>();
        int indexTracker = 0;
        foreach (Triangle t in tList)
        {
            foreach (Edge e in t)
            {
                indexList.Add(verticies.IndexOf(e.vertex1));

                indexTracker++;
            }
        }

        //double sided does not quite appear to be working properly as of yet @FIXME

        int[] flipped = new int[indexList.Count];
        Array.Copy(indexList.ToArray(), flipped, indexList.Count);
        Array.Reverse(flipped, 0, indexList.Count);
        var combined = new int[2 * indexList.Count];
        indexList.CopyTo(combined, 0);
        flipped.CopyTo(combined, indexList.Count);
        
        return combined.ToList<int>();
    }

    double CalculateVolume(List<Triangle> faces)
    {
        return 0.0;
    }
    void CreateLabels()
    {
        return;
    }
    

}
 
