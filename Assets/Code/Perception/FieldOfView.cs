using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Perception))]
public class FieldOfView : MonoBehaviour
{

    // The Radius (distance) the agent can see
    public float ViewRadius;

 
    // The angle (in degrees) the agent can see. Range of 0 to 360
    [Range(0, 360)]
    public float ViewAngle;


    // The layer of target objects (what we're trying to sense)
    public LayerMask TargetLayer;


    // The layer of obstacles (things that should block our line of sight)
    public LayerMask ObstacleLayer;


    // List of visible targets is updated every frame
    public List<Transform> visibleTargets = new List<Transform>();


    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;
    public bool DrawFOV = true;

    public GameObject thisObject; //stores the value of the object it is attached to so it cannot see itself
    private void Start()
    {
        //Mesh drawing initialisations
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        thisObject = this.gameObject;
        //Check for visible targets every 0.2 seconds, which is approx. the average human response time to stimulus
        InvokeRepeating("FindVisibleTargets", 0.2f, 0.2f);
    }

    void FindVisibleTargets()
    {
        //Clear the current visible targets
        visibleTargets.Clear();

        //Do simple sphere collision check for nearby targets
        Collider[] targets = Physics.OverlapSphere(transform.position, ViewRadius, TargetLayer);

        //Iterate through each target
        foreach (Collider target in targets)
        {
            //Get direction and magnitude to target
            Vector3 ToTarget = (target.transform.position - transform.position);

            //Normalize so we have direction without magnitude
            Vector3 ToTargetNormalized = ToTarget.normalized;

            
            if (Vector3.Angle(transform.forward, ToTargetNormalized) < ViewAngle / 2 //Check if the target is within our FoV
                && !Physics.Raycast(transform.position, ToTargetNormalized, ToTarget.magnitude, ObstacleLayer) && // then do the raycast to determine LoS
                target.gameObject.tag == "Player" || target.gameObject.tag == "Enemy_AI" && // and check if the object has the right tag
                target.gameObject != thisObject) // and that it is not the object this script is attached to
            {
                //I see you!
                visibleTargets.Add(target.transform);
            }
        }

        //Add memory record to our perception system
        Perception percept = GetComponent<Perception>();

        percept.ClearFoV();
        foreach(Transform target in visibleTargets)
        {
              percept.AddMemory(target.gameObject);
            
            
        }
    }

    private void LateUpdate()
    {
        //Draw or hide the FoV
        if (DrawFOV)
        {
            viewMeshFilter.gameObject.SetActive(true);
            DrawFieldOfView();

            foreach(Transform target in visibleTargets)
            {
                Debug.DrawLine(transform.position, target.position, Color.red);
            }
        }
        else
        {
            viewMeshFilter.gameObject.SetActive(false);
        }
    }

    //This part draws the FoV for debug purposes
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(ViewAngle * meshResolution);
        float stepAngleSize = ViewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - ViewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }

            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, ViewRadius, ObstacleLayer))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * ViewRadius, ViewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

}
